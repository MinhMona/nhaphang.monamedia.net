
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class HistoryOrderChange : DomainEntities.AppDomain
    {
        public int? MainOrderId { get; set; } = 0;

        public int? TransportationOrderId { get; set; } = 0;

        public int? UID { get; set; } = 0;

        /// <summary>
        /// Nội dung
        /// </summary>
        public string HistoryContent { get; set; } = string.Empty;

        /// <summary>
        /// Loại
        /// </summary>
        public int? Type { get; set; } = 0;

        /// <summary>
        /// Quyền hạn
        /// </summary>
        [NotMapped]
        public string UserGroupName { get; set; } = string.Empty;
    }
}
