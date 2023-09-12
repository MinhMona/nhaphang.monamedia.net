using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.Configuration;
using System.Collections.Generic;

namespace NhapHangV2.Interface.Services.Configuration
{
    public interface ISendNotificationService
    {
        void SendNotification(NotificationSetting notificationSetting, List<string> contentParams, UserNotification toUser);
    }
}
