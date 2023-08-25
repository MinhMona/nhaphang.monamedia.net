
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class PayHelpDetail : DomainEntities.AppDomain
    {
        public int? PayHelpId { get; set; } = 0;

        /// <summary>
        /// Giá tiền
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Desc1 { get; set; } = 0;

        /// <summary>
        /// Nội dung
        /// </summary>
        public string Desc2 { get; set; } = string.Empty;

        /// <summary>
        /// Ảnh khách up
        /// </summary>
        public string UserImage { get; set; } = string.Empty;
        /// <summary>
        /// Ảnh quản trị up
        /// </summary>
        public string AdminImage { get; set; } = string.Empty;
    }
}
