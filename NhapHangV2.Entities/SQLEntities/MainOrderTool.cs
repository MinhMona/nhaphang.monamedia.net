using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.SQLEntities
{
    public class MainOrderTool : DomainEntities.AppDomain
    {
        public string Username { get; set; }
        public string MainOrderCode { get; set; }
        public string Site { get; set; }
        public int MainOrderId { get; set; }
        public bool IsHasOrderTransactionCode { get; set; }

    }

    public class MainOrderCodeTool
    {
        public int Id { get; set; }
        public string MainOrderCode { get; set; }
    }

    public class MainOrderToolSearch
    {
        public int? UID { get; set; }
        [DefaultValue(1)]
        public int PageIndex { get; set; }
        [DefaultValue(20)]
        public int PageSize { get; set; }
        [DefaultValue("Id desc")]
        public string OrderBy { set; get; }
        public string Site { get; set; }
        [DefaultValue(2)]
        public int HasMVD { get; set; }
        public string Username { get; set; }
        public int? MainOrderId { get; set; }
        public string MainOrderCode { get; set; }
    }
}
