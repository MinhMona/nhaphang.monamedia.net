using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class MainOrderCodeToolRequest
    {
        public int MainOrderId { get; set; }
        public int UID { get; set; }
        public List<string> OrderTransactionCodes { get; set; }
    }
}
