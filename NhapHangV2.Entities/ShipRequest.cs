using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class ShipRequest : DomainEntities.AppDomain
    {
        public int? UID { get; set; }
        public string MainOrderId { get; set; } = string.Empty;
        public string TransportrationOrderId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public int? Status { get; set; }

        [NotMapped]
        public List<int> MainOrderIds { get; set; } = new List<int>();

        [NotMapped]
        public List<int> TransportationIds { get; set; } = new List<int>();

        [NotMapped]
        public string UserName { get; set; }
    }
}
