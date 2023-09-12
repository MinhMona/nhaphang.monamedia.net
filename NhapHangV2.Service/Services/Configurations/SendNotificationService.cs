using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.BackgroundServices;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Utilities;
using OneSignal.RestAPIv3.Client;
using OneSignal.RestAPIv3.Client.Resources;
using OneSignal.RestAPIv3.Client.Resources.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services.Configurations
{
    public class SendNotificationService : ISendNotificationService
    {
        protected IHubContext<DomainHub, IDomainHub> hubContext;
        protected IUserInGroupService userInGroupService;
        protected IUserService userService;
        protected INotificationService notificationService;
        protected IEmailConfigurationService emailConfigurationService;
        protected IConfigurationsService configurationsService;
        protected readonly IServiceScopeFactory serviceScopeFactory;
        protected readonly IBackgroundTaskQueue taskQueue;
        protected readonly IServiceProvider serviceProvider;

        public SendNotificationService(IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory, IBackgroundTaskQueue taskQueue)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.taskQueue = taskQueue;
            this.serviceProvider = serviceProvider;
        }
        public void SendNotification(NotificationSetting notificationSetting, List<string> contentParams, UserNotification toUser)
        {
            taskQueue.QueueBackgroundWorkItem(async token =>
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    userInGroupService = scopedServices.GetRequiredService<IUserInGroupService>();
                    userService = scopedServices.GetRequiredService<IUserService>();
                    notificationService = scopedServices.GetRequiredService<INotificationService>();
                    hubContext = scopedServices.GetRequiredService<IHubContext<DomainHub, IDomainHub>>();
                    emailConfigurationService = scopedServices.GetRequiredService<IEmailConfigurationService>();
                    configurationsService = scopedServices.GetRequiredService<IConfigurationsService>();
                    var notificationList = new List<Notification>();
                    var notificationEmailList = new List<Notification>();
                    if (notificationSetting.IsNotifyAdmin && notificationSetting.IsEmailAdmin)
                    {
                        notificationEmailList.AddRange(await createListNotificationEmail(1, notificationSetting, "Admin", contentParams));
                    }
                    else if (notificationSetting.IsNotifyAdmin)
                    {
                        notificationList.AddRange(await createListNotification(1, notificationSetting, "Admin", contentParams));
                    }

                    if (notificationSetting.IsNotifyWarehoueFrom && notificationSetting.IsEmailWarehoueFrom)
                    {
                        notificationEmailList.AddRange(await createListNotificationEmail(5, notificationSetting, "Kho TQ", contentParams));
                    }
                    else if (notificationSetting.IsNotifyWarehoueFrom)
                    {
                        notificationList.AddRange(await createListNotification(5, notificationSetting, "Kho TQ", contentParams));
                    }

                    if (notificationSetting.IsNotifyWarehoue && notificationSetting.IsEmailWarehoue)
                    {
                        notificationEmailList.AddRange(await createListNotificationEmail(6, notificationSetting, "Kho VN", contentParams));
                    }
                    else if (notificationSetting.IsNotifyWarehoue)
                    {
                        notificationList.AddRange(await createListNotification(6, notificationSetting, "Kho VN", contentParams));
                    }

                    if (notificationSetting.IsNotifyAccountant && notificationSetting.IsEmailAccountant)
                    {
                        notificationEmailList.AddRange(await createListNotificationEmail(8, notificationSetting, "Kế toán", contentParams));
                    }
                    else if (notificationSetting.IsNotifyAccountant)
                    {
                        notificationList.AddRange(await createListNotification(8, notificationSetting, "Kế toán", contentParams));
                    }

                    if (notificationSetting.IsNotifyStorekeepers && notificationSetting.IsEmailStorekeepers)
                    {
                        notificationEmailList.AddRange(await createListNotificationEmail(9, notificationSetting, "Thủ kho", contentParams));
                    }
                    else if (notificationSetting.IsNotifyStorekeepers)
                    {
                        notificationList.AddRange(await createListNotification(9, notificationSetting, "Thủ kho", contentParams));
                    }
                    var toUserNotificationList = createListToUserNotification(toUser, notificationSetting, contentParams, notificationList, notificationEmailList);
                    notificationList = toUserNotificationList[0];
                    notificationEmailList = toUserNotificationList[1];
                    await SendNotis(notificationList, notificationEmailList);
                }
            });
        }

        private async Task<List<Notification>> createListNotification(int roleId, NotificationSetting notiSetting, string prefix, List<string> contentParams)
        {
            List<Notification> notis = new List<Notification>();
            var useInGroupAdmins = await userInGroupService.GetUserInGroupsByUserGroupId(roleId);
            foreach (var admin in useInGroupAdmins)
            {
                var noti = new Notification()
                {
                    Url = string.Format(notiSetting.ManagerUrl, contentParams.ToArray()),
                    NotificationContent = string.Format($"{prefix} - " + notiSetting.ManagerContent, contentParams.ToArray()),
                    ToUserId = admin.UserId,
                    OfEmployee = true
                };
                notis.Add(noti);
            }
            return notis;
        }
        private async Task<List<Notification>> createListNotificationEmail(int roleId, NotificationSetting notiSetting, string prefix, List<string> contentParams)
        {
            List<Notification> notis = new List<Notification>();
            var useInGroupAdmins = await userInGroupService.GetUserInGroupsByUserGroupId(roleId);
            foreach (var admin in useInGroupAdmins)
            {
                var noti = new Notification()
                {
                    Url = notiSetting.ManagerUrl,
                    NotificationContent = string.Format($"{prefix} - " + notiSetting.ManagerContent, contentParams),
                    ToUserId = admin.UserId,
                    OfEmployee = true,
                    EmailSubject = notiSetting.EmailSubject,
                    EmailContent = string.Format(notiSetting.EmailContent, contentParams)
                };
                notis.Add(noti);
            }
            return notis;
        }
        private List<List<Notification>> createListToUserNotification(UserNotification userNotification, NotificationSetting notiSetting, List<string> contentParams, List<Notification> notificationList, List<Notification> notificationEmailList)
        {
            if (userNotification?.UserId != null && notiSetting.IsNotifyUser && notiSetting.IsEmailUser)
            {
                notificationEmailList.Add(new Notification()
                {
                    Url = string.Format(notiSetting.UserUrl, contentParams.ToArray()),
                    NotificationContent = string.Format(notiSetting.UserContent, contentParams.ToArray()),
                    ToUserId = userNotification?.UserId ?? 0,
                    OfEmployee = false,
                    EmailSubject = notiSetting.EmailSubject,
                    EmailContent = string.Format(notiSetting.EmailContent, contentParams.ToArray())
                });
            }
            else if (userNotification?.UserId != null && notiSetting.IsNotifyUser)
            {
                notificationList.Add(new Notification()
                {
                    Url = string.Format(notiSetting.UserUrl, contentParams.ToArray()),
                    NotificationContent = string.Format(notiSetting.UserContent, contentParams.ToArray()),
                    ToUserId = userNotification?.UserId ?? 0,
                    OfEmployee = false
                });
            }

            if (userNotification?.SaleId != null && notiSetting.IsNotifySaler && notiSetting.IsEmailSaler)
            {
                notificationEmailList.Add(new Notification()
                {
                    Url = string.Format(notiSetting.ManagerUrl, contentParams.ToArray()),
                    NotificationContent = string.Format($"Kinh doanh - " + notiSetting.ManagerContent, contentParams.ToArray()),
                    ToUserId = userNotification?.SaleId ?? 0,
                    OfEmployee = true,
                    EmailSubject = notiSetting.EmailSubject,
                    EmailContent = string.Format(notiSetting.EmailContent, contentParams.ToArray())
                });
            }
            else if (userNotification?.SaleId != null && notiSetting.IsNotifySaler)
            {
                notificationList.Add(new Notification()
                {
                    Url = string.Format(notiSetting.ManagerUrl, contentParams.ToArray()),
                    NotificationContent = string.Format($"Kinh doanh - " + notiSetting.ManagerContent, contentParams.ToArray()),
                    ToUserId = userNotification?.SaleId ?? 0,
                    OfEmployee = true,
                });
            }

            if (userNotification?.OrdererId != null && notiSetting.IsNotifyOrderer && notiSetting.IsEmailOrderer)
            {
                notificationEmailList.Add(new Notification()
                {
                    Url = string.Format(notiSetting.ManagerUrl, contentParams.ToArray()),
                    NotificationContent = string.Format("Đặt hàng - " + notiSetting.ManagerContent, contentParams.ToArray()),
                    ToUserId = userNotification?.OrdererId ?? 0,
                    OfEmployee = true,
                    EmailSubject = notiSetting.EmailSubject,
                    EmailContent = string.Format(notiSetting.EmailContent, contentParams.ToArray())
                });
            }
            else if (userNotification?.OrdererId != null && notiSetting.IsNotifyOrderer)
            {
                notificationList.Add(new Notification()
                {
                    Url = string.Format(notiSetting.ManagerUrl, contentParams.ToArray()),
                    NotificationContent = string.Format("Đặt hàng - " + notiSetting.ManagerContent, contentParams.ToArray()),
                    ToUserId = userNotification?.OrdererId ?? 0,
                    OfEmployee = true
                });
            }
            return new List<List<Notification>>()
            {
                notificationList,
                notificationEmailList
            };
        }
        private async Task SendNotis(List<Notification> notis, List<Notification> notisEmail)
        {
            try
            {
                var userReceiveEmail = new List<Users>();
                var confi = await configurationsService.GetSingleAsync();
                Guid appId = Guid.Parse(confi.OneSignalAppID);
                string restAPIKey = confi.RestAPIKey;
                if (notis.Count > 0)
                {
                    var playerIds = new List<string>();
                    foreach (var noti in notis)
                    {
                        await notificationService.CreateAsync(noti);
                        await hubContext.Clients.Groups(new List<string>
                        {
                            string.Format("UserId_{0}", noti.ToUserId)
                        }).SendNotification(noti);
                        var user = await userService.GetByIdAsync(noti.ToUserId ?? 0);
                        if (user != null)
                        {
                            playerIds.Add(user.OneSignalPlayerID);
                            if (user.OneSignalPlayerID != null)
                            {
                                await OneSignalPushNotification(playerIds, $"{confi.WebsiteName}", noti, appId, restAPIKey);
                            }
                        }
                    }
                }
                if (notisEmail.Count > 0)
                {
                    var playerIds = new List<string>();
                    foreach (var noti in notis)
                    {
                        await notificationService.CreateAsync(noti);
                        await hubContext.Clients.Groups(new List<string>
                        {
                            string.Format("UserId_{0}", noti.ToUserId)
                        }).SendNotification(noti);
                        var user = await userService.GetByIdAsync(noti.ToUserId ?? 0);
                        if (user != null)
                        {
                            playerIds.Add(user.OneSignalPlayerID);
                            if (user.OneSignalPlayerID != null)
                            {
                                await OneSignalPushNotification(playerIds, $"{confi.WebsiteName}", noti, appId, restAPIKey);
                            }
                            await emailConfigurationService.Send(noti.EmailSubject, new string[] { user.Email }, null, null, new EmailContent()
                            {
                                IsHtml = true,
                                Content = noti.EmailContent
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        private async Task OneSignalPushNotification(List<string> playerIds, string heading, Notification notification, Guid appId, string restKey)
        {
            var confi = await configurationsService.GetSingleAsync();
            Thread oneSignalThread = new Thread(async () =>
            {
                OneSignalClient client = new OneSignalClient(restKey);

                var opt = new NotificationCreateOptions()
                {
                    AppId = appId,
                    IncludePlayerIds = playerIds,
                    SendAfter = DateTime.Now.AddSeconds(5)
                };
                opt.Headings.Add(LanguageCodes.English, heading);
                opt.Contents.Add(LanguageCodes.English, notification.NotificationContent);
                opt.Url = $"{confi.WebsiteUrl}/{notification.Url}";
                try
                {
                    await client.Notifications.CreateAsync(opt);
                }
                catch (AppException)
                {
                    throw new AppException("Gửi thông báo One Signal thất bại");
                }
            });
            oneSignalThread.Start();
        }
    }
}
