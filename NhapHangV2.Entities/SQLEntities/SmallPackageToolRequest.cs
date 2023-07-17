using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class SmallPackageToolRequest : DomainEntities.AppDomain
    {
        public string? OrdertransactionCode { get; set; }
        public int? MainOrderId { get; set; }
        public int? MainOrderCodeId { get; set; }
        public int? UID { get; set; }
    }
}
