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
    public class AdminSendUserWalletService : DomainService<AdminSendUserWallet, AdminSendUserWalletSearch>, IAdminSendUserWalletService
    {
        protected readonly IUserService userService;
        protected readonly IUserInGroupService userInGroupService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly ISendNotificationService sendNotificationService;

        public AdminSendUserWalletService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
            userService = serviceProvider.GetRequiredService<IUserService>();

        }

        public override async Task<AdminSendUserWallet> GetByIdAsync(int id)
        {
            var adminSendUserWallet = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (adminSendUserWallet == null)
                return null;
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Id == adminSendUserWallet.UID).FirstOrDefaultAsync();
            if (user != null)
                adminSendUserWallet.UserName = user.UserName;
            return adminSendUserWallet;
        }

        protected override string GetStoreProcName()
        {
            return "AdminSendUserWallet_GetPagingData";
        }

        public async Task<bool> UpdateStatus(AdminSendUserWallet item, int status)
        {
            var currentUser = LoginContext.Instance.CurrentUser;
            var user = new Users();
            if (item.UID == 0 || item.UID == null)
            {
                user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId); //User
                if (user.IsAdmin || (user.UserGroupId != (int)PermissionTypes.Accountant || user.UserGroupId != (int)PermissionTypes.Manager))
                { }
                else throw new InvalidCastException(string.Format("Bạn không có quyền duyệt yêu cầu này"));
            }
            else
                user = await userService.GetByIdAsync(item.UID ?? 0); //Admin nạp / rút dùm

            if (user == null) throw new KeyNotFoundException("Không tìm thấy User");

            item.UID = user.Id;

            switch (status)
            {
                case (int)WalletStatus.DaDuyet: //Duyệt

                    //Cập nhật ví tiền
                    user.Wallet += item.Amount;
                    await userService.UpdateAsync(user);

                    //Lịch sử ví tiền VNĐ
                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                    {
                        UID = user.Id,
                        MainOrderId = 0,
                        MoneyLeft = user.Wallet,
                        Amount = item.Amount,
                        Type = (int)DauCongVaTru.Cong,
                        TradeType = (int)HistoryPayWalletContents.NapTien,
                        Content = string.IsNullOrEmpty(item.TradeContent) ? string.Format("{0} đã được nạp tiền vào tài khoản.", user.UserName) : item.TradeContent,
                    });

                    item.Status = (int)WalletStatus.DaDuyet;

                    //Thông báo nạp VND
                    var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.DuyetYeuCauNap);
                    sendNotificationService.SendNotification(notificationSetting,
                        new List<string> { item.Id.ToString(), currentUser.UserName },
                        new UserNotification() { UserId = user.Id });
                    break;
                case (int)WalletStatus.Huy: //Hủy
                    item.Status = (int)WalletStatus.Huy;
                    var notificationSettingHuy = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.HuyYeuCauNap);
                    sendNotificationService.SendNotification(notificationSettingHuy,
                        new List<string> { item.Id.ToString(), currentUser.UserName },
                        new UserNotification() { UserId = user.Id });
                    break;
                default:
                    break;
            }

            unitOfWork.Repository<AdminSendUserWallet>().Update(item);
            await unitOfWork.SaveAsync();
            return true;
        }

        public override async Task<bool> CreateAsync(AdminSendUserWallet item)
        {
            var user = await userService.GetByIdAsync(item.UID ?? 0);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy tài khoản");
            var currentUser = LoginContext.Instance.CurrentUser;
            bool isSendAdmin = false;
            if (item.Status == (int)WalletStatus.DaDuyet)
            {
                item.UpdatedBy = currentUser.UserName;
                item.Updated = DateTime.Now;
                user.Wallet += item.Amount ?? 0;
                unitOfWork.Repository<Users>().Update(user);
                await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                {
                    UID = user.Id,
                    Amount = item.Amount,
                    Content = item.TradeContent,
                    MoneyLeft = user.Wallet,
                    Type = (int)DauCongVaTru.Cong,
                    TradeType = (int)HistoryPayWalletContents.NapTien
                });
                isSendAdmin = true;
            }
            if (isSendAdmin)
            {
                var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.AdminChuyenTien);
                sendNotificationService.SendNotification(notificationSetting,
                        new List<string> { currentUser.UserName, user.UserName, item.Amount.ToString() },
                        new UserNotification() { UserId = user.Id });
            }
            else
            {
                var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.YeuCauNap);
                sendNotificationService.SendNotification(notificationSetting,
                        new List<string>(),
                        new UserNotification() { UserId = user.Id });
            }
            await unitOfWork.Repository<AdminSendUserWallet>().CreateAsync(item);
            await unitOfWork.SaveAsync();
            //Thông báo
            //Thông báo tới admin và manager có yêu cầu nạp tiền VND

            return true;
        }

        public override async Task<bool> UpdateAsync(AdminSendUserWallet item)
        {
            return await UpdateStatus(item, item.Status ?? 0);
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
                    Note = item.TradeContent,
                    Amount = item.Amount ?? 0
                };
                return billInfor;
            }
            throw new EntryPointNotFoundException("Không tìm thấy yêu cầu");
        }
    }
}
