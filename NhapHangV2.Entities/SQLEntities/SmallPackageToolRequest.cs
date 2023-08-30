using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class SmallPackageToolRequest : DomainEntities.AppDomain
    {
        public List<string> OrdertransactionCodes { get; set; }
        public int? MainOrderId { get; set; }
        public string MainOrderCode { get; set; }
    }
}
