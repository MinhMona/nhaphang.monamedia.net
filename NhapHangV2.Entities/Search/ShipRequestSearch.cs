using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Search
{
    public class ShipRequestSearch : BaseSearch
    {
        public string Phone { get; set; }
        public int? UID { get; set; }
        public int? Status { get; set; }
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public int? Type { get; set; }

    }
}
