using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class ShipRequestRequest : AppDomainRequest
    {
        public int? UID { get; set; }
        public string? MainOrderId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public int? Status { get; set; }

        public List<int>? MainOrderIds { get; set; }
        public List<int>? TransportationIds { get; set; }
    }
}
