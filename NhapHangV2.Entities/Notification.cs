using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class Notification : DomainEntities.AppDomain
    {
        /// <summary>
        /// Mã template thông báo
        /// </summary>
        public int? NotificationTemplateId { get; set; } = 0;

        /// <summary>
        /// Nội dung thông báo
        /// </summary>
        public string NotificationContent { get; set; } = string.Empty;

        /// <summary>
        /// Cờ đã xem
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Gửi đến người dùng
        /// </summary>
        public int? ToUserId { get; set; }

        /// <summary>
        /// Gửi đến nhóm người dùng
        /// </summary>
        public int? UserGroupId { get; set; }

        /// <summary>
        /// Link Url
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int? MainOrderId { get; set; }

        /// <summary>
        /// Là thông báo của nhân viên
        /// </summary>
        public bool OfEmployee { get; set; }

        [NotMapped]
        public string EmailSubject { get; set; }
        [NotMapped]
        public string EmailContent { get; set; }
    }
}
