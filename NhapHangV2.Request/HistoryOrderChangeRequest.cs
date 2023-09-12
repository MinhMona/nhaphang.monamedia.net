using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class HistoryOrderChangeRequest : AppDomainRequest
    {
        public int? TransportationOrderId { get; set; }
        public int? MainOrderId { get; set; }

        public int? UID { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        [StringLength(1000)]
        public string? HistoryContent { get; set; }

        /// <summary>
        /// Loại
        /// </summary>
        public int? Type { get; set; }
    }
}
