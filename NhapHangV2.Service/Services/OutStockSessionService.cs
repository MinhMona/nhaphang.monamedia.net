using AutoMapper;
using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NhapHangV2.Entities.Search;
using NhapHangV2.Interface.DbContext;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Extensions;
using static NhapHangV2.Utilities.CoreContants;
using Microsoft.EntityFrameworkCore;

namespace NhapHangV2.Service.Services
{
    public class OutStockSessionService : DomainService<OutStockSession, OutStockSessionSearch>, IOutStockSessionService
    {
        protected readonly IAppDbContext Context;
        protected readonly IUserService userService;
        protected readonly IMainOrderService mainOrderService;
        protected readonly ITransportationOrderService transportationOrderService;
        public OutStockSessionService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            userService = serviceProvider.GetRequiredService<IUserService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            transportationOrderService = serviceProvider.GetRequiredService<ITransportationOrderService>();
        }

        public override async Task<bool> CreateAsync(OutStockSession item)
        {
            DateTime currentDate = DateTime.Now;

            List<int> mainOrderIds = new List<int>();
            List<int> transportationOrderIds = new List<int>();

            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);

                    if (user.UserGroupId == (int)PermissionTypes.Admin
                        || user.UserGroupId == (int)PermissionTypes.Manager
                        || user.UserGroupId == (int)PermissionTypes.VietNamWarehouseManager)
                    {

                        item.Status = 1;
                        await unitOfWork.Repository<OutStockSession>().CreateAsync(item);
                        await unitOfWork.SaveAsync();

                        if (item.SmallPackageIds == null || item.SmallPackageIds.Count == 0)
                            throw new KeyNotFoundException("Không tìm thấy các kiện");
                        decimal? totalPay = 0; // Tổng tiền thanh toán của phiên xuất kho

                        foreach (var id in item.SmallPackageIds)
                        {
                            decimal? payInWarehouse = 0;

                            var smallPackage = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(e => !e.Deleted && e.Id == id).FirstOrDefaultAsync();
                            if (smallPackage == null)
                                throw new KeyNotFoundException("Có kiện không tồn tại hoặc đã bị xóa");

                            var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.MainOrderId).FirstOrDefaultAsync();
                            var trans = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();

                            if (mainOrder != null && trans != null)
                                throw new KeyNotFoundException("Đơn có vấn đề");

                            //Tính phí lưu kho
                            //var inWarehousePrice = await unitOfWork.Repository<InWareHousePrice>().GetQueryable().Where(e => !e.Deleted &&
                            //       smallPackage.PayableWeight > e.WeightFrom && smallPackage.PayableWeight <= e.WeightTo).FirstOrDefaultAsync();

                            //decimal? maxDay = inWarehousePrice == null ? 0 : inWarehousePrice.MaxDay;
                            //decimal? payPerDay = inWarehousePrice == null ? 0 : inWarehousePrice.PricePay;

                            //if (smallPackage.TotalDateInLasteWareHouse - maxDay > 0)
                            //    payInWarehouse = (smallPackage.TotalDateInLasteWareHouse - maxDay) * payPerDay * smallPackage.PayableWeight;

                            smallPackage.DateOutWarehouse = currentDate;

                            if (mainOrder != null)
                            {
                                if (!mainOrderIds.Contains(mainOrder.Id))
                                {
                                    mainOrder.FeeInWareHouse = payInWarehouse;
                                    totalPay += mainOrder.TotalPriceVND + payInWarehouse - mainOrder.Deposit;
                                }

                                //Detach
                                if (!mainOrderIds.Contains(mainOrder.Id))
                                    unitOfWork.Repository<MainOrder>().Update(mainOrder);
                                mainOrderIds.Add(mainOrder.Id);
                            }
                            else if (trans != null)
                            {
                                trans.WarehouseFee = payInWarehouse;
                                if (trans.Status != (int)StatusGeneralTransportationOrder.DaThanhToan)
                                    totalPay += trans.TotalPriceVND + payInWarehouse;

                                //Detach
                                if (!transportationOrderIds.Contains(trans.Id))
                                    unitOfWork.Repository<TransportationOrder>().Update(trans);
                                transportationOrderIds.Add(trans.Id);
                            }
                            else
                                throw new AppException("Kiện trôi nổi không thể xuất kho");

                            unitOfWork.Repository<SmallPackage>().Update(smallPackage);

                            if (mainOrder != null)
                            {
                                await unitOfWork.Repository<OutStockSessionPackage>().CreateAsync(new OutStockSessionPackage
                                {
                                    OutStockSessionId = item.Id,
                                    SmallPackageId = smallPackage.Id,
                                    OrderTransactionCode = smallPackage.OrderTransactionCode,
                                    MainOrderID = mainOrder.Id
                                });
                            }
                            else if (trans != null)
                            {
                                await unitOfWork.Repository<OutStockSessionPackage>().CreateAsync(new OutStockSessionPackage
                                {
                                    OutStockSessionId = item.Id,
                                    SmallPackageId = smallPackage.Id,
                                    OrderTransactionCode = smallPackage.OrderTransactionCode,
                                    TransportationID = trans.Id
                                });
                            }

                            await unitOfWork.SaveAsync();
                        }

