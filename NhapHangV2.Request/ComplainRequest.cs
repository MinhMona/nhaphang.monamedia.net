using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Request
{
    public class ComplainRequest : AppDomainRequest
    {
        /// <summary>
        /// UID
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// Mã shop
        /// </summary>
        public int? MainOrderId { get; set; }

        /// <summary>
        /// Tiền bồi thường
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        public string? IMG { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string? ComplainText { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = (int)StatusComplain.ChuaDuyet;

        /// <summary>
        /// Id đơn ký gửi
        /// </summary>
        public int? TransportationOrderId { get; set; }
    }
}
