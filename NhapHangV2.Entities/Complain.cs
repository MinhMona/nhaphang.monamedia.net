
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Entities
{
    public class Complain : DomainEntities.AppDomain
    {
        /// <summary>
        /// UID
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// Mã shop
        /// </summary>
        public int? MainOrderId { get; set; } = 0;

        /// <summary>
        /// Tiền bồi thường
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Amount { get; set; } = 0;

        /// <summary>
        /// Hình ảnh
        /// </summary>
        public string IMG { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung
        /// </summary>
        public string ComplainText { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = (int)StatusComplain.MoiTao;

        /// <summary>
        /// UserName
        /// </summary>
        [NotMapped]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Tỉ giá theo Shop
        /// </summary>
        [NotMapped]
        public decimal? CurrentCNYVN { get; set; } = 0;

        /// <summary>
        /// Id đơn ký gửi
        /// </summary>
        public int? TransportationOrderId { get; set; } = 0;
    }
}
