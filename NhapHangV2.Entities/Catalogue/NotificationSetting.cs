using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Catalogue
{
    public class NotificationSetting : AppDomainCatalogue
    {
        /// <summary>
        /// Thông báo cho admin
        /// </summary>
        public bool IsNotifyAdmin { get; set; } = false;

        /// <summary>
        /// Thông báo cho người dùng
        /// </summary>
        public bool IsNotifyUser { get; set; } = false;

        /// <summary>
        /// Gửi email cho admin
        /// </summary>
        public bool IsEmailAdmin { get; set; } = false;

        /// <summary>
        /// Gửi email cho người dùng
        /// </summary>
        public bool IsEmailUser { get; set; } = false;

        /// <summary>
        /// Thông báo cho kho TQ
        /// </summary>
        public bool IsNotifyWarehoueFrom { get; set; } = false;

        /// <summary>
        /// Thông báo cho kho VN
        /// </summary>
        public bool IsNotifyWarehoue { get; set; } = false;

        /// <summary>
        /// Thông báo cho đặt hàng
        /// </summary>
        public bool IsNotifyOrderer { get; set; } = false;

        /// <summary>
        /// Thông báo cho saler
        /// </summary>
        public bool IsNotifySaler { get; set; } = false;

        /// <summary>
        /// Thông báo cho thủ kho
        /// </summary>
        public bool IsNotifyStorekeepers { get; set; } = false;

        /// <summary>
        /// Thông báo cho thủ kho
        /// </summary>
        public bool IsNotifyAccountant { get; set; } = false;
        /// <summary>
        /// Thông báo cho kho TQ
        /// </summary>
        public bool IsEmailWarehoueFrom { get; set; } = false;

        /// <summary>
        /// Thông báo cho kho VN
        /// </summary>
        public bool IsEmailWarehoue { get; set; } = false;

        /// <summary>
        /// Thông báo cho đặt hàng
        /// </summary>
        public bool IsEmailOrderer { get; set; } = false;

        /// <summary>
        /// Thông báo cho saler
        /// </summary>
        public bool IsEmailSaler { get; set; } = false;

        /// <summary>
        /// Thông báo cho thủ kho
        /// </summary>
        public bool IsEmailStorekeepers { get; set; } = false;

        /// <summary>
        /// Thông báo cho kế toán
        /// </summary>
        public bool IsEmailAccountant { get; set; } = false;

        public int? Type { get; set; }
        public string ManagerContent { get; set; }
        public string UserContent { get; set; }
        public string ManagerUrl { get; set; }
        public string UserUrl { get; set; }
        public string ManagerEmailSubject { get; set; }
        public string ManagerEmailContent { get; set; }
        public string EmailSubject { get; set; }
        public string EmailContent { get; set; }
    }
}
