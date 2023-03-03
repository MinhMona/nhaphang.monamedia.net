
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Entities
{
    public class StaffIncome : DomainEntities.AppDomain
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int? MainOrderId { get; set; } = 0;
        /// <summary>
        /// Mã đơn hàng ký gửi
        /// </summary>
        public int? TransportationOrderId { get; set; } = 0;
        /// <summary>
        /// Mã đơn hàng thanh toán hộ
        /// </summary>
        public int? PayHelpOrderId { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? OrderTotalPrice { get; set; } = 0;

        /// <summary>
        /// Phần trăm
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? PercentReceive { get; set; } = 0;

        /// <summary>
        /// ID người dùng
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = (int)StatusStaffIncome.Unpaid; //Để mặc định trạng thái

        /// <summary>
        /// Hoa hồng (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceReceive { get; set; } = 0;

        public DateTime? OrderCreatedDate { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        [NotMapped]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Quyền hạn
        /// </summary>
        [NotMapped]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Tổng tiền đã thanh toán
        /// </summary>
        [NotMapped]
        public decimal? MaxTotalPriceReceivePayment { get; set; } = 0;

        /// <summary>
        /// Tổng tiền chưa thanh toán
        /// </summary>
        [NotMapped]
        public decimal? MaxTotalPriceReceiveNotPayment { get; set; } = 0;

        /// <summary>
        /// Ngày hoàn thành của đơn mua hộ
        /// </summary>
        [NotMapped]
        public DateTime? MainOrderCompleteDate { get; set; }
    }
}
