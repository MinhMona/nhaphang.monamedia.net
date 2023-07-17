using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Auth;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Request;
using NhapHangV2.Service.Services.Configurations;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        private readonly INotificationTemplateService notificationTemplateService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly ISMSEmailTemplateService sMSEmailTemplateService;

        public PayHelpService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            userService = serviceProvider.GetRequiredService<IUserService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            notificationTemplateService = serviceProvider.GetRequiredService<INotificationTemplateService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
            sMSEmailTemplateService = serviceProvider.GetRequiredService<ISMSEmailTemplateService>();

        }

        protected override string GetStoreProcName()
        {
            return "PayHelp_2_GetPagingData";
        }

        public async Task<bool> UpdateStatus(PayHelp model, int status, int statusOld)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    DateTime currentDate = DateTime.Now;
                    var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
                    var userRequest = await userService.GetByIdAsync(model.UID ?? 0);
                    model.Updated = DateTime.Now;
                    model.UpdatedBy = user.UserName;
                    string oldStatusText = "";
                    switch (statusOld)
                    {
                        case (int)StatusPayHelp.ChuaThanhToan:
                            oldStatusText = "Chưa thanh toán";
                            break;
                        case (int)StatusPayHelp.DaThanhToan:
                            oldStatusText = "Đã thanh toán";
                            break;
                        case (int)StatusPayHelp.DaHuy:
                            oldStatusText = "Đã hủy";
                            break;
                        case (int)StatusPayHelp.DaHoanThanh:
                            oldStatusText = "Hoàn thành";
                            break;
                        case (int)StatusPayHelp.DaXacNhan:
                            oldStatusText = "Đã xác nhận";
                            break;
                        default:
                            oldStatusText = string.Empty;
                            break;
                    }
                    string newStatusText = "";
                    switch (status)
                    {
                        case (int)StatusPayHelp.ChuaThanhToan:
                            newStatusText = "Chưa thanh toán";
                            break;
                        case (int)StatusPayHelp.DaThanhToan:
                            newStatusText = "Đã thanh toán";
                            break;
                        case (int)StatusPayHelp.DaHuy:
                            newStatusText = "Đã hủy";
                            break;
                        case (int)StatusPayHelp.DaHoanThanh:
                            newStatusText = "Hoàn thành";
                            break;
                        case (int)StatusPayHelp.DaXacNhan:
                            newStatusText = "Đã xác nhận";
                            break;
                        default:
                            newStatusText = string.Empty;
                            break;
                    }

                    foreach (var payHelpDetail in model.PayHelpDetails)
                    {
                        unitOfWork.Repository<PayHelpDetail>().Update(payHelpDetail);
                    }

                    if (status == statusOld)
                    {
                        unitOfWork.Repository<PayHelp>().Update(model);
                        await unitOfWork.SaveAsync();
                        await dbContextTransaction.CommitAsync();
                        return true;
                    }

                    var staffIncome = await unitOfWork.Repository<StaffIncome>().GetQueryable().FirstOrDefaultAsync(x => x.PayHelpOrderId == model.Id);
                    switch (status)
                    {
                        case (int)StatusPayHelp.DaHuy:
                            //Nếu đơn khác trạng thái hoàn thành và đã có hoa hồng thì xóa hoa hồng đó đi
                            if (staffIncome != null)
                            {
                                staffIncome.Deleted = true;
                                await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffIncome, new Expression<Func<StaffIncome, object>>[]
                                {
                                    s =>s.Deleted
                                });
                            }
                            if (model.Status == (int)StatusPayHelp.ChuaThanhToan || model.Status == (int)StatusPayHelp.DaXacNhan)
                            {
                                model.Status = (int)StatusPayHelp.DaHuy;
                                unitOfWork.Repository<PayHelp>().Update(model);
                            }
                            else
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
                                    TradeType = (int)HistoryPayWalletContents.ThanhToanHo, //Chưa hiểu nè
                                    Content = string.Format("Hoàn tiền thanh toán hộ đơn: {0}.", model.Id),
                                });

                                await unitOfWork.Repository<HistoryServices>().CreateAsync(new HistoryServices
                                {
                                    PostId = model.Id,
                                    UID = userRequest.Id,
                                    OldStatus = statusOld,
                                    OldeStatusText = oldStatusText,
                                    NewStatus = status,
                                    NewStatusText = newStatusText,
                                    Type = (int)TypeHistoryServices.ThanhToanHo,
                                    Note = string.Format("Hoàn tiền thanh toán hộ. Trạng thái từ {0} sang {1}", oldStatusText, newStatusText)
                                });

                                model.Status = (int)StatusPayHelp.DaHuy;
                                unitOfWork.Repository<PayHelp>().Update(model);

                                var notificationSetting = await notificationSettingService.GetByIdAsync(18);
                                var notiTemplateDaHuy = await notificationTemplateService.GetByIdAsync(26);
                                var emailTemplateTQ = await sMSEmailTemplateService.GetByCodeAsync("UTTHBH");
                                string subjectTQ = emailTemplateTQ.Subject;
                                string emailContentTQ = string.Format(emailTemplateTQ.Body);
                                await sendNotificationService.SendNotification(notificationSetting, notiTemplateDaHuy, model.Id.ToString(), String.Format(Detail_MainOrder_Admin, model.Id), String.Format(Detail_MainOrder, model.Id), userRequest.Id, string.Empty, string.Empty);
                            }
                            break;
                        case (int)StatusPayHelp.ChuaThanhToan:
                            //Nếu đơn khác trạng thái hoàn thành và đã có hoa hồng thì xóa hoa hồng đó đi
                            if (staffIncome != null)
                            {
                                staffIncome.Deleted = true;
                                await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffIncome, new Expression<Func<StaffIncome, object>>[]
                                {
                                    s =>s.Deleted
                                });
                            }
                            model.Status = (int)StatusPayHelp.ChuaThanhToan;
                            unitOfWork.Repository<PayHelp>().Update(model);
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
                            if (model.Status != (int?)StatusPayHelp.DaXacNhan) //Không đúng trạng thái
                                throw new AppException(string.Format("Đơn này bị sai trạng thái để Thanh toán, vui lòng kiểm tra lại"));

                            decimal wallet = userRequest.Wallet ?? 0;

                            decimal totalPriceVND = model.TotalPriceVND ?? 0;

                            if (wallet < totalPriceVND)
                                throw new AppException("Tài khoản yêu cầu không đủ số dư để thanh toán");
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
                                TradeType = (int)HistoryPayWalletContents.ThanhToanHo,
                                Content = string.Format("{0} đã trả tiền thanh toán hộ đơn: {1}.", userRequest.UserName, model.Id),
                                Deleted = false,
                                Active = true,
                                Created = currentDate,
                                CreatedBy = user.UserName
                            });

                            await unitOfWork.Repository<HistoryServices>().CreateAsync(new HistoryServices
                            {
                                PostId = model.Id,
                                UID = userRequest.Id,
                                OldStatus = statusOld,
                                OldeStatusText = oldStatusText,
                                NewStatus = (int)StatusPayHelp.DaThanhToan,
                                NewStatusText = "Đã thanh toán",
                                Type = (int)TypeHistoryServices.ThanhToanHo,
                                Note = string.Format("{0} đã trả tiền thanh toán hộ {1} VNĐ. Trạng thái từ {2} sang Đã thanh toán", user.UserName, string.Format("{0:N0}", totalPriceVND), oldStatusText),
                                Deleted = false,
                                Active = true,
                                Created = currentDate,
                                CreatedBy = user.UserName
                            });

                            isSuccess = true;
                            model.Status = (int?)StatusPayHelp.DaThanhToan;
                            model.Deposit = totalPriceVND;
                            unitOfWork.Repository<PayHelp>().Update(model);

                            //Thông báo đã thanh toán đơn thanh toán hộ
                            if (isSuccess)
                            {
                                var notificationSetting = await notificationSettingService.GetByIdAsync(18);
                                //Thông báo cho user 
                                var notiTemplateUser = await notificationTemplateService.GetByIdAsync(25);
                                var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync("ADTTTTH");
                                string subject = emailTemplate.Subject;
                                string emailContent = string.Format(emailTemplate.Body);
                                await sendNotificationService.SendNotification(notificationSetting, notiTemplateUser, model.Id.ToString(),
                                    String.Format(Detail_Payhelp_Admin, model.Id), String.Format(Detail_Payhelp, model.Id), userRequest.Id, subject, emailContent);

                                //Thông báo cho Admin và manager
                                var notiTemplateAdmin = await notificationTemplateService.GetByIdAsync(21);
                                notiTemplateAdmin.Content = user.UserName + "đã trả tiền thanh toán hộ cho đơn: {0}";
                                await sendNotificationService.SendNotification(notificationSetting, notiTemplateAdmin, model.Id.ToString(),
                                    String.Format(Detail_Payhelp_Admin, model.Id), String.Format(Detail_Payhelp, model.Id), null, subject, emailContent);
                            }
                            break;
                        case (int)StatusPayHelp.DaHoanThanh:
                            //Nếu đơn đã hoàn thành mà chưa có hoa hồng thì thêm hoa hồng thanh toán hộ
                            if (staffIncome == null)
                            {
                                var staffIncomeNew = await CommissionPayHelpOrder(model);
                                if (staffIncomeNew != null)
                                    await unitOfWork.Repository<StaffIncome>().CreateAsync(staffIncomeNew);
                            }
                            if (model.Status != (int)StatusPayHelp.DaThanhToan)
                                throw new AppException("Đơn chưa thanh toán");
                            model.Status = (int?)StatusPayHelp.DaHoanThanh;

                            await unitOfWork.Repository<HistoryServices>().CreateAsync(new HistoryServices
                            {
                                PostId = model.Id,
                                UID = userRequest.Id,
                                OldStatus = statusOld,
                                OldeStatusText = oldStatusText,
                                NewStatus = (int)StatusPayHelp.DaHoanThanh,
                                NewStatusText = "Đã hoàn thành",
                                Type = (int)TypeHistoryServices.ThanhToanHo,
                                Note = string.Format("{0} đã đổi trạng thái sang Đã hoàn thành", user.UserName),
                                Deleted = false,
                                Active = true,
                                Created = currentDate,
                                CreatedBy = user.UserName
                            });
                            unitOfWork.Repository<PayHelp>().Update(model);
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
                            await unitOfWork.Repository<HistoryServices>().CreateAsync(new HistoryServices
                            {
                                PostId = model.Id,
                                UID = userRequest.Id,
                                OldStatus = statusOld,
                                OldeStatusText = oldStatusText,
                                NewStatus = status,
                                NewStatusText = newStatusText,
                                Type = (int)TypeHistoryServices.ThanhToanHo,
                                Note = string.Format("{0} đã thay đôi trạng thái từ {1} sang {2}", user.UserName, oldStatusText, newStatusText),
                            });
                            model.Status = status;
                            unitOfWork.Repository<PayHelp>().Update(model);
                            break;
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
                    item.Status = (int)StatusPayHelp.ChuaThanhToan;
                    item.Deposit = 0;
                    item.SalerID = salerID;
                    await unitOfWork.Repository<PayHelp>().CreateAsync(item);
                    await unitOfWork.SaveAsync();

                    item.PayHelpDetails.ForEach(e => e.PayHelpId = item.Id);
                    await unitOfWork.Repository<PayHelpDetail>().CreateAsync(item.PayHelpDetails);

                    await unitOfWork.Repository<HistoryServices>().CreateAsync(new HistoryServices
                    {
                        PostId = item.Id,
                        UID = user.Id,
                        OldStatus = 0, //Chưa hiểu
                        OldeStatusText = "",
                        NewStatus = (int)StatusPayHelp.ChuaThanhToan,
                        NewStatusText = "Chưa thanh toán",
                        Type = (int)TypeHistoryServices.ThanhToanHo,
                        Note = string.Format("{0} đã tạo đơn thanh toán hộ.", user.UserName),
                    });

                    //Thông báo có đơn thanh toán hộ mới
                    var notiTemplate = await notificationTemplateService.GetByIdAsync(22);
                    var notificationSetting = await notificationSettingService.GetByIdAsync(18);
                    var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync("ACDTTHM");
                    string subject = emailTemplate.Subject;
                    string emailContent = string.Format(emailTemplate.Body);
                    await sendNotificationService.SendNotification(notificationSetting, notiTemplate, item.Id.ToString(), String.Format(Detail_Payhelp_Admin, item.Id), "", null, string.Empty, string.Empty);

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

            //var configuration = await configurationsService.GetSingleAsync();
            //payHelp.CurrencyConfig = configuration.Currency;

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
