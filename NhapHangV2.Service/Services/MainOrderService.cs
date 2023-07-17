using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Entities.SQLEntities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Models.ExcelModels;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Utilities;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlParameter = Microsoft.Data.SqlClient.SqlParameter;
using Users = NhapHangV2.Entities.Users;

namespace NhapHangV2.Service.Services
{

    public class MainOrderService : DomainService<MainOrder, MainOrderSearch>, IMainOrderService
    {
        private readonly string[] MainOrderExcelColumns = new string[] { "OrderID", "Username", "Tổng tiền", "Tiền đã trả",
            "Tiền còn lại", "Tiền hàng trên Web", "Phí dịch vụ", "Phí ship TQ", "TPhí kiểm hàng", "Phí đóng gỗ",
            "Phí bảo hiểm", "Phụ phí", "Phí vận chuyển", "Cân nặng","Trạng thái", "NV kinh doanh", "NV đặt hàng",
            "Ngày tạo","Ngày đặt cọc", "Ngày mua hàng", "Ngày về kho TQ", "Ngày về kho VN","Ngày hoàn thành"
        };

        protected readonly IAppDbContext Context;
        protected readonly IOrderShopTempService orderShopTempService;
        protected readonly IUserService userService;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IUserInGroupService userInGroupService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly INotificationTemplateService notificationTemplateService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly ISMSEmailTemplateService sMSEmailTemplateService;
        private readonly IServiceProvider serviceProvider;

        public MainOrderService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            this.serviceProvider = serviceProvider;
            orderShopTempService = serviceProvider.GetRequiredService<IOrderShopTempService>();
            userService = serviceProvider.GetRequiredService<IUserService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            notificationTemplateService = serviceProvider.GetRequiredService<INotificationTemplateService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
            sMSEmailTemplateService = serviceProvider.GetRequiredService<ISMSEmailTemplateService>();
        }
        public async Task<PagedList<MainOrderTool>> GetPagedListOfOrderStaff(MainOrderToolSearch search)
        {
            PagedList<MainOrderTool> pagedList = new PagedList<MainOrderTool>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", search.UID));
            sqlParameters.Add(new SqlParameter("@PageIndex", search.PageIndex));
            sqlParameters.Add(new SqlParameter("@PageSize", search.PageSize));
            sqlParameters.Add(new SqlParameter("@OrderBy", search.OrderBy));
            pagedList = await this.unitOfWork.Repository<MainOrderTool>().ExcuteQueryPagingAsync("OrderOfStaff", sqlParameters.ToArray());
            pagedList.PageIndex = search.PageIndex;
            pagedList.PageSize = search.PageSize;
            return pagedList;
        }

