using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Search
{
    public class TransportationOrderSearch : BaseSearch
    {
        public bool? IsShipRequest { get; set; }
        /// <summary>
        /// ID User
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// UserGroupID
        /// </summary>
        public int? RoleID { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Tìm kiếm theo
        /// </summary>
        public int? TypeSearch { get; set; }

        /// <summary>
        /// Từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// ID User
        /// </summary>
        public int? SalerId { get; set; }

    }
}
