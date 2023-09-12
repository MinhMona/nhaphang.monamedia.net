using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.Configurations;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class OrderService : DomainService<Order, OrderSearch>, IOrderService
    {
        protected readonly IAppDbContext Context;
        protected readonly IMainOrderService mainOrderService;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IUserInGroupService userInGroupService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly ISendNotificationService sendNotificationService;


        public OrderService(IAppUnitOfWork unitOfWork, IMapper mapper, IServiceProvider serviceProvider, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();

        }

        protected override string GetStoreProcName()
        {
            return "Orders_GetPagingData";
        }

        public override async Task<bool> UpdateAsync(Order item)
        {
            //item.PricePromotion = item.PriceOrigin;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var currentUser = LoginContext.Instance.CurrentUser;
                    unitOfWork.Repository<Order>().Update(item);
                    //Lịch sử thay đổi của đơn hàng
                    await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(item.HistoryOrderChanges);
                    await unitOfWork.SaveAsync();

                    #region Tính lại tiền
                    var mainOrder = await mainOrderService.GetByIdAsync(item.MainOrderId ?? 0);
                    var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == item.UID).FirstOrDefaultAsync();

                    decimal insurancePercent = Convert.ToDecimal(mainOrder.InsurancePercent);
                    decimal currency = Convert.ToDecimal(mainOrder.CurrentCNYVN);

                    //Tính tiền
                    decimal? priceVND = 0;
                    decimal? priceCNY = 0;
                    int counprosMore10 = 0;
                    int counprosLes10 = 0;
                    foreach (var order in mainOrder.Orders)
                    {
                        decimal? uPriceBuy = 0;
                        decimal? uPriceVN = 0;

                        if (order.PricePromotion > 0 && order.PricePromotion < order.PriceOrigin)
                        {
                            uPriceBuy = order.PricePromotion;
                            uPriceVN = order.PricePromotion * currency;
                        }
                        else
                        {
                            uPriceBuy = order.PriceOrigin;
                            uPriceVN = order.PriceOrigin * currency;
                        }

                        priceCNY += (uPriceBuy * order.Quantity);
                        priceVND += (uPriceVN * order.Quantity);

                        //Này dành cho phí kiểm hàng
                        if (mainOrder.IsCheckProduct == true)
                        {
                            if (order.PriceOrigin >= 10)
                                counprosMore10 += order.Quantity ?? 0;
                            else
                                counprosLes10 += order.Quantity ?? 0;
                        }
                    }

                    mainOrder.PriceCNY = priceCNY;
                    mainOrder.PriceVND = priceVND;

                    //Tính lại phí mua hàng
                    var userLevel = await unitOfWork.Repository<UserLevel>().GetQueryable().Where(x => x.Id == user.LevelId).FirstOrDefaultAsync();

                    decimal lessDeposit = userLevel == null ? 0 : (userLevel.LessDeposit ?? 0);
                    if (user.Deposit > 0)
                        lessDeposit = user.Deposit ?? 0;

                    var cKFeeBuyPro = userLevel == null ? 0 : (userLevel.FeeBuyPro ?? 0);

                    decimal serviceFee = 0;
                    decimal feebpnotdc = 0;
                    var feeBuyPro = await unitOfWork.Repository<FeeBuyPro>().GetQueryable().Where(x => mainOrder.PriceVND >= x.PriceFrom && mainOrder.PriceVND <= x.PriceTo).FirstOrDefaultAsync();
                    if (feeBuyPro != null)
                    {
                        decimal feePercent = feeBuyPro.FeePercent > 0 ? (feeBuyPro.FeePercent ?? 0) : 0;
                        serviceFee = feePercent / 100;
                    }

                    if (user.FeeBuyPro > 0)
                        feebpnotdc = (mainOrder.PriceVND ?? 0) * (user.FeeBuyPro ?? 0) / 100;
                    else
                        feebpnotdc = (mainOrder.PriceVND ?? 0) * serviceFee;

                    decimal subfeebp = feebpnotdc * (cKFeeBuyPro / 100);
                    decimal feebp = feebpnotdc - subfeebp;

                    //Phí mua hàng tối thiểu
                    var conf = await unitOfWork.Repository<Entities.Configurations>()
                        .GetQueryable()
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync();
                    feebp = feebp > (conf.FeeBuyProMin ?? 0) ? feebp : (conf.FeeBuyProMin ?? 0);

                    mainOrder.FeeBuyPro = feebp;
                    mainOrder.CKFeeBuyPro = Math.Round(feebp / currency, 1);

                    //Tính lại phí kiểm hàng
                    if (mainOrder.IsCheckProduct == true)
                    {
                        mainOrder.IsCheckProductPrice = 0;
                        var feeCheckProducts = await unitOfWork.Repository<FeeCheckProduct>().GetQueryable().ToListAsync();
                        if (feeCheckProducts.Any())
                        {
                            var feeCheckProduct = new List<FeeCheckProduct>();
                            if (counprosMore10 > 0)
                            {
                                feeCheckProduct = await unitOfWork.Repository<FeeCheckProduct>().GetQueryable().Where(x => !x.Deleted && x.Type == 1).ToListAsync();
                                foreach (var jtem in feeCheckProduct)
                                {
                                    if (counprosMore10 >= jtem.AmountFrom && counprosMore10 <= jtem.AmountTo)
                                        mainOrder.IsCheckProductPrice += jtem.Fee * counprosMore10;
                                }
                            }

                            if (counprosLes10 > 0)
                            {
                                feeCheckProduct = await unitOfWork.Repository<FeeCheckProduct>().GetQueryable().Where(x => !x.Deleted && x.Type == 2).ToListAsync();
                                foreach (var jtem in feeCheckProduct)
                                {
                                    if (counprosLes10 >= jtem.AmountFrom && counprosLes10 <= jtem.AmountTo)
                                        mainOrder.IsCheckProductPrice += jtem.Fee * counprosLes10;
                                }
                            }
                            mainOrder.IsCheckProductPriceCNY = mainOrder.IsCheckProductPrice / currency;
                        }
                    }
                    else
                        mainOrder.IsCheckProductPrice = mainOrder.IsCheckProductPriceCNY = 0;

                    //Tính lại phí bảo hiểm
                    if (mainOrder.IsInsurance == true)
                    {
                        mainOrder.InsuranceMoney = (mainOrder.PriceVND * mainOrder.InsurancePercent) / 100; //Bảo hiểm = tiền mua hàng * %
                    }
                    //Tính tổng tiền VNĐ
                    mainOrder.TotalPriceVND = (mainOrder.FeeWeight ?? 0)
                        + (mainOrder.FeeShipCN ?? 0)
                        + (mainOrder.FeeBuyPro ?? 0)
                        + (mainOrder.IsCheckProductPrice ?? 0)
                        + (mainOrder.IsPackedPrice ?? 0)
                        + (mainOrder.IsFastDeliveryPrice ?? 0)
                        + (mainOrder.PriceVND ?? 0)
                        + (mainOrder.Surcharge ?? 0)
                        + (mainOrder.InsuranceMoney ?? 0);

                    //Tính lại tiền phải cọc
                    decimal totalForDeposit = (priceVND + feebp) ?? 0;
                    mainOrder.AmountDeposit = lessDeposit > 0 ? (totalForDeposit * lessDeposit / 100) : totalForDeposit;

                    //Nếu đã đặt cọc thì phải trả phí lại cho người ta (Hoàn tiền - Giống MainOrderService)
                    if (mainOrder.Deposit > 0)
                    {
                        if (mainOrder.Deposit > mainOrder.AmountDeposit)
                        {
                            decimal? moneyLeft = mainOrder.Deposit - mainOrder.AmountDeposit;

                            //Cập nhật trừ số tiền trong account
                            user.Wallet += moneyLeft;
                            unitOfWork.Repository<Users>().Update(user);

                            //Thêm lịch sử thanh toán mua hộ
                            await unitOfWork.Repository<PayOrderHistory>().CreateAsync(new PayOrderHistory()
                            {
                                MainOrderId = mainOrder.Id,
                                UID = user.Id,
                                Status = (int?)StatusPayOrderHistoryContants.HoanTienSanPham,
                                Amount = moneyLeft,
                                Type = (int?)TypePayOrderHistoryContants.ViDienTu
                            });

                            //Thêm lịch sử của ví tiền
                            await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                            {
                                UID = user.Id,
                                MainOrderId = mainOrder.Id,
                                Amount = moneyLeft,
                                Content = string.Format("Sản phẩm đơn hàng: {0} {1}", mainOrder.Id, item.ProductStatus == 2 ? "hết hàng" : "giảm giá"),
                                MoneyLeft = user.Wallet,
                                Type = (int?)DauCongVaTru.Cong,
                                TradeType = (int?)HistoryPayWalletContents.HoanTienSanPham,
                            });
                            mainOrder.Deposit = mainOrder.AmountDeposit;
                        }
                        else
                        {
                            mainOrder.AmountDeposit = mainOrder.Deposit;
                        }
                    }
                    else
                    {
                        mainOrder.Deposit = 0;
                    }

                    mainOrder.IsUpdatePrice = true;
                    mainOrder.AmountDeposit = Math.Round((mainOrder.AmountDeposit ?? 0), 0);
                    mainOrder.TotalPriceVND = Math.Round((mainOrder.TotalPriceVND ?? 0), 0);
                    unitOfWork.Repository<MainOrder>().Update(mainOrder);
                    await unitOfWork.SaveAsync();
                    #endregion
                    unitOfWork.Repository<MainOrder>().Detach(mainOrder);
                    await dbContextTransaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
