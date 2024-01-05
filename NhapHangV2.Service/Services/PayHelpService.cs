using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class PayHelpService : DomainService<PayHelp, PayHelpSearch>, IPayHelpService
    {
        protected readonly IAppDbContext Context;
        protected readonly IUserService userService;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IUserInGroupService userInGroupService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly IServiceProvider serviceProvider;

        public PayHelpService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            this.serviceProvider = serviceProvider;
            userService = serviceProvider.GetRequiredService<IUserService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
        }

        protected override string GetStoreProcName()
        {
            return "PayHelp_2_GetPagingData";
        }

        public List<CountStatusData> CountStatus(PayHelpSearch payHelpSearch)
        {
            var storeService = serviceProvider.GetRequiredService<IStoreSqlService<CountStatusData>>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@UID", payHelpSearch.UID));
            sqlParameters.Add(new SqlParameter("@RoleID", payHelpSearch.RoleID));
            SqlParameter[] parameters = sqlParameters.ToArray();
            var data = storeService.GetDataFromStore(parameters, "CountPayHelpStatus");
            var all = data.Sum(x => x.Quantity);
            data.Add(new() { Status = -1, Quantity = all });
            if (data.Count != Enum.GetNames(typeof(StatusPayHelp)).Length + 1)
            {
                int j = 0;
                foreach (var item in Enum.GetValues(typeof(StatusPayHelp)))
                {
                    if (data[j].Status != (int)item)
                        data.Add(new() { Status = (int)item, Quantity = 0 });
                    else
                        j++;
                }
            }

            return data;
        }

        public async Task<bool> UpdateStatus(PayHelp model, int status, int statusOld, decimal? totalPriceVNDOld)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    DateTime currentDate = DateTime.Now;
                    var user = LoginContext.Instance.CurrentUser;
                    var userRequest = await userService.GetByIdAsync(model.UID ?? 0);
                    model.Updated = DateTime.Now;
                    model.UpdatedBy = user.UserName;
                    string oldStatusText = "";
                    switch (statusOld)
                    {
                        case (int)StatusPayHelp.DonHuy:
                            oldStatusText = "Đơn hủy";
                            break;
                        case (int)StatusPayHelp.ChoDuyet:
                            oldStatusText = "Chờ duyệt";
                            break;
                        case (int)StatusPayHelp.DaDuyet:
                            oldStatusText = "Đã duyệt";
                            break;
                        case (int)StatusPayHelp.DaThanhToan:
                            oldStatusText = "Đã thanh toán";
                            break;
                        case (int)StatusPayHelp.DaHoanThanh:
                            oldStatusText = "Đã hoàn thành";
                            break;
                        default:
                            oldStatusText = string.Empty;
                            break;
                    }
                    string newStatusText = "";
                    switch (status)
                    {
                        case (int)StatusPayHelp.DonHuy:
                            newStatusText = "Đơn hủy";
                            break;
                        case (int)StatusPayHelp.ChoDuyet:
                            newStatusText = "Chờ duyệt";
                            break;
                        case (int)StatusPayHelp.DaDuyet:
                            newStatusText = "Đã duyệt";
                            break;
                        case (int)StatusPayHelp.DaThanhToan:
                            newStatusText = "Đã thanh toán";
                            break;
                        case (int)StatusPayHelp.DaHoanThanh:
                            newStatusText = "Đã hoàn thành";
                            break;
                        default:
                            newStatusText = string.Empty;
                            break;
                    }

                    foreach (var payHelpDetail in model.PayHelpDetails)
                    {
                        unitOfWork.Repository<PayHelpDetail>().Update(payHelpDetail);
                        await unitOfWork.SaveAsync();
                    }

                    if (status == statusOld && totalPriceVNDOld == model.TotalPriceVND)
                    {
                        unitOfWork.Repository<PayHelp>().Update(model);
                        await unitOfWork.SaveAsync();
                        await dbContextTransaction.CommitAsync();
                        return true;
                    }

                    var staffIncome = await unitOfWork.Repository<StaffIncome>().GetQueryable().FirstOrDefaultAsync(x => x.PayHelpOrderId == model.Id);
                    switch (status)
                    {
                        case (int)StatusPayHelp.DonHuy:
                            if (model.CancelDate == null)
                                model.CancelDate = currentDate;
                            //Nếu đơn khác trạng thái hoàn thành và đã có hoa hồng thì xóa hoa hồng đó đi
                            if (staffIncome != null)
                            {
                                staffIncome.Deleted = true;
                                await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffIncome, new Expression<Func<StaffIncome, object>>[]
                                {
                                    s =>s.Deleted
                                });
                            }
                            if (model.Status > (int)StatusPayHelp.DaDuyet)
                            {
                                //Trả tiền lại ví người dùng
                                userRequest.Wallet += model.TotalPriceVND;
                                //Tính tiền tích lũy
                                userRequest = await userService.CreateUserTransactionMoney(userRequest, 0 - (model.TotalPriceVND ?? 0));
                                unitOfWork.Repository<Users>().Update(userRequest);
                                //Lịch sử ví tiền
                                await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                                {
                                    UID = userRequest.Id,
                                    MoneyLeft = userRequest.Wallet,
                                    Amount = model.TotalPriceVND,
                                    Type = (int)DauCongVaTru.Cong,
                                    TradeType = (int)HistoryPayWalletContents.HoanTienThanhToanHo,
                                    Content = string.Format("Hoàn tiền thanh toán hộ đơn: {0}.", model.Id),
                                });
                            }
                            break;
                        case (int)StatusPayHelp.DaDuyet:
                            //Nếu đơn khác trạng thái hoàn thành và đã có hoa hồng thì xóa hoa hồng đó đi
                            if (staffIncome != null)
                            {
                                staffIncome.Deleted = true;
                                await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffIncome, new Expression<Func<StaffIncome, object>>[]
                                {
                                    s =>s.Deleted
                                });
                            }
                            if (model.ConfirmDate == null)
                                model.ConfirmDate = DateTime.Now;
                            break;

                        case (int)StatusPayHelp.DaThanhToan:
                            //Nếu đơn khác trạng thái hoàn thành và đã có hoa hồng thì xóa hoa hồng đó đi
                            if (staffIncome != null)
                            {
                                staffIncome.Deleted = true;
                                await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffIncome, new Expression<Func<StaffIncome, object>>[]
                                {
                                    s =>s.Deleted
                                });
                            }
                            bool isSuccess = false;
                            //Kiểm tra
                            if (model.Status < (int?)StatusPayHelp.DaDuyet) //Không đúng trạng thái
                                throw new AppException(string.Format("Đơn chưa được duyệt"));

                            decimal wallet = userRequest.Wallet ?? 0;

                            decimal totalPriceVND = (model.TotalPriceVND ?? 0) - (model.Deposit ?? 0);

                            if (wallet < totalPriceVND)
                                throw new AppException("Số dư không đủ để thanh toán");
                            decimal walletleft = wallet - totalPriceVND;
                            userRequest.Wallet = walletleft;
                            userRequest.Updated = currentDate;
                            userRequest.UpdatedBy = user.UserName;
                            //Tính tiền tích lũy
                            userRequest = await userService.CreateUserTransactionMoney(userRequest, totalPriceVND);
                            unitOfWork.Repository<Users>().Update(userRequest);

                            await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                            {
                                UID = userRequest.Id,
                                MainOrderId = 0,
                                MoneyLeft = walletleft,
                                Amount = totalPriceVND,
                                Type = (int)DauCongVaTru.Tru,
                                TradeType = (int)HistoryPayWalletContents.ThanhToanThanhToanHo,
                                Content = string.Format("{0} đã trả tiền thanh toán hộ đơn: {1}.", userRequest.UserName, model.Id),
                                Deleted = false,
                                Active = true,
                                Created = currentDate,
                                CreatedBy = user.UserName
                            });
                            isSuccess = true;
                            model.Deposit = (model.Deposit ?? 0) + totalPriceVND;
                            if (model.PaidDate == null)
                                model.PaidDate = currentDate;
                            break;
                        case (int)StatusPayHelp.DaHoanThanh:
                            //Nếu đơn đã hoàn thành mà chưa có hoa hồng thì thêm hoa hồng thanh toán hộ
                            if (model.Status != (int)StatusPayHelp.DaThanhToan)
                                throw new AppException("Đơn chưa thanh toán");
                            if (staffIncome == null)
                            {
                                var staffIncomeNew = await CommissionPayHelpOrder(model);
                                if (staffIncomeNew != null)
                                    await unitOfWork.Repository<StaffIncome>().CreateAsync(staffIncomeNew);
                            }
                            if (model.CompleteDate == null)
                                model.CompleteDate = currentDate;
                            break;
                        default:
                            //Nếu đơn khác trạng thái hoàn thành và đã có hoa hồng thì xóa hoa hồng đó đi
                            if (staffIncome != null)
                            {
                                staffIncome.Deleted = true;
                                await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffIncome, new Expression<Func<StaffIncome, object>>[]
                                {
                                    s =>s.Deleted
                                });
                            }
                            if (status == (int)StatusPayHelp.DaDuyet && model.ConfirmDate == null)
                                model.ConfirmDate = currentDate;
                            break;
                    }
                    model.Status = status;
                    unitOfWork.Repository<PayHelp>().Update(model);
                    await unitOfWork.Repository<HistoryServices>().CreateAsync(new HistoryServices
                    {
                        PostId = model.Id,
                        UID = user.UserId,
                        OldStatus = statusOld,
                        OldeStatusText = oldStatusText,
                        NewStatus = status,
                        NewStatusText = newStatusText,
                        Type = (int)TypeHistoryServices.ThanhToanHo,
                        Note = $"{user.UserName} đã đổi trạng thái đơn # {model.Id} từ {oldStatusText} sang {newStatusText}"
                    });
                    if (status == ((int)StatusPayHelp.DaThanhToan) && user.UserGroupId == 2)
                    {
                        var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.ThanhToanThanhToanHo);
                        sendNotificationService.SendNotification(notificationSetting,
                            new List<string>() { model.Id.ToString(), user.UserName },
                            new UserNotification() { SaleId = model.SalerID });
                    }
                    else
                    {
                        var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiThanhToanHo);
                        sendNotificationService.SendNotification(notificationSetting,
                            new List<string>() { model.Id.ToString(), user.UserName, oldStatusText, newStatusText },
                            new UserNotification() { UserId = model.UID, SaleId = model.SalerID });
                    }
                    await unitOfWork.SaveAsync();
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

        public override async Task<bool> CreateAsync(PayHelp item)
        {
            var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
            int salerID = user.SaleId ?? 0;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var configurations = await configurationsService.GetSingleAsync();
                    decimal pcConfig = configurations == null ? 0 : configurations.PricePayHelpDefault ?? 0;

                    //Tính tổng tiền
                    decimal? price = item.PayHelpDetails.Sum(x => x.Desc1);
                    if (price <= 0)
                        throw new AppException("Vui lòng nhập số tiền");
                    decimal? pC = await configurationsService.GetCurrentPayHelp(price ?? 0);
                    decimal? totalPrice = price * pC;

                    item.UID = user.Id;
                    item.TotalPrice = price;
                    item.TotalPriceVND = totalPrice;
                    item.Currency = pC;
                    item.CurrencyConfig = pcConfig;
                    item.TotalPriceVNDGiaGoc = pcConfig * price;
                    item.Status = (int)StatusPayHelp.ChoDuyet;
                    item.Deposit = 0;
                    item.SalerID = salerID;
                    await unitOfWork.Repository<PayHelp>().CreateAsync(item);
                    await unitOfWork.SaveAsync();

                    item.PayHelpDetails.ForEach(e => e.PayHelpId = item.Id);
                    await unitOfWork.Repository<PayHelpDetail>().CreateAsync(item.PayHelpDetails);

                    //Thông báo có đơn thanh toán hộ mới
                    var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TaoThanhToanHo);
                    sendNotificationService.SendNotification(notificationSetting,
                        new List<string>() { item.Id.ToString() },
                        new UserNotification() { SaleId = item.SalerID });
                    await unitOfWork.SaveAsync();
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

        public override async Task<PayHelp> GetByIdAsync(int id)
        {
            var payHelp = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (payHelp == null) return null;
            var user = await userService.GetByIdAsync(payHelp.UID ?? 0);
            if (user != null)
                payHelp.UserName = user.UserName;

            var historyServicess = await unitOfWork.Repository<HistoryServices>().GetQueryable().Where(e => !e.Deleted && e.Active && e.PostId == payHelp.Id).OrderByDescending(o => o.Id).ToListAsync();
            if (historyServicess != null)
            {
                payHelp.HistoryServicess = historyServicess;
                foreach (var historyService in payHelp.HistoryServicess)
                {
                    var userHistory = await userService.GetByIdAsync(historyService.UID ?? 0);
                    if (userHistory == null)
                        continue;
                    historyService.UserName = userHistory.UserName;
                }
            }

            var payHelpDetails = await unitOfWork.Repository<PayHelpDetail>().GetQueryable().Where(e => !e.Deleted && e.Active && e.PayHelpId == payHelp.Id).OrderByDescending(o => o.Id).ToListAsync();
            if (payHelpDetails != null)
                payHelp.PayHelpDetails = payHelpDetails;
            return payHelp;
        }

        public async Task<AmountStatistic> GetTotalOrderPriceByUID(int UID)
        {
            var payHelps = await unitOfWork.Repository<PayHelp>().GetQueryable().Where(x => x.UID == UID && !x.Deleted).ToListAsync();
            return new AmountStatistic
            {
                TotalOrderPrice = payHelps.Sum(x => x.TotalPriceVND) ?? 0,
                TotalPaidPrice = payHelps.Sum(x => x.Deposit) ?? 0
            };
        }

        public async Task<bool> UpdateStaff(PayHelp payHelp, int oldSalerId)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var currentUser = LoginContext.Instance.CurrentUser;
                    switch (payHelp.Status)
                    {
                        case (int)StatusPayHelp.DaHoanThanh:
                            if (payHelp.SalerID != oldSalerId)
                            {
                                var staffIncome = await unitOfWork.Repository<StaffIncome>().GetQueryable().FirstOrDefaultAsync(x => x.PayHelpOrderId == payHelp.Id && !x.Deleted);
                                if (staffIncome != null)
                                {
                                    //Xóa hoa hồng hiện tại của đơn
                                    staffIncome.Deleted = true;
                                    await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffIncome, new Expression<Func<StaffIncome, object>>[]
                                    {
                                        s =>s.Deleted
                                    });
                                }
                                //Thêm hoa hồng mới
                                var staffIncomeNew = await CommissionPayHelpOrder(payHelp);
                                if (staffIncomeNew != null)
                                    await unitOfWork.Repository<StaffIncome>().CreateAsync(staffIncomeNew);
                            }
                            break;
                        default:
                            break;
                    }
                    await unitOfWork.Repository<PayHelp>().UpdateFieldsSaveAsync(payHelp, new Expression<Func<PayHelp, object>>[]
                    {
                        x => x.SalerID
                    });
                    if (payHelp.SalerID != oldSalerId)
                    {
                        await unitOfWork.Repository<HistoryServices>().CreateAsync(new HistoryServices
                        {
                            PostId = payHelp.Id,
                            UID = payHelp.UID,
                            OldStatus = payHelp.Status,
                            Type = (int)TypeHistoryServices.ThanhToanHo,
                            Note = $"{currentUser} đã đổi nhân viên Sale của đơn #{payHelp.Id} từ {userService.GetSaleName(oldSalerId)} sang {userService.GetSaleName(payHelp.SalerID)}"
                        });
                    }
                    await unitOfWork.SaveAsync();
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
        public async Task<StaffIncome> CommissionPayHelpOrder(PayHelp payHelp)
        {
            var configuration = await configurationsService.GetSingleAsync();
            if (configuration == null)
                throw new KeyNotFoundException("Không tìm thấy cấu hình hệ thống");
            //Không có SalerId
            if (payHelp.SalerID == null || payHelp.SalerID < 1)
                return null;
            decimal orderTotalPrice = payHelp.TotalPriceVND ?? 0;
            int percentRecevie = configuration.SaleTranportationPersent ?? 0;
            decimal profit = (payHelp.TotalPriceVND ?? 0) - (payHelp.TotalPriceVNDGiaGoc ?? 0);
            decimal totalPriceRecieve = 0;
            if (profit > 0 && percentRecevie > 0)
            {
                totalPriceRecieve = profit * percentRecevie / 100;
            }
            return new StaffIncome()
            {
                PayHelpOrderId = payHelp.Id,
                OrderTotalPrice = orderTotalPrice,
                PercentReceive = percentRecevie,
                UID = payHelp.SalerID,
                TotalPriceReceive = totalPriceRecieve
            };
        }
    }
}