        public async Task<bool> UpdateCurrency(int id, decimal currency)
        {
            var currentUser = LoginContext.Instance.CurrentUser;
            var currentDate = DateTime.Now;
            var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
            if (mainOrder == null)
            {
                return false;
            }
            if (mainOrder.CurrentCNYVN == currency)
            {
                return true;
            }
            var userOrder = await unitOfWork.Repository<Users>().GetQueryable().FirstOrDefaultAsync(x => x.Id == mainOrder.UID);
            if (userOrder == null)
            {
                return false;
            }
            decimal oldCurrency = mainOrder.CurrentCNYVN ?? 0;
            mainOrder.CurrentCNYVN = currency;
            var orders = await unitOfWork.Repository<Order>().GetQueryable().Where(x => x.MainOrderId == id).ToListAsync();
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    order.CurrentCNYVN = currency;
                    order.PriceVND = (order.PriceOrigin ?? 0) * (order.Quantity ?? 0) * currency;
                    await unitOfWork.Repository<Order>().UpdateFieldsSaveAsync(order, new Expression<Func<Order, object>>[]
                    {
                        x => x.CurrentCNYVN,
                        x => x.PriceVND,
                        x => x.Updated,
                        x => x.UpdatedBy
                    });
                }
            }
            mainOrder.PriceVND = currency * (mainOrder.PriceCNY ?? 0);
            mainOrder.TotalPriceReal = currency * (mainOrder.TotalPriceRealCNY ?? 0);
            mainOrder.FeeShipCN = currency * (mainOrder.FeeShipCNCNY ?? 0);
            mainOrder.FeeShipCNReal = currency * (mainOrder.FeeShipCNRealCNY ?? 0);
            mainOrder.CKFeeBuyPro = Math.Round((mainOrder.FeeBuyPro ?? 0) / currency, 2);
            mainOrder.IsPackedPrice = currency * (mainOrder.IsPackedPriceCNY ?? 0);
            mainOrder.IsCheckProductPrice = currency * (mainOrder.IsCheckProductPriceCNY ?? 0);

            var userLevel = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == userOrder.LevelId);
            decimal lessDeposit = userLevel == null ? 0 : (userLevel.LessDeposit) ?? 0;
            decimal amountDeposit = lessDeposit > 0 ? ((mainOrder.PriceVND ?? 0) * lessDeposit / 100) : (mainOrder.PriceVND ?? 0);
            mainOrder.AmountDeposit = amountDeposit;
            mainOrder.TotalPriceVND = (mainOrder.PriceVND ?? 0) + (mainOrder.FeeShipCN ?? 0) + (mainOrder.FeeBuyPro ?? 0)
                + (mainOrder.IsFastDeliveryPrice ?? 0) + (mainOrder.IsCheckProductPrice ?? 0) + (mainOrder.IsPackedPrice ?? 0)
                + (mainOrder.InsuranceMoney ?? 0) + (mainOrder.Surcharge ?? 0) + (mainOrder.FeeWeight ?? 0);

            if (mainOrder.Deposit > mainOrder.TotalPriceVND)
            {
                decimal refundAmount = (mainOrder.Deposit ?? 0) - (mainOrder.TotalPriceVND ?? 0);
                decimal oldWallet = userOrder.Wallet ?? 0;
                decimal newWallet = (userOrder.Wallet ?? 0) + refundAmount;
                userOrder.Wallet = newWallet;
                await unitOfWork.Repository<Users>().UpdateFieldsSaveAsync(userOrder, new Expression<Func<Users, object>>[]
                {
                    x => x.Wallet,
                    x => x.Updated,
                    x => x.UpdatedBy
                });
                await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                {
                    UID = userOrder.Id,
                    MainOrderId = id,
                    Amount = refundAmount,
                    Content = $"{currentUser.UserName} đã hoàn tiền khi thay đổi tỷ giá đơn #{mainOrder.Id} số tiền {string.Format("{0:N0}", refundAmount)}.",
                    MoneyLeft = newWallet,
                    Type = (int?)DauCongVaTru.Cong,
                    TradeType = (int?)HistoryPayWalletContents.NhanLaiTienDatCoc,
                    Deleted = false,
                    Active = true,
                    CreatedBy = currentUser.UserName,
                    Created = currentDate
                });
                mainOrder.Deposit = mainOrder.TotalPriceVND;
            }

            await unitOfWork.Repository<MainOrder>().UpdateFieldsSaveAsync(mainOrder, new Expression<Func<MainOrder, object>>[]
            {
                 x => x.CurrentCNYVN,
                 x => x.PriceVND,
                 x => x.TotalPriceReal,
                 x => x.FeeShipCN,
                 x => x.FeeShipCNReal,
                 x => x.CKFeeBuyPro,
                 x => x.IsPackedPrice,
                 x => x.IsCheckProductPrice,
                 x => x.AmountDeposit,
                 x => x.Deposit,
                 x => x.TotalPriceVND,
                 x => x.Updated,
                 x => x.UpdatedBy
            });
            await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
            {
                MainOrderId = id,
                UID = currentUser.UserId,
                HistoryContent = $"{currentUser.UserName} đã đổi tỷ giá đơn #{id} từ {string.Format("{0:N0}", oldCurrency)} sang {string.Format("{0:N0}", currency)}",
                Type = (int?)TypeHistoryOrderChange.TienDatCoc
            });

            await unitOfWork.SaveAsync();
            return true;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var exists = Queryable
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == id);
            if (exists != null)
            {
                exists.Status = (int)StatusOrderContants.Huy;
                unitOfWork.Repository<MainOrder>().Update(exists);
                await unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new Exception(id + " not exists");
            }
        }
        protected override string GetStoreProcName()
        {
            return "MainOrder_GetPagingData";
        }
        public async Task<bool> UpdateMainOrder(List<int> listID, int status, int userId)
        {
            DateTime currentDate = DateTime.Now;
            decimal totalMustPay = 0;
            string userName = LoginContext.Instance.CurrentUser.UserName;
            var users = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.UserName == userName).FirstOrDefaultAsync();

            decimal wallet = users.Wallet ?? 0;
            if (wallet <= 0 && status != (int?)StatusOrderContants.DaDatCoc)
                throw new AppException("Số dư trong tài khoản không đủ. Vui lòng nạp thêm tiền");

            if (!listID.Any())
                throw new KeyNotFoundException("List không tồn tại");

            var mainOrders = await this.GetAsync(x => !x.Deleted && x.Active
                && (listID.Contains(x.Id))
            );

            int checkStatus = 0;

            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    switch (status)
                    {
                        case 1: //Hủy

                            //Kiểm tra đơn hàng đủ điều kiện hay không
                            checkStatus = mainOrders.Where(x => x.Status == (int?)StatusOrderContants.ChuaDatCoc).Count();
                            if (mainOrders.Count() != checkStatus) //Nếu số lượng khác nhau
                            {
                                List<int> listError = mainOrders.Where(x => x.Status != (int?)StatusOrderContants.ChuaDatCoc).Select(x => x.Id).ToList();
                                throw new AppException(string.Format("Đơn này bị sai trạng thái Đặt cọc, vui lòng kiểm tra lại"));
                            }

                            foreach (var item in mainOrders)
                            {
                                //Cập nhật cộng tiền trong account
                                users.Wallet += item.Deposit ?? 0;
                                users.Updated = currentDate;
                                users.UpdatedBy = userName;
                                unitOfWork.Repository<Users>().Update(users);

                                //Cập nhật lại trạng thái của đơn hang
                                item.Deposit = 0;
                                item.Status = (int?)StatusOrderContants.Huy;
                                item.Updated = currentDate;
                                item.UpdatedBy = userName;
                                unitOfWork.Repository<MainOrder>().Update(item);

                                //Thêm lịch sử đơn hàng thay đổi
                                await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                                {
                                    MainOrderId = item.Id,
                                    UID = users.Id,
                                    HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng ID là: {1} từ: Chờ đặt cọc, sang: Hủy đơn hàng.", users.UserName, item.Id),
                                    Type = (int?)TypeHistoryOrderChange.TienDatCoc
                                });
                            }

                            break;

                        case 2: //Đã đặt cọc

                            //Kiểm tra đơn hàng đủ điều kiện hay không
                            checkStatus = mainOrders.Where(x => x.Status == (int?)StatusOrderContants.ChuaDatCoc).Count();
                            if (mainOrders.Count() != checkStatus) //Nếu số lượng khác nhau
                            {
                                List<int> listError = mainOrders.Where(x => x.Status != (int?)StatusOrderContants.ChuaDatCoc).Select(x => x.Id).ToList();
                                throw new AppException(string.Format("Những đơn có là: {0} bị sai trạng thái Hủy, vui lòng kiểm tra lại", listError));
                            }

                            totalMustPay = mainOrders.Sum(x => x.AmountDeposit - x.Deposit) ?? 0; //Tính tổng tiền đặt cọc

                            if (wallet < totalMustPay)
                                throw new AppException("Số dư trong tài khoản không đủ. Vui lòng nạp thêm tiền");

                            foreach (var item in mainOrders)
                            {
                                decimal deposit = item.Deposit ?? 0;
                                decimal amountDeposit = item.AmountDeposit ?? 0;
                                decimal mustDeposit = amountDeposit - deposit;

                                //Cập nhật trừ số tiền trong account
                                users.Wallet -= mustDeposit;
                                users.Updated = currentDate;
                                users.UpdatedBy = userName;

                                //Tính tiền tích lũy
                                users = await userService.CreateUserTransactionMoney(users, mustDeposit);
                                unitOfWork.Repository<Users>().Update(users);
                                //Cập nhật lại trạng thái, tiền cọc, ngày cọc, ngày dự kiến của đơn hàng
                                item.Status = (int?)StatusOrderContants.DaDatCoc;
                                item.Deposit = amountDeposit;
                                item.DepositDate = currentDate;

                                var warehouse = await unitOfWork.CatalogueRepository<Warehouse>().GetQueryable().Where(x => x.Id == item.ReceivePlace).FirstOrDefaultAsync();
                                if (warehouse != null)
                                    item.ExpectedDate = currentDate.AddDays(Convert.ToInt32(warehouse.ExpectedDate));

                                item.Updated = currentDate;
                                item.UpdatedBy = userName;
                                unitOfWork.Repository<MainOrder>().Update(item);
                                await unitOfWork.SaveAsync();

                                //Thêm lịch sử của ví tiền
                                await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                                {
                                    UID = users.Id,
                                    MainOrderId = item.Id,
                                    Amount = mustDeposit,
                                    Content = string.Format("{0} đã đặt cọc đơn hàng: {1}.", users.UserName, item.Id),
                                    MoneyLeft = wallet - amountDeposit,
                                    Type = (int?)DauCongVaTru.Tru,
                                    TradeType = (int?)HistoryPayWalletContents.DatCoc,
                                    Deleted = false,
                                    Active = true,
                                    CreatedBy = users.UserName,
                                    Created = currentDate
                                });
                                wallet -= amountDeposit;

                                //Thêm lịch sử thanh toán mua hộ
                                await unitOfWork.Repository<PayOrderHistory>().CreateAsync(new PayOrderHistory()
                                {
                                    MainOrderId = item.Id,
                                    UID = users.Id,
                                    Status = (int?)StatusPayOrderHistoryContants.DatCoc2,
                                    Amount = mustDeposit,
                                    Type = (int?)TypePayOrderHistoryContants.ViDienTu
                                });

                                //Thông báo
                                //Đơn hàng được đặt cọc
                                var notificationSettingDC = await notificationSettingService.GetByIdAsync(6);
                                var notiTemplateAdmin = await notificationTemplateService.GetByIdAsync(13);
                                var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync("ADHDC");
                                string subject = emailTemplate.Subject;
                                string emailContent = string.Format(emailTemplate.Body);
                                await sendNotificationService.SendNotification(notificationSettingDC, notiTemplateAdmin, item.Id.ToString(), string.Format(Detail_MainOrder_Admin, item.Id), "", null, subject, emailContent);
                            }
                            break;

                        case 7: //Thanh toán

                            //Kiểm tra đơn hàng đủ điều kiện hay không
                            checkStatus = mainOrders.Where(x => x.Status == (int?)StatusOrderContants.DaVeKhoVN).Count();
                            if (mainOrders.Count() != checkStatus) //Nếu số lượng khác nhau
                            {
                                List<int> listError = mainOrders.Where(x => x.Status != (int?)StatusOrderContants.DaVeKhoVN).Select(x => x.Id).ToList();
                                throw new AppException(string.Format("Những đơn có là: {0} bị sai trạng thái Thanh toán, vui lòng kiểm tra lại", listError));
                            }

                            totalMustPay = mainOrders.Sum(x => (x.TotalPriceVND + x.FeeInWareHouse) - x.Deposit) ?? 0; //Tính tổng tiền thanh toán

                            if (wallet < totalMustPay)
                                throw new AppException("Số dư trong tài khoản không đủ. Vui lòng nạp thêm tiền");

                            foreach (var item in mainOrders)
                            {
                                decimal deposit = item.Deposit ?? 0;
                                decimal feeWarehouse = item.FeeInWareHouse ?? 0;
                                decimal totalPriceVND = item.TotalPriceVND ?? 0;
                                decimal moneyLeft = (totalPriceVND + feeWarehouse) - deposit;

                                //Cập nhật trừ số tiền trong account
                                users.Wallet -= moneyLeft;
                                users.Updated = currentDate;
                                users.UpdatedBy = userName;
                                //Tính tiền tích lũy
                                users = await userService.CreateUserTransactionMoney(users, moneyLeft);
                                unitOfWork.Repository<Users>().Update(users);

                                //Cập nhật lại trạng thái, tiền cọc (tổng tiền), ngày thanh toán, ngày dự kiến của Đơn hàng
                                item.Status = (int?)StatusOrderContants.KhachDaThanhToan;
                                item.Deposit = moneyLeft + deposit;
                                item.PayDate = currentDate;

                                var warehouse = await unitOfWork.CatalogueRepository<Warehouse>().GetQueryable().Where(x => x.Id == item.ReceivePlace).FirstOrDefaultAsync();
                                if (warehouse != null)
                                    item.ExpectedDate = currentDate.AddDays(Convert.ToInt32(warehouse.ExpectedDate));

                                item.Updated = currentDate;
                                item.UpdatedBy = userName;
                                unitOfWork.Repository<MainOrder>().Update(item);

                                //Thêm lịch sử của ví tiền
                                await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                                {
                                    UID = users.Id,
                                    MainOrderId = item.Id,
                                    Amount = moneyLeft,
                                    Content = string.Format("{0} đã thanh toán đơn hàng: {1}.", users.UserName, item.Id),
                                    MoneyLeft = wallet - moneyLeft,
                                    Type = (int?)DauCongVaTru.Tru,
                                    TradeType = (int?)HistoryPayWalletContents.ThanhToanHoaDon,
                                    Deleted = false,
                                    Active = true,
                                    CreatedBy = users.UserName,
                                    Created = currentDate
                                });
                                wallet -= moneyLeft;

                                //Thêm lịch sử đơn hàng thay đổi
                                await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                                {
                                    MainOrderId = item.Id,
                                    UID = users.Id,
                                    HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng ID là: {1} từ: Chờ thanh toán, sang: Khách đã thanh toán.", users.UserName, item.Id),
                                    Type = (int?)TypeHistoryOrderChange.TienDatCoc
                                });

                                //Thêm lịch sử thanh toán mua hộ
                                await unitOfWork.Repository<PayOrderHistory>().CreateAsync(new PayOrderHistory()
                                {
                                    MainOrderId = item.Id,
                                    UID = users.Id,
                                    Status = (int?)StatusPayOrderHistoryContants.ThanhToan,
                                    Amount = moneyLeft,
                                    Type = (int?)DauCongVaTru.Cong,
                                    Deleted = false,
                                    Active = true,
                                    CreatedBy = users.UserName,
                                    Created = currentDate
                                });

                                //Thông báo
                                //Đơn hàng được thanh toán
                                var notificationSettingTT = await notificationSettingService.GetByIdAsync(11);
                                var notiTemplate = await notificationTemplateService.GetByIdAsync(15);
                                var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync("ADHDTT");
                                string subject = emailTemplate.Subject;
                                string emailContent = string.Format(emailTemplate.Body);
                                await sendNotificationService.SendNotification(notificationSettingTT, notiTemplate, item.Id.ToString(), string.Format(Detail_MainOrder_Admin, item.Id), "", null, subject, emailContent);
                            }

                            break;
                        default:
                            break;
                    }
                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new AppException(ex.Message);
                }
                return true;
            }
        }

        public async Task<bool> CreateOrder(List<MainOrder> listData, bool isAnother)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var data in listData)
                    {
                        await this.CreateAsync(data);
                        foreach (var item in data.Orders)
                        {
                            item.MainOrderId = data.Id;
                        }
                        data.StaffIncomes.ForEach(e => e.MainOrderId = data.Id);

                        await unitOfWork.Repository<Order>().CreateAsync(data.Orders);

                        await unitOfWork.Repository<StaffIncome>().CreateAsync(data.StaffIncomes);
                        await unitOfWork.SaveAsync();

                        if (data.ShopTempId > 0)
                            //Xóa Shop temp và order temp
                            await orderShopTempService.DeleteAsync(data.ShopTempId);

                        //Thông báo có đơn đặt hàng mới
                        int notiTemplateId = 9; //Đơn mới
                        string emailTemplateCode = "ADHM";
                        if (isAnother)
                        {
                            notiTemplateId = 10; //Đơn TMĐT mới
                            emailTemplateCode = "ATMDKM";
                        }
                        var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync(emailTemplateCode);
                        string subject = emailTemplate.Subject;
                        string emailContent = string.Format(emailTemplate.Body);
                        var notiTemplate = await notificationTemplateService.GetByIdAsync(notiTemplateId);
                        var notificationSetting = await notificationSettingService.GetByIdAsync(5);
                        await sendNotificationService.SendNotification(notificationSetting, notiTemplate, data.Id.ToString(), string.Format(Detail_MainOrder_Admin, data.Id), "", null, string.Empty, string.Empty);
                    }

                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
            return true;
        }

        public async Task<bool> Payment(int id, int paymentType, int paymentMethod, decimal amount, string note)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    DateTime currentDate = DateTime.Now;
                    var mainOrder = await this.GetByIdAsync(id);
                    if (mainOrder == null)
                        throw new KeyNotFoundException("Đơn hàng không tồn tại");
                    var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == LoginContext.Instance.CurrentUser.UserId).FirstOrDefaultAsync();

                    decimal? deposit = mainOrder.Deposit == null ? 0 : mainOrder.Deposit;
                    decimal? amountDeposit = mainOrder.AmountDeposit == null ? 0 : mainOrder.AmountDeposit;
                    decimal? mustPaymentDeposit = amountDeposit - deposit;

                    decimal? totalPriceVND = mainOrder.TotalPriceVND == null ? 0 : mainOrder.TotalPriceVND;
                    decimal? mustPayment = totalPriceVND - deposit;

                    var userMainOrder = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == mainOrder.UID).FirstOrDefaultAsync();
                    if (userMainOrder == null)
                        throw new KeyNotFoundException("User của đơn hàng không tồn tại");

                    decimal? wallet = userMainOrder.Wallet;

                    switch (paymentType) //Loại thanh toán
                    {
                        case 1: //Đặt cọc
                            if (amount < mustPaymentDeposit)
                                throw new AppException("Số tiền nhập vào không đủ để đặt cọc");

                            decimal? depositPayment = 0;
                            switch (paymentMethod) //Phương thức thanh toán
                            {
                                case 1: //Trực tiếp
                                    depositPayment = deposit + amount;
                                    //Tính tiền tích lũy
                                    userMainOrder = await userService.CreateUserTransactionMoney(userMainOrder, amount);
                                    unitOfWork.Repository<Users>().Update(userMainOrder);
                                    await unitOfWork.Repository<AdminSendUserWallet>().CreateAsync(new AdminSendUserWallet()
                                    {
                                        UID = userMainOrder.Id,
                                        Amount = amount,
                                        Updated = DateTime.UtcNow.AddHours(7),
                                        UpdatedBy = LoginContext.Instance.CurrentUser.UserName,
                                        Status = (int)WalletStatus.DaDuyet,
                                        BankId = 100, //Đặt đại
                                        TradeContent = string.Format("{0} đã được nạp tiền vào tài khoản", userMainOrder.UserName),
                                    });

                                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                                    {
                                        UID = userMainOrder.Id,
                                        MainOrderId = 0,
                                        Amount = amount,
                                        Content = string.Format("{0} đã được nạp tiền vào tài khoản.", userMainOrder.UserName),
                                        MoneyLeft = userMainOrder.Wallet + amount,
                                        Type = (int?)DauCongVaTru.Cong,
                                        TradeType = (int?)HistoryPayWalletContents.AdminChuyenTien,
                                    });

                                    //Thêm lịch sử của ví tiền
                                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                                    {
                                        UID = userMainOrder.Id,
                                        MainOrderId = mainOrder.Id,
                                        Amount = amount,
                                        Content = string.Format("{0} đã đặt cọc đơn hàng: {1}.", userMainOrder.UserName, mainOrder.Id),
                                        MoneyLeft = userMainOrder.Wallet,
                                        Type = (int?)DauCongVaTru.Tru,
                                        TradeType = (int?)HistoryPayWalletContents.DatCoc
                                    });
                                    break;
                                case 2: //Ví điện tử
                                    if (wallet < amount)
                                        throw new AppException("Số tiền trong ví của khách hàng không đủ để đặt cọc");

                                    depositPayment = deposit + amount;
                                    //depositPayment = wallet - amount;

                                    //Cập nhật trừ số tiền trong account
                                    userMainOrder.Wallet -= amount;
                                    //Tính tiền tích lũy
                                    userMainOrder = await userService.CreateUserTransactionMoney(userMainOrder, amount);
                                    unitOfWork.Repository<Users>().Update(userMainOrder);

                                    //Thêm lịch sử của ví tiền
                                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                                    {
                                        UID = userMainOrder.Id,
                                        MainOrderId = mainOrder.Id,
                                        Amount = amount,
                                        Content = string.Format("{0} đã đặt cọc đơn hàng: {1}.", userMainOrder.UserName, mainOrder.Id),
                                        MoneyLeft = userMainOrder.Wallet,
                                        Type = (int?)DauCongVaTru.Tru,
                                        TradeType = (int?)HistoryPayWalletContents.DatCoc
                                    });

                                    break;
                                default:
                                    break;
                            }

                            if (depositPayment == 0)
                                throw new AppException("Số tiền đặt cọc bằng 0");

                            //Thêm lịch sử đơn hàng thay đổi
                            await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                            {
                                MainOrderId = mainOrder.Id,
                                UID = LoginContext.Instance.CurrentUser.UserId,
                                HistoryContent = String.Format("{0} đã đổi tiền đặt cọc của đơn hàng ID là: {1} từ: {2}, sang: {3}.", LoginContext.Instance.CurrentUser.UserName, mainOrder.Id, deposit, depositPayment),
                                Type = (int?)TypeHistoryOrderChange.PhiGiaoTanNha
                            });

                            //Thêm lịch sử thanh toán mua hộ
                            await unitOfWork.Repository<PayOrderHistory>().CreateAsync(new PayOrderHistory()
                            {
                                MainOrderId = mainOrder.Id,
                                UID = LoginContext.Instance.CurrentUser.UserId,
                                Status = (int?)StatusPayOrderHistoryContants.DatCoc2,
                                Amount = amount,
                                Type = (int?)TypePayOrderHistoryContants.ViDienTu
                            });

                            await unitOfWork.Repository<PayAllOrderHistory>().CreateAsync(new PayAllOrderHistory()
                            {
                                MainOrderId = mainOrder.Id,
                                UID = LoginContext.Instance.CurrentUser.UserId,
                                Status = (int?)StatusPayOrderHistoryContants.DatCoc2,
                                Note = note,
                                Amount = amount
                            });

                            mainOrder.Deposit = depositPayment;
                            mainOrder.Status = (int)StatusOrderContants.DaDatCoc;
                            mainOrder.DepositDate = currentDate;
                            unitOfWork.Repository<MainOrder>().Update(mainOrder);

                            break;
                        case 2: //Thanh toán
                            if (amount < mustPayment)
                                throw new AppException("Số tiền nhập vào không đủ để thanh toán");
                            if (amount > mustPayment)
                                throw new AppException("Số tiền đã vượt mức cần phải thanh toán");

                            decimal? orderPayment = 0;
                            switch (paymentMethod) //Phương thức thanh toán
                            {
                                case 1: //Trực tiếp
                                    orderPayment = deposit + amount;
                                    mainOrder.Deposit = orderPayment;

                                    //Tính tiền tích lũy
                                    userMainOrder = await userService.CreateUserTransactionMoney(userMainOrder, amount);
                                    unitOfWork.Repository<Users>().Update(userMainOrder);

                                    await unitOfWork.Repository<PayAllOrderHistory>().CreateAsync(new PayAllOrderHistory()
                                    {
                                        MainOrderId = mainOrder.Id,
                                        UID = LoginContext.Instance.CurrentUser.UserId,
                                        Status = (int?)StatusPayOrderHistoryContants.ThanhToan,
                                        Note = note,
                                        Amount = amount
                                    });

                                    await unitOfWork.Repository<AdminSendUserWallet>().CreateAsync(new AdminSendUserWallet()
                                    {
                                        UID = userMainOrder.Id,
                                        Amount = amount,
                                        Updated = DateTime.UtcNow.AddHours(7),
                                        UpdatedBy = LoginContext.Instance.CurrentUser.UserName,
                                        Status = (int)WalletStatus.DaDuyet,
                                        BankId = 100, //Đặt đại
                                        TradeContent = string.Format("{0} đã được nạp tiền vào tài khoản", userMainOrder.UserName),
                                    });

                                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                                    {
                                        UID = userMainOrder.Id,
                                        MainOrderId = 0,
                                        Amount = amount,
                                        Content = string.Format("{0} đã được nạp tiền vào tài khoản.", userMainOrder.UserName),
                                        MoneyLeft = userMainOrder.Wallet + amount,
                                        Type = (int?)DauCongVaTru.Cong,
                                        TradeType = (int?)HistoryPayWalletContents.AdminChuyenTien,
                                    });

                                    //Thêm lịch sử của ví tiền
                                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                                    {
                                        UID = userMainOrder.Id,
                                        MainOrderId = mainOrder.Id,
                                        Amount = amount,
                                        Content = string.Format("{0} đã thanh toán đơn hàng: {1}.", userMainOrder.UserName, mainOrder.Id),
                                        MoneyLeft = userMainOrder.Wallet,
                                        Type = (int?)DauCongVaTru.Tru,
                                        TradeType = (int?)HistoryPayWalletContents.ThanhToanHoaDon
                                    });

                                    break;
                                case 2: //Ví điện tử
                                    if (wallet < amount)
                                        throw new AppException("Số tiền trong ví của khách hàng không đủ để thanh toán");

                                    orderPayment = deposit + amount;
                                    mainOrder.Deposit = orderPayment;
                                    if (mainOrder.DepositDate == null)
                                        mainOrder.DepositDate = currentDate;

                                    var walletLeft = wallet - amount;

                                    //Cập nhật trừ số tiền trong account
                                    userMainOrder.Wallet = walletLeft;
                                    //Tính tiền tích lũy
                                    userMainOrder = await userService.CreateUserTransactionMoney(userMainOrder, amount);
                                    unitOfWork.Repository<Users>().Update(userMainOrder);

                                    //Thêm lịch sử của ví tiền
                                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                                    {
                                        UID = userMainOrder.Id,
                                        MainOrderId = mainOrder.Id,
                                        Amount = amount,
                                        Content = string.Format("{0} đã thanh toán đơn hàng: {1}.", userMainOrder.UserName, mainOrder.Id),
                                        MoneyLeft = walletLeft,
                                        Type = (int?)DauCongVaTru.Tru,
                                        TradeType = (int?)HistoryPayWalletContents.ThanhToanHoaDon
                                    });

                                    //Cập nhật ngày dự kiến
                                    var warehouse = await unitOfWork.Repository<Warehouse>().GetQueryable().Where(e => e.Id == mainOrder.ReceivePlace).FirstOrDefaultAsync();
                                    mainOrder.ExpectedDate = warehouse == null ? null : currentDate.AddDays(Convert.ToInt32(warehouse.ExpectedDate));

                                    break;
                                default:
                                    break;
                            }

                            if (orderPayment == 0)
                                throw new AppException("Số tiền thanh toán bằng 0");

                            //Thêm lịch sử đơn hàng thay đổi
                            await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                            {
                                MainOrderId = mainOrder.Id,
                                UID = LoginContext.Instance.CurrentUser.UserId,
                                HistoryContent = String.Format("{0} đã thanh toán và nhận hàng của đơn hàng ID là: {1} từ: {2}, sang: {3}.", LoginContext.Instance.CurrentUser.UserName, mainOrder.Id, deposit, orderPayment),
                                Type = (int?)TypeHistoryOrderChange.TienDatCoc
                            });

                            //Thêm lịch sử thanh toán mua hộ
                            await unitOfWork.Repository<PayOrderHistory>().CreateAsync(new PayOrderHistory()
                            {
                                MainOrderId = mainOrder.Id,
                                UID = LoginContext.Instance.CurrentUser.UserId,
                                Status = (int?)StatusPayOrderHistoryContants.ThanhToan,
                                Amount = amount,
                                Type = (int?)TypePayOrderHistoryContants.ViDienTu
                            });

                            mainOrder.Status = (int)StatusOrderContants.KhachDaThanhToan;
                            mainOrder.PayDate = currentDate;
                            unitOfWork.Repository<MainOrder>().Update(mainOrder);

                            break;
                        default:
                            break;
                    }
                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
            return true;
        }
        public override async Task<MainOrder> GetByIdAsync(int id)
        {
            var mainOrder = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            //var mainOrder = await Queryable.Where(e => e.Id == id && !e.Deleted).FirstOrDefaultAsync();
            if (mainOrder == null)
                return null;

            //Danh sách mã vận đơn
            var smallPackages = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.MainOrderId == mainOrder.Id).OrderBy(o => o.Id).ToListAsync();

            //Danh sách mã đơn hàng
            var mainOrderCodes = await unitOfWork.Repository<MainOrderCode>().GetQueryable().Where(x => !x.Deleted && x.MainOrderID == mainOrder.Id).OrderBy(o => o.Id).ToListAsync();

            //Lấy danh sách các mã vận đơn của 1 mã đơn hàng
            foreach (var smallPacage in smallPackages)
            {
                foreach (var mainOrderCode in mainOrderCodes)
                {
                    if (smallPacage.MainOrderCodeId == mainOrderCode.Id)
                    {
                        mainOrderCode.OrderTransactionCode.Add(smallPacage.OrderTransactionCode);
                        smallPacage.Code = mainOrderCode.Code;
                    }
                }
            }
            if (mainOrderCodes.Any())
                mainOrder.MainOrderCodes = mainOrderCodes.ToList();
            if (smallPackages.Any())
                mainOrder.SmallPackages = smallPackages;
            //Danh sách sản phẩm
            var orders = await unitOfWork.Repository<Order>().GetQueryable().Where(x => !x.Deleted && x.MainOrderId == mainOrder.Id).OrderByDescending(o => o.Id).ToListAsync();
            if (orders.Any())
                mainOrder.Orders = orders;

            //Thông tin người đặt hàng
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == mainOrder.UID).FirstOrDefaultAsync();
            if (user != null)
            {
                mainOrder.UserName = user.UserName;
                mainOrder.Wallet = user.Wallet ?? 0;
                mainOrder.FullName = user.FullName;
                mainOrder.Address = user.Address;
                mainOrder.Email = user.Email;
                mainOrder.Phone = user.Phone;
            }

            //Đặt hàng, Saler
            var orderer = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == mainOrder.DatHangId).FirstOrDefaultAsync();
            if (orderer != null)
                mainOrder.OrdererUserName = orderer.UserName;
            var saler = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == mainOrder.SalerId).FirstOrDefaultAsync();
            if (saler != null)
                mainOrder.SalerUserName = saler.UserName;

            //Lịch sử thay đổi
            var historyOrderChanges = await unitOfWork.Repository<HistoryOrderChange>().GetQueryable().Where(x => !x.Deleted && x.Active
                && (x.MainOrderId == id)).OrderByDescending(o => o.Id).ToListAsync();
            if (historyOrderChanges.Any())
            {
                foreach (var historyOrderChange in historyOrderChanges)
                {
                    var userHistoryOrderChange = await userService.GetByIdAsync(historyOrderChange.UID ?? 0);
                    if (userHistoryOrderChange == null) continue;
                    historyOrderChange.UserGroupName = userHistoryOrderChange.UserGroupName;
                }

                mainOrder.HistoryOrderChanges = historyOrderChanges;
            }

            //Lịch sử thanh toán
            var payOrderHistories = await unitOfWork.Repository<PayOrderHistory>().GetQueryable().Where(x => !x.Deleted && x.Active
                && (x.MainOrderId == id)).OrderByDescending(o => o.Id).ToListAsync();
            if (payOrderHistories.Any())
                mainOrder.PayOrderHistories = payOrderHistories;

            //Lịch sử khiếu nại
            var complains = await unitOfWork.Repository<Complain>().GetQueryable().Where(x => !x.Deleted && x.Active
                && (x.MainOrderId == id)).OrderByDescending(o => o.Id).ToListAsync();
            if (historyOrderChanges.Any())
                mainOrder.Complains = complains;

            //Nhắn tin với khách hàng, với nội bộ
            var orderComments = await unitOfWork.Repository<OrderComment>().GetQueryable().Where(x => !x.Deleted && x.Active
                && (x.MainOrderId == id)).OrderByDescending(o => o.Id).ToListAsync();
            if (orderComments.Any())
                mainOrder.OrderComments = orderComments;

            //Kho TQ, Kho VN, Phương thức VC
            var warehouseFrom = await unitOfWork.CatalogueRepository<WarehouseFrom>().GetQueryable().Where(x => x.Id == mainOrder.FromPlace).FirstOrDefaultAsync();
            if (warehouseFrom != null)
                mainOrder.FromPlaceName = warehouseFrom.Name;

            var warehouse = await unitOfWork.CatalogueRepository<Warehouse>().GetQueryable().Where(x => x.Id == mainOrder.ReceivePlace).FirstOrDefaultAsync();
            if (warehouse != null)
                mainOrder.ReceivePlaceName = warehouse.Name;

            var shippingTypeToWareHouse = await unitOfWork.CatalogueRepository<ShippingTypeToWareHouse>().GetQueryable().Where(x => x.Id == mainOrder.ShippingType).FirstOrDefaultAsync();
            if (shippingTypeToWareHouse != null)
                mainOrder.ShippingTypeName = shippingTypeToWareHouse.Name;

            //Phụ phí - Danh sách phụ phí (Cập nhật mỗi lần getbyId)
            var feeSupports = await unitOfWork.Repository<FeeSupport>().GetQueryable().Where(x => !x.Deleted && x.Active && (x.MainOrderId == id)).OrderByDescending(o => o.Id).ToListAsync();
            if (feeSupports.Any())
            {
                mainOrder.FeeSupports = feeSupports;
                mainOrder.Surcharge = feeSupports.Sum(e => e.SupportInfoVND);
                unitOfWork.Repository<MainOrder>().Update(mainOrder);
                await unitOfWork.SaveAsync();
                unitOfWork.Repository<MainOrder>().Detach(mainOrder);
            }

            return mainOrder;
        }

        public override async Task<bool> UpdateAsync(MainOrder item)
        {
            //Bên SmallPackage cũng có tính toán nữa đó
            DateTime currentDate = DateTime.Now;
            var oldMainOrderStatus = (await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => e.Id == item.Id).FirstOrDefaultAsync()).Status;

            var configurations = await configurationsService.GetSingleAsync();
            var userMainOrder = await unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == item.UID).FirstOrDefaultAsync();
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == LoginContext.Instance.CurrentUser.UserId).FirstOrDefaultAsync();

            var userLevelMainOrder = await unitOfWork.Repository<UserLevel>().GetQueryable().Where(e => !e.Deleted && userMainOrder.LevelId == e.Id).FirstOrDefaultAsync();
            decimal? ckFeeWeight = userLevelMainOrder == null ? 1 : userLevelMainOrder.FeeWeight;

            //Danh sách mã vận đơn
            var smallPackages = item.SmallPackages;

            decimal? totalWeight = smallPackages.Sum(e => e.PayableWeight);
            decimal? totalVolume = smallPackages.Sum(e => e.VolumePayment);
            decimal? warehouseFeePrice = 0;
            decimal? volumeFeePrice = 0;

            //Phí cân nặng vận chuyển TQ - VN
            decimal? feeWeight = 0;
            //Phí thể tích TQ - VN
            decimal? feeVolume = 0;

            foreach (var smallPackage in smallPackages)
            {
                var exists = new SmallPackage();

                var warehouseFee = await unitOfWork.Repository<WarehouseFee>().GetQueryable().Where(e => !e.Deleted &&
                    e.WarehouseFromId == item.FromPlace && e.WarehouseId == item.ReceivePlace && e.ShippingTypeToWareHouseId == item.ShippingType && e.IsHelpMoving == false &&
                    smallPackage.PayableWeight >= e.WeightFrom && smallPackage.PayableWeight < e.WeightTo).FirstOrDefaultAsync();

                var volumeFee = await unitOfWork.Repository<VolumeFee>().GetQueryable().Where(e => !e.Deleted &&
                    e.WarehouseFromId == item.FromPlace && e.WarehouseId == item.ReceivePlace && e.ShippingTypeToWareHouseId == item.ShippingType && e.IsHelpMoving == false &&
                    smallPackage.VolumePayment >= e.VolumeFrom && smallPackage.VolumePayment < e.VolumeTo).FirstOrDefaultAsync();

                if (warehouseFee != null)
                    warehouseFeePrice = warehouseFee.Price;

                if (volumeFee != null)
                    volumeFeePrice = volumeFee.Price;
                //decimal? smPriceWeight = totalWeight * warehouseFeePrice;
                decimal? smPriceWeight = 0;
                if ((userMainOrder.FeeTQVNPerWeight ?? 0) > 0)
                    smPriceWeight = smallPackage.PayableWeight * userMainOrder.FeeTQVNPerWeight;
                else
                    smPriceWeight = smallPackage.PayableWeight * warehouseFeePrice;
                feeWeight += smPriceWeight;

                //decimal? smPriceVolume = totalVolume * volumeFeePrice;
                decimal? smPriceVolume = 0;
                if ((userMainOrder.FeeTQVNPerVolume ?? 0) > 0)
                    smPriceVolume = smallPackage.VolumePayment * userMainOrder.FeeTQVNPerVolume;
                else
                    smPriceVolume = smallPackage.VolumePayment * volumeFeePrice;
                feeVolume += smPriceVolume;

                smallPackage.TotalPrice = smPriceWeight > smPriceVolume ? smPriceWeight : smPriceVolume;

                switch (smallPackage.Status)
                {
                    case (int)StatusSmallPackage.DaVeKhoTQ:

                        smallPackage.DateInTQWarehouse = currentDate;
                        smallPackage.StaffTQWarehouse = user.UserName;

                        break;
                    case (int)StatusSmallPackage.DaVeKhoVN:
                        smallPackage.DateInLasteWareHouse = currentDate;
                        smallPackage.StaffVNWarehouse = user.UserName;
                        break;

                    default:
                        break;
                }

                if (smallPackage.Id == 0) //Thêm mới
                {
                    smallPackage.UID = userMainOrder.Id;
                    smallPackage.MainOrderId = item.Id;

                    exists = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.OrderTransactionCode.Equals(smallPackage.OrderTransactionCode)).FirstOrDefaultAsync();
                    if (exists != null)
                        throw new AppException(string.Format("Mã vận đơn {0} đã tồn tại", smallPackage.OrderTransactionCode));

                    await unitOfWork.Repository<SmallPackage>().CreateAsync(smallPackage);
                    await unitOfWork.SaveAsync();
                    unitOfWork.Repository<SmallPackage>().Detach(smallPackage);
                }
                else
                {
                    int totalPackageWaiting = unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.Status >= (int)StatusSmallPackage.MoiDat && x.FloatingStatus <= (int)StatusConfirmSmallPackage.DangChoXacNhan).Count();
                    if (totalPackageWaiting == 0)
                    {
                        var bigPackage = await unitOfWork.Repository<BigPackage>().GetQueryable().Where(x => x.Id == smallPackage.BigPackageId).FirstOrDefaultAsync();
                        if (bigPackage != null)
                        {
                            bigPackage.Status = (int)StatusBigPackage.DaNhanHang;
                            await unitOfWork.Repository<BigPackage>().CreateAsync(bigPackage);
                        }
                    }

                    unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                }
            }

            //Danh sách phụ phí
            var feeSupports = item.FeeSupports;
            foreach (var feeSupport in feeSupports)
            {
                if (feeSupport.Id == 0) //Thêm mới
                {
                    feeSupport.MainOrderId = item.Id;
                    await unitOfWork.Repository<FeeSupport>().CreateAsync(feeSupport);
                }
                else
                {
                    unitOfWork.Repository<FeeSupport>().Update(feeSupport);
                }
            }
            item.Surcharge = feeSupports.Sum(e => e.SupportInfoVND);

            //Nhân viên xử lý
            if (user.UserGroupId == (int)PermissionTypes.ChinaWarehouseManager && (item.ReceivePlace == user.Id || item.ReceivePlace == 0))
                item.ReceivePlace = user.Id;
            else if (user.UserGroupId == (int)PermissionTypes.ChinaWarehouseManager && (item.FromPlace == user.Id || item.FromPlace == 0))
                item.FromPlace = user.Id;

            //Cập nhật thông tin của đơn hàng
            if (item.Status == (int)StatusOrderContants.Huy)
            {
                item.CancelDate = currentDate;
                if (item.Deposit > 0)
                {
                    //Cập nhật số tiền trong account
                    userMainOrder.Wallet += item.Deposit;
                    unitOfWork.Repository<Users>().Update(userMainOrder);

                    //Thêm lịch sử của ví tiền
                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet()
                    {
                        UID = userMainOrder.Id,
                        MainOrderId = item.Id,
                        Amount = item.Deposit,
                        Content = string.Format("Đơn hàng {0} bị hủy và hoàn tiền cọc cho khách.", item.Id),
                        MoneyLeft = userMainOrder.Wallet,
                        Type = (int?)DauCongVaTru.Cong,
                        TradeType = (int?)HistoryPayWalletContents.NhanLaiTienDatCoc,
                    });

                    //Thêm lịch sử thanh toán mua hộ
                    await unitOfWork.Repository<PayOrderHistory>().CreateAsync(new PayOrderHistory()
                    {
                        MainOrderId = item.Id,
                        UID = userMainOrder.Id,
                        Status = (int?)StatusPayOrderHistoryContants.HuyHoanTien,
                        Amount = item.Deposit,
                        Type = (int?)TypePayOrderHistoryContants.ViDienTu,
                    });

                    item.Deposit = 0;
                }

            }

            item.TQVNWeight = item.OrderWeight = totalWeight;
            item.TQVNVolume = totalVolume;

            decimal? feeDelivery = feeWeight > feeVolume ? feeWeight : feeVolume;
            decimal? feeDeliveryDiscount = feeDelivery * ckFeeWeight / 100;
            feeDelivery -= feeDeliveryDiscount;

            item.FeeWeight = feeDelivery;

            item.TotalPriceVND = (item.FeeWeight ?? 0) + (item.FeeShipCN ?? 0)
                + (item.FeeBuyPro ?? 0) + (item.IsCheckProductPrice ?? 0)
                + (item.IsPackedPrice ?? 0) + (item.IsFastDeliveryPrice ?? 0)
                + (item.PriceVND ?? 0) + (item.Surcharge ?? 0)
                + (item.InsuranceMoney ?? 0);

            switch (item.Status)
            {
                case (int)StatusOrderContants.DaDatCoc:
                    if (oldMainOrderStatus != item.Status)
                        item.DepositDate = currentDate; break;
                case (int)StatusOrderContants.DaMuaHang:
                    item.DateBuy = currentDate;

                    //Thông báo
                    //Đơn hàng đã được mua
                    var notiTemplate = await notificationTemplateService.GetByIdAsync(14);
                    var notifcationSetting = await notificationSettingService.GetByIdAsync(7);
                    var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync("ADHDMH");
                    string subject = emailTemplate.Subject;
                    string emailContent = string.Format(emailTemplate.Body);
                    await sendNotificationService.SendNotification(notifcationSetting, notiTemplate, item.Id.ToString(), "", string.Format(Detail_MainOrder, item.Id), item.UID, subject, emailContent);
                    break;
                case (int)StatusOrderContants.DaVeKhoTQ:
                    if (oldMainOrderStatus != item.Status)
                        item.DateTQ = currentDate;
                    //Thông báo
                    //Đơn hàng đã đến kho TQ
                    var notiTemplateTQ = await notificationTemplateService.GetByIdAsync(20);
                    var notiicationSettingTQ = await notificationSettingService.GetByIdAsync(8);
                    var emailTemplateTQ = await sMSEmailTemplateService.GetByCodeAsync("UDDVTQ");
                    string subjectTQ = emailTemplateTQ.Subject;
                    string emailContentTQ = string.Format(emailTemplateTQ.Body);
                    await sendNotificationService.SendNotification(notiicationSettingTQ, notiTemplateTQ, item.Id.ToString(), string.Format(Detail_MainOrder_Admin, item.Id), string.Format(Detail_MainOrder, item.Id), item.UID, subjectTQ, emailContentTQ);
                    break;
                case (int)StatusOrderContants.DaVeKhoVN:
                    if (oldMainOrderStatus != item.Status)
                        item.DateVN = currentDate;
                    //Thông báo
                    //Đơn hàng đã đến kho VN
                    var notiTemplateVN = await notificationTemplateService.GetByIdAsync(19);
                    var notiicationSettingVN = await notificationSettingService.GetByIdAsync(9);
                    var emailTemplateVN = await sMSEmailTemplateService.GetByCodeAsync("UDDVVN");
                    string subjectVN = emailTemplateVN.Subject;
                    string emailContentVN = string.Format(emailTemplateVN.Body);
                    await sendNotificationService.SendNotification(notiicationSettingVN, notiTemplateVN, item.Id.ToString(), string.Format(Detail_MainOrder_Admin, item.Id), string.Format(Detail_MainOrder, item.Id), item.UID, subjectVN, emailContentVN);
                    break;
                case (int)StatusOrderContants.KhachDaThanhToan:
                    if (oldMainOrderStatus != item.Status)
                        item.PayDate = currentDate;
                    var smallPackagePaid = item.SmallPackages;
                    if (smallPackagePaid.Count > 0)
                    {
                        foreach (var smallPackage in smallPackagePaid)
                        {
                            smallPackage.IsPayment = true;
                            unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                        }
                    }
                    break;
                case (int)StatusOrderContants.DaHoanThanh:
                    if (oldMainOrderStatus != item.Status)
                        item.CompleteDate = currentDate;
                    var smallPackageUpdates = item.SmallPackages;
                    if (smallPackageUpdates.Count > 0)
                    {
                        foreach (var smallPackage in smallPackageUpdates)
                        {
                            smallPackage.IsPayment = true;
                            unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                        }
                    }
                    break;
                default:
                    break;
            }

            //Tính phí đơn hàng
            //Commission(item.StaffIncomes);
            foreach (var staffIncome in item.StaffIncomes)
            {
                if (staffIncome.Deleted)
                {
                    //Xóa luôn
                    unitOfWork.Repository<StaffIncome>().Delete(staffIncome);
                    continue;
                }

                if (staffIncome.Id == 0)
                    await unitOfWork.Repository<StaffIncome>().CreateAsync(staffIncome);
                else
                    unitOfWork.Repository<StaffIncome>().Update(staffIncome);
            }

            //Lịch sử thay đổi của đơn hàng
            await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(item.HistoryOrderChanges);

            unitOfWork.Repository<MainOrder>().Update(item);
            await unitOfWork.SaveAsync();
            unitOfWork.Repository<MainOrder>().Detach(item);

            return true;
        }

        /// <summary>
        /// Cập nhật cân nặng trong chi tiết đơn
        /// </summary>
        /// <param name="id"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public async Task<bool> UpdateMainOrderWeight(int id, decimal weight)
        {
            using (var transactionDbContext = Context.Database.BeginTransaction())
            {
                try
                {
                    var oldMainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
                    if (weight != (oldMainOrder.TQVNWeight ?? 0))
                    {
                        var history = new HistoryOrderChange()
                        {
                            MainOrderId = oldMainOrder.Id,
                            HistoryContent = $"{LoginContext.Instance.CurrentUser.UserName} đã đổi cân nặng từ {oldMainOrder.TQVNWeight ?? 0} kg sang {weight} kg.",
                            UID = LoginContext.Instance.CurrentUser.UserId,
                            Type = (int?)TypeHistoryOrderChange.CanNangDonHang,
                            CreatedBy = LoginContext.Instance.CurrentUser.UserName
                        };
                        var warehouseFee = await unitOfWork.Repository<WarehouseFee>().GetQueryable().Where(e => !e.Deleted &&
                            e.WarehouseFromId == oldMainOrder.FromPlace && e.WarehouseId == oldMainOrder.ReceivePlace && e.ShippingTypeToWareHouseId == oldMainOrder.ShippingType && e.IsHelpMoving == false &&
                            weight >= e.WeightFrom && weight < e.WeightTo).FirstOrDefaultAsync();

                        decimal? warehouseFeePrice = 0;
                        if (warehouseFee != null)
                            warehouseFeePrice = warehouseFee.Price;

                        //Phí cân nặng vận chuyển TQ - VN
                        decimal feeWeight = 0;
                        var userMainOrder = await unitOfWork.Repository<Users>().GetQueryable().FirstOrDefaultAsync(x => x.Id == oldMainOrder.UID);
                        decimal? smPriceWeight = 0;
                        if ((userMainOrder.FeeTQVNPerWeight ?? 0) > 0)
                            smPriceWeight = weight * userMainOrder.FeeTQVNPerWeight;
                        else
                            smPriceWeight = weight * warehouseFeePrice;
                        feeWeight += (smPriceWeight ?? 0);

                        decimal feeVolume = 0;
                        var smallPackages = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.MainOrderId == oldMainOrder.Id).ToListAsync();
                        foreach (var smallPackage in smallPackages)
                        {
                            decimal volumeFeePrice = 0;
                            var volumeFeeSM = await unitOfWork.Repository<VolumeFee>().GetQueryable().Where(e => !e.Deleted &&
                                e.WarehouseFromId == oldMainOrder.FromPlace && e.WarehouseId == oldMainOrder.ReceivePlace && e.ShippingTypeToWareHouseId == oldMainOrder.ShippingType && e.IsHelpMoving == false &&
                                smallPackage.VolumePayment >= e.VolumeFrom && smallPackage.VolumePayment < e.VolumeTo).FirstOrDefaultAsync();

                            if (volumeFeeSM != null)
                                volumeFeePrice = volumeFeeSM.Price ?? 0;

                            decimal? smPriceVolume = 0;
                            if ((userMainOrder.FeeTQVNPerVolume ?? 0) > 0)
                                smPriceVolume = smallPackage.VolumePayment * userMainOrder.FeeTQVNPerVolume;
                            else
                                smPriceVolume = smallPackage.VolumePayment * volumeFeePrice;
                            feeVolume += (smPriceVolume ?? 0);

                        }
                        var userLevelMainOrder = await unitOfWork.Repository<UserLevel>().GetQueryable().Where(e => !e.Deleted && userMainOrder.LevelId == e.Id).FirstOrDefaultAsync();
                        decimal? ckFeeWeight = userLevelMainOrder == null ? 1 : userLevelMainOrder.FeeWeight;

                        decimal? feeDelivery = feeWeight > feeVolume ? feeWeight : feeVolume;
                        decimal? feeDeliveryDiscount = feeDelivery * ckFeeWeight / 100;
                        feeDelivery -= feeDeliveryDiscount;
                        oldMainOrder.TotalPriceVND = (oldMainOrder.TotalPriceVND - oldMainOrder.FeeWeight + feeDelivery);
                        oldMainOrder.FeeWeight = feeDelivery;
                        oldMainOrder.TQVNWeight = weight;
                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(history);
                        unitOfWork.Repository<MainOrder>().Update(oldMainOrder);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<MainOrder>().Detach(oldMainOrder);

                        await transactionDbContext.CommitAsync();
                    }
                    return true;

                }
                catch (Exception ex)
                {
                    await transactionDbContext.RollbackAsync();
                    throw new AppException(ex.Message);
                }
            }
        }
        public async Task<bool> UpdateMainOrderDelivery(int id, decimal deliveryPrice)
        {
            using (var transactionDbContext = Context.Database.BeginTransaction())
            {
                try
                {
                    var oldMainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == id);
                    if (deliveryPrice != (oldMainOrder.FeeWeight ?? 0))
                    {
                        var history = new HistoryOrderChange()
                        {
                            MainOrderId = oldMainOrder.Id,
                            HistoryContent = $"{LoginContext.Instance.CurrentUser.UserName} đã đổi phí vận chuyển từ {oldMainOrder.FeeWeight ?? 0} VND sang {deliveryPrice} VND.",
                            UID = LoginContext.Instance.CurrentUser.UserId,
                            Type = (int?)TypeHistoryOrderChange.CanNangDonHang,
                            CreatedBy = LoginContext.Instance.CurrentUser.UserName
                        };

                        oldMainOrder.TotalPriceVND = (oldMainOrder.TotalPriceVND - oldMainOrder.FeeWeight + deliveryPrice);
                        oldMainOrder.FeeWeight = deliveryPrice;
                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(history);
                        unitOfWork.Repository<MainOrder>().Update(oldMainOrder);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<MainOrder>().Detach(oldMainOrder);

                        await transactionDbContext.CommitAsync();
                    }
                    return true;

                }
                catch (Exception ex)
                {
                    await transactionDbContext.RollbackAsync();
                    throw new AppException(ex.Message);
                }
            }
        }
        /// <summary>
        /// Tính lại tiền khi thay đổi SmallPackage (công thức giống của UpdateAsync)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<MainOrder> PriceAdjustment(MainOrder item)
        {

            var smallPackages = item.SmallPackages;
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Id == item.UID).FirstOrDefaultAsync();
            var userLevel = await unitOfWork.Repository<UserLevel>().GetQueryable().Where(e => !e.Deleted && e.Id == user.LevelId).FirstOrDefaultAsync();
            decimal? ckFeeWeight = userLevel == null ? 0 : userLevel.FeeWeight;
            //decimal? ckFeeWeight = userLevel == null ? 1 : userLevel.FeeWeight;
            item.FeeWeightCK = ckFeeWeight;
            decimal? totalWeight = smallPackages.Sum(e => e.PayableWeight);
            decimal? totalVolume = smallPackages.Sum(e => e.VolumePayment);

            #region Tính phí vận chuyển của tổng cân nặng
            ////Fix lấy warehouseFee 
            //var warehouseFee = await unitOfWork.Repository<WarehouseFee>().GetQueryable()
            //    .Where(e => !e.Deleted &&
            //    e.WarehouseFromId == item.FromPlace &&
            //    e.WarehouseId == item.ReceivePlace &&
            //    e.ShippingTypeToWareHouseId == item.ShippingType &&
            //    e.IsHelpMoving == false &&
            //    (e.WeightFrom < totalWeight && e.WeightTo >= totalWeight)).FirstOrDefaultAsync();

            //decimal? warehouseFeePrice = warehouseFee == null ? 1 : warehouseFee.Price;

            //decimal? feeWeight = 0;
            //if (user.FeeTQVNPerWeight > 0)
            //{
            //    feeWeight = totalWeight * user.FeeTQVNPerWeight;
            //    smallPackages.ForEach(e => { e.PriceWeight = user.FeeTQVNPerWeight; e.DonGia = user.FeeTQVNPerWeight; e.TotalPrice = totalWeight * warehouseFeePrice; });
            //}
            //else
            //{
            //    feeWeight = totalWeight * warehouseFeePrice;
            //    smallPackages.ForEach(e => { e.PriceWeight = warehouseFeePrice; e.DonGia = warehouseFeePrice; e.TotalPrice = totalWeight * warehouseFeePrice; });
            //}
            #endregion
            #region Tính phí vận chuyển của từng mã vận đơn rồi cộng lại
            decimal? feeDelivery = 0;
            decimal? totalFeeWeight = 0;
            decimal? totalFeeVolume = 0;
            foreach (var smallPackage in smallPackages)
            {
                decimal? payableWeight = smallPackage.PayableWeight;
                decimal? smallPackageVolume = smallPackage.VolumePayment;
                decimal? feeWeight = 0;
                decimal? feeVolume = 0;

                //Tiền cân nặng
                var warehouseFee = await unitOfWork.Repository<WarehouseFee>().GetQueryable()
                    .Where(e => !e.Deleted &&
                    e.WarehouseFromId == item.FromPlace &&
                    e.WarehouseId == item.ReceivePlace &&
                    e.ShippingTypeToWareHouseId == item.ShippingType &&
                    e.IsHelpMoving == false &&
                    (payableWeight >= e.WeightFrom && payableWeight < e.WeightTo)).FirstOrDefaultAsync();
                if (warehouseFee == null)
                    throw new KeyNotFoundException("Không tìm thấy bảng giá cân nặng");
                decimal? warehouseFeePrice = warehouseFee == null ? 1 : warehouseFee.Price;

                if (user.FeeTQVNPerWeight > 0)
                {
                    feeWeight = payableWeight * user.FeeTQVNPerWeight;
                    smallPackage.PriceWeight = user.FeeTQVNPerWeight;
                    smallPackage.DonGia = user.FeeTQVNPerWeight;
                }
                else
                {
                    feeWeight = payableWeight * warehouseFeePrice;
                    smallPackage.PriceWeight = warehouseFeePrice;
                    smallPackage.DonGia = warehouseFeePrice;
                }
                //Tiền khối
                var volumeFee = await unitOfWork.Repository<VolumeFee>().GetQueryable().FirstOrDefaultAsync(e => !e.Deleted
                    && e.WarehouseFromId == item.FromPlace
                    && e.WarehouseId == item.ReceivePlace
                    && e.ShippingTypeToWareHouseId == item.ShippingType
                    && e.IsHelpMoving == false
                    && (smallPackageVolume >= e.VolumeFrom && smallPackageVolume < e.VolumeTo));
                if (volumeFee == null)
                    throw new KeyNotFoundException("Không tìm thấy bảng giá khối");
                if (user.FeeTQVNPerVolume > 0)
                {
                    feeVolume = smallPackageVolume * user.FeeTQVNPerVolume;
                    smallPackage.PriceVolume = user.FeeTQVNPerVolume;
                }
                else
                {
                    feeVolume = smallPackageVolume * volumeFee.Price;
                    smallPackage.PriceVolume = volumeFee.Price;
                }
                smallPackage.TotalPrice = feeVolume > feeWeight ? feeVolume : feeWeight;

                totalFeeVolume += feeVolume;
                totalFeeWeight += feeWeight;
                feeDelivery += feeVolume > feeWeight ? feeVolume : feeWeight;
            }
            #endregion

            decimal? feeDeliveryDiscount = feeDelivery * ckFeeWeight / 100;
            feeDelivery -= feeDeliveryDiscount;

            bool isChangeTQVNWeight = item.IsChangeFeeWeight != null ? item.IsChangeFeeWeight.Value : false;
            bool isChangeFeeWeight = item.IsChangeFeeWeight != null ? item.IsChangeFeeWeight.Value : false; ;


            if (isChangeTQVNWeight == false)
            {
                item.TQVNWeight = item.OrderWeight = Math.Round(totalWeight.Value, 2);
            }

            item.TQVNVolume = totalVolume;

            if (isChangeFeeWeight == false)
            {
                if (isChangeTQVNWeight == false)
                    item.FeeWeight = feeDelivery;
            }


            decimal? totalPriceVNDFn = 0;
            if (item.FeeWeight != null) totalPriceVNDFn += item.FeeWeight;
            if (item.FeeShipCN != null) totalPriceVNDFn += item.FeeShipCN;
            if (item.FeeBuyPro != null) totalPriceVNDFn += item.FeeBuyPro;
            if (item.IsCheckProductPrice != null) totalPriceVNDFn += item.IsCheckProductPrice;
            if (item.IsPackedPrice != null) totalPriceVNDFn += item.IsPackedPrice;
            if (item.IsFastDeliveryPrice != null) totalPriceVNDFn += item.IsFastDeliveryPrice;
            if (item.PriceVND != null) totalPriceVNDFn += item.PriceVND;
            if (item.Surcharge != null) totalPriceVNDFn += item.Surcharge;
            if (item.InsuranceMoney != null) totalPriceVNDFn += item.InsuranceMoney;
            item.TotalPriceVND = Math.Round(totalPriceVNDFn.Value, 0);
            return item;
        }

        public async Task<bool> UpdateStaff(MainOrder item)
        {
            //Tính phí đơn hàng
            foreach (var staffIncome in item.StaffIncomes)
            {
                if (staffIncome.Deleted)
                {
                    //Xóa luôn
                    unitOfWork.Repository<StaffIncome>().Delete(staffIncome);
                    continue;
                }

                if (staffIncome.Id == 0)
                    await unitOfWork.Repository<StaffIncome>().CreateAsync(staffIncome);
                else
                    unitOfWork.Repository<StaffIncome>().Update(staffIncome);
            }
            //Lịch sử thay đổi của đơn hàng
            await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(item.HistoryOrderChanges);

            unitOfWork.Repository<MainOrder>().Update(item);
            await unitOfWork.SaveAsync();
            return true;
        }

        /// <summary>
        /// Tính phí đơn hàng
        /// </summary>
        /// <param name="staffIncomes"></param>
        protected async void Commission(List<StaffIncome> staffIncomes)
        {
            foreach (var staffIncome in staffIncomes)
            {
                if (staffIncome.Deleted)
                {
                    //Xóa luôn
                    unitOfWork.Repository<StaffIncome>().Delete(staffIncome);
                    continue;
                }

                if (staffIncome.Id == 0)
                    await unitOfWork.Repository<StaffIncome>().CreateAsync(staffIncome);
                else
                    unitOfWork.Repository<StaffIncome>().Update(staffIncome);
            }
        }

        public async Task<AmountStatistic> GetTotalOrderPriceByUID(int UID)
        {
            var mainOrders = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(x => x.UID == UID && !x.Deleted).ToListAsync();
            return new AmountStatistic
            {
                TotalOrderPrice = mainOrders.Sum(x => x.TotalPriceVND) ?? 0,
                TotalPaidPrice = mainOrders.Sum(x => x.Deposit) ?? 0
            };
        }

        public async Task<bool> UpdateStatus(int ID, int status)
        {
            var mainOrder = await this.GetByIdAsync(ID);
            mainOrder.Status = status;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    unitOfWork.Repository<MainOrder>().Update(mainOrder);
                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                    unitOfWork.Repository<MainOrder>().Detach(mainOrder);
                    return true;
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task<bool> UpdateIsCheckNotiPrice(MainOrder mainOrder)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    unitOfWork.Repository<MainOrder>().Update(mainOrder);
                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                    unitOfWork.Repository<MainOrder>().Detach(mainOrder);
                    return true;
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

        public void UpdateMainOrderFromSql(string commandText)
        {

            SqlConnection connection = null;
            SqlCommand command = null;
            try
            {
                connection = (SqlConnection)Context.Database.GetDbConnection();
                command = connection.CreateCommand();
                connection.Open();
                if (!string.IsNullOrEmpty(commandText))
                {
                    command.CommandText = commandText;

                    using var reader = command.ExecuteReader();
                }
            }
            finally
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                    connection.Close();

                if (command != null)
                    command.Dispose();
            }
        }

        public List<NumberOfOrders> GetNumberOfOrders(MainOrderSearch mainOrderSearch)
        {
            var storeService = serviceProvider.GetRequiredService<IStoreSqlService<NumberOfOrders>>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", mainOrderSearch.UID));
            sqlParameters.Add(new SqlParameter("@RoleID", mainOrderSearch.RoleID));
            sqlParameters.Add(new SqlParameter("@OrderType", mainOrderSearch.OrderType));
            SqlParameter[] parameters = sqlParameters.ToArray();
            var data = storeService.GetDataFromStore(parameters, "GetNumberOfOrder");
            var all = data.Sum(x => x.Quantity);
            data.Add(new() { Status = -1, Quantity = all });
            if (data.Count != Enum.GetNames(typeof(StatusOrderContants)).Length)
            {
                int j = 0;
                foreach (var item in Enum.GetValues(typeof(StatusOrderContants)))
                {
                    if (data[j].Status != (int)item)
                        data.Add(new() { Status = (int)item, Quantity = 0 });
                    else
                        j++;
                }
            }

            return data;
        }

        public PriceInMonth GetPriceInMonth(MainOrderSearch mainOrderSearch)
        {
            var storeService = serviceProvider.GetRequiredService<IStoreSqlService<PriceInMonth>>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", mainOrderSearch.UID));
            sqlParameters.Add(new SqlParameter("@RoleID", mainOrderSearch.RoleID));
            sqlParameters.Add(new SqlParameter("@OrderType", mainOrderSearch.OrderType));
            sqlParameters.Add(new SqlParameter("@FromDate", mainOrderSearch.FromDate));
            sqlParameters.Add(new SqlParameter("@ToDate", mainOrderSearch.ToDate));
            SqlParameter[] parameters = sqlParameters.ToArray();
            var data = storeService.GetDataFromStore(parameters, "GetPriceInMonth");

            return data.FirstOrDefault();
        }

        public MainOrdersInfor GetMainOrdersInfor(int UID, int orderType)
        {
            var storeService = serviceProvider.GetRequiredService<IStoreSqlService<MainOrdersInfor>>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", UID));
            sqlParameters.Add(new SqlParameter("@OrderType", orderType));
            SqlParameter[] parameters = sqlParameters.ToArray();
            var data = storeService.GetDataFromStore(parameters, "GetMainOrdersInfor");

            return data.FirstOrDefault();
        }

        public MainOrdersAmount GetMainOrdersAmount(int UID, int orderType)
        {
            var storeService = serviceProvider.GetRequiredService<IStoreSqlService<MainOrdersAmount>>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", UID));
            sqlParameters.Add(new SqlParameter("@OrderType", orderType));
            SqlParameter[] parameters = sqlParameters.ToArray();
            var data = storeService.GetDataFromStore(parameters, "GetMainOrdersAmount");

            return data.FirstOrDefault();
        }
        public CountAllOrder GetCountAllOrder(MainOrderSearch mainOrderSearch)
        {
            var storeService = serviceProvider.GetRequiredService<IStoreSqlService<CountAllOrder>>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", mainOrderSearch.UID));
            sqlParameters.Add(new SqlParameter("@RoleID", mainOrderSearch.RoleID));
            SqlParameter[] parameters = sqlParameters.ToArray();
            var data = storeService.GetDataFromStore(parameters, "GetCountOrder");
            return data.FirstOrDefault();
        }

        //Code ngu cần sửa lại, dùng cho xuất tất cả
        public byte[] GetMainOrdersExcel(MainOrderSearch mainOrderSearch)
        {
            var storeService = serviceProvider.GetRequiredService<IStoreSqlService<MainOrderExcelModel>>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", mainOrderSearch.UID));
            sqlParameters.Add(new SqlParameter("@RoleID", mainOrderSearch.RoleID));
            sqlParameters.Add(new SqlParameter("@Status", mainOrderSearch.Status));
            sqlParameters.Add(new SqlParameter("@OrderType", mainOrderSearch.OrderType));
            sqlParameters.Add(new SqlParameter("@FromDate", mainOrderSearch.FromDate));
            sqlParameters.Add(new SqlParameter("@ToDate", mainOrderSearch.ToDate));
            SqlParameter[] parameters = sqlParameters.ToArray();
            var data = storeService.GetDataTableFromStore(parameters, "ExcelMainOrder");
            using (var workBook = new XLWorkbook())
            {
                var worksheet = workBook.Worksheets.Add();
                worksheet.ColumnWidth = 20;
                var currentRow = 1;
                for (int i = 0; i < MainOrderExcelColumns.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i + 1).Style.Font.FontSize = 14;
                    worksheet.Cell(currentRow, i + 1).Value = MainOrderExcelColumns[i];
                }
                foreach (DataRow item in data.Rows)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item["Id"].ToString();
                    worksheet.Cell(currentRow, 2).Value = item["UserName"].ToString();
                    worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 3).Value = item["TotalPriceVND"];
                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 4).Value = item["Deposit"];
                    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 5).Value = item["RemainingAmount"];
                    worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 6).Value = item["PriceVND"];
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 7).Value = item["FeeBuyProPT"];
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 8).Value = item["FeeShipCN"];
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 9).Value = item["IsCheckProductPrice"];
                    worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 10).Value = item["IsPackedPrice"];
                    worksheet.Cell(currentRow, 11).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 11).Value = item["InsuranceMoney"];
                    worksheet.Cell(currentRow, 12).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 12).Value = item["Surcharge"];
                    worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 13).Value = item["FeeWeight"];
                    worksheet.Cell(currentRow, 14).Value = item["OrderWeight"].ToString();
                    worksheet.Cell(currentRow, 15).Value = item["StatusName"].ToString();
                    worksheet.Cell(currentRow, 16).Value = item["SalerUserName"].ToString();
                    worksheet.Cell(currentRow, 17).Value = item["OrdererUserName"].ToString();
                    worksheet.Cell(currentRow, 18).Value = item["Created"].ToString();
                    worksheet.Cell(currentRow, 19).Value = item["DepositDate"].ToString();
                    worksheet.Cell(currentRow, 20).Value = item["DateBuy"].ToString();
                    worksheet.Cell(currentRow, 21).Value = item["DateTQ"].ToString();
                    worksheet.Cell(currentRow, 22).Value = item["DateVN"].ToString();
                    worksheet.Cell(currentRow, 23).Value = item["CompleteDate"].ToString();
                }
                using (var stream = new MemoryStream())
                {
                    workBook.SaveAs(stream);
                    var content = stream.ToArray();
                    return content;
                }
            }
        }
    }
}
