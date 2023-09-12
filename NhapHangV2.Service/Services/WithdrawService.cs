using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
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
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class WithdrawService : DomainService<Withdraw, WithdrawSearch>, IWithdrawService
    {
        protected readonly IUserService userService;
        protected readonly IUserInGroupService userInGroupService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly ISendNotificationService sendNotificationService;
        public WithdrawService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            userService = serviceProvider.GetRequiredService<IUserService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
        }

        public async Task<BillInfor> GetBillInforAsync(int id)
        {
            var item = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (item != null)
            {
                var user = await userService.GetByIdAsync(item.UID ?? 0);
                var billInfor = new BillInfor()
                {
                    UserName = user.FullName,
                    UserAddress = user.Address,
                    Note = item.Note,
                    Amount = item.Amount ?? 0
                };
                return billInfor;
            }
            throw new EntryPointNotFoundException("Không tìm thấy yêu cầu");
        }

        public override async Task<Withdraw> GetByIdAsync(int id)
        {
            var item = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (item == null)
                return null;
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == item.UID && !e.Deleted).FirstOrDefaultAsync();
            if (user != null)
                item.UserName = user.UserName;
            return item;
        }

        protected override string GetStoreProcName()
        {
            return "Withdraw_GetPagingData";
        }

        public async Task<bool> UpdateStatus(Withdraw item, int status)
        {
            var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId); //User

            if (user == null) throw new KeyNotFoundException("Không tìm thấy User");
            switch (status)
            {
                case (int)WalletStatus.DaDuyet: //Duyệt
                    switch (item.Type)
                    {
                        case (int)WithdrawTypes.RutTien:

                            if (user.UserGroupId == (int)PermissionTypes.Accountant || user.UserGroupId == (int)PermissionTypes.Admin || user.UserGroupId == (int)PermissionTypes.Manager)
                            {
                                //Cập nhật status Đã duyệt
                                item.Status = (int)WalletStatus.DaDuyet;
                                unitOfWork.Repository<Withdraw>().Update(item);
                                //Thông báo đã duyệt yêu cầu rút cho user
                                var notiSettingRutTien = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.DuyetYeuCauRut);
                                sendNotificationService.SendNotification(notiSettingRutTien,
                                    new List<string>() { user.UserName, item.Id.ToString() },
                                    new UserNotification() { });
                            }
                            else
                            {
                                throw new InvalidCastException(string.Format("Bạn không có quyền duyệt yêu cầu này"));
                            }
                            break;
                        default:
                            break;
                    }

                    break;
                case (int)WalletStatus.Huy: //Hủy
                    switch (item.Type)
                    {
                        case (int)WithdrawTypes.RutTien:
                            var loginUser = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
                            var userRequest = await userService.GetByIdAsync(item.UID.Value);

                            //Cập nhật lại ví
                            userRequest.Wallet += item.Amount ?? 0;
                            unitOfWork.Repository<Users>().Update(userRequest);

                            //Thêm vào lịch sử ví tiền
                            await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                            {
                                UID = userRequest.Id,
                                MainOrderId = 0,
                                Amount = item.Amount,
                                Content = "Hủy lệnh rút tiền",
                                MoneyLeft = userRequest.Wallet, //Vì ở trên có += rồi nên lấy thằng này luôn
                                Type = (int)DauCongVaTru.Cong,
                                TradeType = (int)HistoryPayWalletContents.HuyRutTien,
                            });
                            item.Status = (int)WalletStatus.Huy;
                            unitOfWork.Repository<Withdraw>().Update(item);

                            if (loginUser.UserGroupId != (int)PermissionTypes.User)
                            {
                                var notiSettingRutTien = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.HuyYeuCauRut);
                                sendNotificationService.SendNotification(notiSettingRutTien,
                                    new List<string>() { item.Id.ToString(), user.UserName  },
                                    new UserNotification() { UserId = item.UID });
                            }
                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    unitOfWork.Repository<Withdraw>().Update(item);
                    break;
            }

            await unitOfWork.SaveAsync();
            return true;
        }

        public override async Task<bool> CreateAsync(Withdraw item)
        {
            var user = await userService.GetByIdAsync(item.UID ?? 0);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy User");

            var currentUser = LoginContext.Instance.CurrentUser;
            item.Created = DateTime.Now;
            if (item.UID != currentUser.UserId)
            {
                item.UpdatedBy = currentUser.UserName;
                item.Updated = DateTime.Now;
            }
            switch (item.Type)
            {
                case (int)WithdrawTypes.RutTien: //Rút tiền (VNĐ)

                    if (user.Wallet < item.Amount)
                        throw new AppException("Số tiền trong tài khoản không đủ để lập lệnh rút. Vui lòng kiểm tra lại");

                    //Cập nhật lại ví (lúc yêu cầu là trừ luôn)
                    user.Wallet -= item.Amount ?? 0;
                    unitOfWork.Repository<Users>().Update(user);

                    //Thêm vào lịch sử ví tiền
                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                    {
                        UID = user.Id,
                        Amount = item.Amount,
                        Content = item.Note,
                        MoneyLeft = user.Wallet,
                        Type = (int)DauCongVaTru.Tru,
                        TradeType = (int)HistoryPayWalletContents.RutTien
                    });

                    //Thông báo tới admin và manager có yêu cầu rút tiền
                    var notificationSettingRutTien = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.YeuCauRut);
                    sendNotificationService.SendNotification(notificationSettingRutTien,
                        new List<string>() { user.UserName, item.Amount.ToString() },
                        new UserNotification());
                    break;
                default:
                    break;
            }
            await unitOfWork.Repository<Withdraw>().CreateAsync(item);
            await unitOfWork.SaveAsync();

            return true;
        }

        public override async Task<bool> UpdateAsync(Withdraw item)
        {
            return await UpdateStatus(item, item.Status ?? 0);
        }
    }
}