                        item.TotalPay = totalPay;
                        if (totalPay == 0)
                        {
                            item.Status = 2;
                        }
                        unitOfWork.Repository<OutStockSession>().Update(item);
                        await unitOfWork.SaveAsync();
                        await dbContextTransaction.CommitAsync();
                        return true;
                    }
                    else
                        throw new InvalidCastException("Không có quyền gán đơn");
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

        protected override string GetStoreProcName()
        {
            return "OutStockSession_GetPagingData";
        }

        public override async Task<OutStockSession> GetByIdAsync(int id)
        {
            var outStock = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (outStock == null)
                return null;
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Id == outStock.UID).FirstOrDefaultAsync();
            if (user != null)
            {
                outStock.UserName = user.UserName;
                outStock.UserPhone = user.Phone;
                outStock.UserWallet = user.Wallet;
            }
            var outStockPackages = await unitOfWork.Repository<OutStockSessionPackage>().GetQueryable().Where(e => !e.Deleted && e.OutStockSessionId == outStock.Id).OrderByDescending(o => o.Id).ToListAsync();
            if (outStockPackages == null || !outStockPackages.Any())
                throw new KeyNotFoundException("Không tìm thấy chi tiết đơn xuất kho");

            var oldMainOrderCals = new List<int>();
            var mainOrderIds = new List<int>();
            decimal? totalPaid = 0;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var outStockPackage in outStockPackages)
                    {
                        var smallPackage = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(e => !e.Deleted && e.Id == outStockPackage.SmallPackageId).FirstOrDefaultAsync();
                        if (smallPackage == null)
                            throw new KeyNotFoundException("Không tìm thấy SmallPackage");
                        if (smallPackage.MainOrderId > 0)
                        {
                            var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.MainOrderId).FirstOrDefaultAsync();
                            decimal? totalMustPay = mainOrder.TotalPriceVND + mainOrder.FeeInWareHouse;
                            if (smallPackage.IsPayment != null)
                            {
                                if (smallPackage.IsPayment.Value)
                                    outStockPackage.IsPayment = true;
                            }

                            outStock.TotalWeight += smallPackage.Weight;
                            outStock.ExchangeWeight += smallPackage.Volume;
                            outStock.PayableWeight += smallPackage.PayableWeight;
                            outStock.TotalWarehouseFee += mainOrder.FeeInWareHouse;

                            outStockPackage.WarehouseFee = mainOrder.FeeInWareHouse;
                            outStockPackage.TotalLeftPay = smallPackage.TotalPrice;
                            outStockPackage.SmallPackage = smallPackage;

                            outStockPackage.OrderRemaining = Math.Round((mainOrder.TotalPriceVND ?? 0) - (mainOrder.Deposit ?? 0), 0);
                            await unitOfWork.SaveAsync();
                            unitOfWork.Repository<MainOrder>().Detach(mainOrder);
                        }
                        else if (smallPackage.TransportationOrderId > 0)
                        {
                            var transportOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();
                            if (transportOrder.Status == (int)StatusGeneralTransportationOrder.DaThanhToan ||
                                transportOrder.Status == (int)StatusGeneralTransportationOrder.DaHoanThanh)
                            {
                                outStockPackage.IsPayment = true;
                            }
                            outStock.TotalWeight += smallPackage.Weight;
                            outStock.ExchangeWeight += smallPackage.Volume;
                            outStock.PayableWeight += smallPackage.PayableWeight;
                            outStock.TotalWarehouseFee += transportOrder.WarehouseFee;
                            outStockPackage.WarehouseFee = transportOrder.WarehouseFee;
                            outStockPackage.TotalLeftPay = smallPackage.TotalPrice;
                            totalPaid += transportOrder.TotalPriceVND;

                            outStockPackage.SmallPackage = smallPackage;
                        }
                        else
                            throw new AppException("Đơn này là đơn trôi nổi");
                    }

                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }

            outStock.OutStockSessionPackages = outStockPackages;
            //outStock.TotalPay = totalPaid;
            return outStock;
        }

        public async Task<bool> UpdateStatus(int id, int status, bool isPaymentWallet)
        {
            //Còn cập nhật Tên người nhận hàng, Ghi chú nữa đó OutStockSession
            DateTime currentDate = DateTime.Now;
            var userName = LoginContext.Instance.CurrentUser.UserName;
            var item = await this.GetByIdAsync(id);
            if (item == null)
                throw new KeyNotFoundException("Item không tồn tại");

            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Id == item.UID).FirstOrDefaultAsync();
            if (user == null)
                throw new KeyNotFoundException("User không tồn tại");

            var wallet = user.Wallet;

            //Tổng tiền thanh toán
            decimal? totalMustPay = item.TotalPay;
            if (isPaymentWallet && wallet < totalMustPay)
                throw new AppException("Không đủ tiền trong tài khoản! Vui lòng nạp thêm tiền");

            switch (status)
            {
                case 2: //Thanh toán

                    var mainOrderIds = new List<int>();

                    using (var dbContextTransaction = Context.Database.BeginTransaction())
                    {
                        try
                        {
                            if (!isPaymentWallet) //Thanh toán trực tiếp (nạp tiền vô rồi trừ)
                            {
                                user.Wallet += totalMustPay;

                                await unitOfWork.Repository<AdminSendUserWallet>().CreateAsync(new AdminSendUserWallet()
                                {
                                    UID = item.UID,
                                    Amount = totalMustPay,
                                    Updated = DateTime.UtcNow.AddHours(7),
                                    UpdatedBy = userName,
                                    Status = (int)WalletStatus.DaDuyet,
                                    BankId = 100, //Đặt đại
                                    TradeContent = string.Format("{0} đã được nạp tiền vào tài khoản", user.UserName),
                                });

                                await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                                {
                                    UID = user.Id,
                                    MainOrderId = 0,
                                    Amount = totalMustPay,
                                    Content = string.Format("{0} đã được nạp tiền vào tài khoản.", user.UserName),
                                    MoneyLeft = user.Wallet,
                                    Type = (int?)DauCongVaTru.Cong,
                                    TradeType = (int?)HistoryPayWalletContents.AdminChuyenTien,
                                });
                            }

                            user.Wallet -= totalMustPay;
                            //Lịch sử của ví
                            await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                            {
                                UID = user.Id,
                                MainOrderId = 0,
                                Amount = totalMustPay,
                                Content = string.Format($"{user.UserName} đã thanh toán phiên xuất kho #{id}"),
                                MoneyLeft = user.Wallet,
                                Type = (int?)DauCongVaTru.Tru,
                                TradeType = (int?)HistoryPayWalletContents.ThanhToanHoaDon,
                            });
                            unitOfWork.Repository<Users>().Update(user);

                            foreach (var outStockSessionPackage in item.OutStockSessionPackages)
                            {
                                var smallPackage = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(e => !e.Deleted && e.Id == outStockSessionPackage.SmallPackageId).AsNoTracking().FirstOrDefaultAsync();
                                if (smallPackage == null)
                                    throw new KeyNotFoundException("Không tìm thấy SmallPackage");
                                var mainOrder = await mainOrderService.GetByIdAsync(smallPackage.MainOrderId ?? 0);
                                if (mainOrder != null && !mainOrderIds.Contains(mainOrder.Id))
                                    mainOrderIds.Add(mainOrder.Id);
                                if (mainOrder != null)
                                    unitOfWork.Repository<MainOrder>().Detach(mainOrder);

                                var transOrder = await transportationOrderService.GetByIdAsync(smallPackage.TransportationOrderId ?? 0);
                                if (mainOrder == null && transOrder == null)
                                    throw new KeyNotFoundException("Không tìm thấy Đơn mua hộ và đơn ký gửi");

                                if (transOrder != null)
                                {
                                    transOrder.Status = (int?)StatusGeneralTransportationOrder.DaThanhToan;
                                    if (transOrder.PaidDate == null)
                                        transOrder.PaidDate = DateTime.Now;
                                    unitOfWork.Repository<TransportationOrder>().Update(transOrder);
                                    await unitOfWork.SaveAsync();
                                    unitOfWork.Repository<TransportationOrder>().Detach(transOrder);

                                }
                                //smallPackage.Status = (int)StatusSmallPackage.DaThanhToan;
                                smallPackage.IsPayment = true;
                                smallPackage.DateOutWarehouse = currentDate;
                                smallPackage.StaffVNOutWarehouse = userName;
                                unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                                await unitOfWork.SaveAsync();
                            }

                            foreach (var moId in mainOrderIds)
                            {
                                var mo = await mainOrderService.GetByIdAsync(moId);

                                //Lịch sử đơn hàng thay đổi
                                await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                                {
                                    MainOrderId = mo.Id,
                                    UID = LoginContext.Instance.CurrentUser.UserId,
                                    HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng ID là: {1} từ: Đã về kho đích, sang: Khách đã thanh toán.", userName, mo.Id),
                                    Type = (int?)TypeHistoryOrderChange.TienDatCoc
                                });

                                //Lịch sử thanh toán mua hộ
                                await unitOfWork.Repository<PayOrderHistory>().CreateAsync(new PayOrderHistory()
                                {
                                    MainOrderId = mo.Id,
                                    UID = user.Id,
                                    Status = (int?)StatusPayOrderHistoryContants.ThanhToan,
                                    Amount = mo.TotalPriceVND - mo.Deposit,
                                    Type = (int?)DauCongVaTru.Tru
                                });

                                mo.Status = (int?)StatusOrderContants.KhachDaThanhToan;
                                mo.Deposit = mo.TotalPriceVND;
                                mo.PayDate = currentDate;

                                //Detach
                                unitOfWork.Repository<MainOrder>().Update(mo);
                                await unitOfWork.SaveAsync();
                                unitOfWork.Repository<MainOrder>().Detach(mo);

                            }
                            await unitOfWork.Repository<AccountantOutStockPayment>().CreateAsync(new AccountantOutStockPayment()
                            {
                                OutStockSessionID = item.Id,
                                UID = item.UID,
                                TotalPrice = totalMustPay,
                                Note = isPaymentWallet ? "Thanh toán bằng ví điện tử" : "Thanh toán bằng tiền mặt"
                            });

                            item.Type = isPaymentWallet ? 1 : 2;
                            item.Status = 2; //Đã xử lý
                            unitOfWork.Repository<OutStockSession>().Update(item);
                            await unitOfWork.SaveAsync();
                            await dbContextTransaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            await dbContextTransaction.RollbackAsync();
                            throw new Exception(ex.Message);
                        }
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public async Task<bool> DeleteNotPayment(OutStockSession item)
        {
            var listDelete = item.OutStockSessionPackages.Where(e => e.IsPayment == false).ToList();
            //Xóa luôn
            unitOfWork.Repository<OutStockSessionPackage>().Delete(listDelete);
            if (item.OutStockSessionPackages.Count == listDelete.Count) //Nếu bằng nhau thì xóa luôn thằng cha
                unitOfWork.Repository<OutStockSession>().Delete(item);
            else
            {
                item.TotalPay = 0;
                await unitOfWork.Repository<OutStockSession>().UpdateFieldsSaveAsync(item, new System.Linq.Expressions.Expression<Func<OutStockSession, object>>[]
                {
                    x=>x.TotalPay
                });
            }
            await unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> Export(int id)
        {
            DateTime currentDate = DateTime.Now;

            var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
            if (user.UserGroupId == (int)PermissionTypes.Admin
                || user.UserGroupId == (int)PermissionTypes.Manager
                || user.UserGroupId == (int)PermissionTypes.VietNamWarehouseManager
                || user.UserGroupId == (int)PermissionTypes.Accountant)
            {
                var item = await this.GetByIdAsync(id);
                if (item == null)
                    throw new KeyNotFoundException("Không tìm thấy item");
                var mainOrders = new List<MainOrder>();
                var transportationOrders = new List<TransportationOrder>();
                foreach (var OutStockSessionPackage in item.OutStockSessionPackages)
                {
                    if (OutStockSessionPackage.MainOrderID > 0)
                    {
                        var smallPackage = OutStockSessionPackage.SmallPackage;
                        var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.MainOrderId).FirstOrDefaultAsync();
                        if (mainOrder == null)
                            throw new KeyNotFoundException("Không tìm thấy đơn hàng");
                        if (!mainOrders.Any(x => x.Id == mainOrder.Id))
                            mainOrders.Add(mainOrder);

                        //Cập nhật
                        smallPackage.Status = (int)StatusSmallPackage.DaGiao;
                        smallPackage.StaffVNOutWarehouse = user.UserName;
                        smallPackage.DateOutWarehouse = mainOrder.CompleteDate = currentDate;

                        unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<SmallPackage>().Detach(smallPackage);

                    }
                    else if (OutStockSessionPackage.TransportationID > 0)
                    {
                        var smallPackage = OutStockSessionPackage.SmallPackage;
                        var transOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();
                        if (transOrder == null)
                            throw new KeyNotFoundException("Không tìm thấy đơn ký gửi");
                        if (!transportationOrders.Any(x => x.Id == transOrder.Id))
                            transportationOrders.Add(transOrder);
                        //Cập nhật
                        smallPackage.Status = (int)StatusSmallPackage.DaGiao;
                        smallPackage.StaffVNOutWarehouse = user.UserName;
                        smallPackage.DateOutWarehouse = transOrder.DateExport = currentDate;

                        unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<SmallPackage>().Detach(smallPackage);

                    }
                }

                //Cập nhật đơn mua hộ
                foreach (var mainOrder in mainOrders)
                {
                    var smallPackageUpdates = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.MainOrderId == mainOrder.Id).ToListAsync();
                    if (smallPackageUpdates.Any(x => x.Status != (int)StatusSmallPackage.DaGiao))
                        continue;
                    else
                    {
                        //Lịch sử đơn hàng thay đổi
                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                        {
                            MainOrderId = mainOrder.Id,
                            UID = LoginContext.Instance.CurrentUser.UserId,
                            HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng ID là: {1} từ: Khách đã thanh toán, sang: Đã hoàn thành.", LoginContext.Instance.CurrentUser.UserName, mainOrder.Id),
                            Type = (int?)TypeHistoryOrderChange.TienDatCoc
                        });
                        mainOrder.Status = (int)StatusOrderContants.DaHoanThanh;
                        unitOfWork.Repository<MainOrder>().Update(mainOrder);
                    }
                }

                //Cập nhật đơn ký gửi
                foreach (var trans in transportationOrders)
                {
                    var smallPackageUpdates = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.TransportationOrderId == trans.Id).ToListAsync();
                    if (smallPackageUpdates.Any(x => x.Status != (int)StatusSmallPackage.DaGiao))
                        continue;
                    else
                    {
                        trans.Status = (int)StatusGeneralTransportationOrder.DaHoanThanh;
                        if (trans.CompleteDate == null)
                            trans.CompleteDate = DateTime.Now;
                        unitOfWork.Repository<TransportationOrder>().Update(trans);
                    }
                }

                item.Status = 2; //Đã xử lý
                unitOfWork.Repository<OutStockSession>().Update(item);
            }
            else throw new InvalidCastException("Không có quyền xuất kho");

            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
