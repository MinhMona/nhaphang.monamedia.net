using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class PayHelpDetailRequest : AppDomainRequest
    {
        public int? PayHelpId { get; set; }

        /// <summary>
        /// Giá tiền
        /// </summary>
        public decimal? Desc1 { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string? Desc2 { get; set; }
        /// <summary>
        /// Ảnh khách up
        /// </summary>
        public string? UserImage { get; set; }
        /// <summary>
        /// Ảnh quản trị up
        /// </summary>
        public string? AdminImage { get; set; }
    }
}
