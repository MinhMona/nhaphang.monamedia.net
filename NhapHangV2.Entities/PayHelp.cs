using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class PayHelp : DomainEntities.AppDomain
    {
        /// <summary>
        /// Id User
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// UserName
        /// </summary>
        [NotMapped]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Tổng tiền (Tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")] 
        public decimal? TotalPrice { get; set; } = 0;

        /// <summary>
        /// Tổng tiền (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? TotalPriceVND { get; set; } = 0;

        /// <summary>
        /// Tỉ giá (Tỉ giá tính tiền)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")] 
        public decimal? Currency { get; set; } = 0;

        /// <summary>
        /// Đã trả
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? Deposit { get; set; } = 0;

        /// <summary>
        /// Tỉ giá hệ thống
        /// </summary>
        [Column(TypeName = "decimal(18,2)")] 
        public decimal? CurrencyConfig { get; set; } = 0;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = 0;

        /// <summary>
        /// Hoàn thiện
        /// </summary>
        public bool? IsComplete { get; set; } = false;

        [Column(TypeName = "decimal(18,0)")] 
        public decimal? TotalPriceVNDGiaGoc { get; set; } = 0;

        /// <summary>
        /// Hóa đơn thanh toán hộ
        /// </summary>
        [NotMapped]
        public List<PayHelpDetail> PayHelpDetails { get; set; } = new List<PayHelpDetail>();

        /// <summary>
        /// Lịch sử thay đổi
        /// </summary>
        [NotMapped]
        public List<HistoryServices> HistoryServices { get; set; } = new List<HistoryServices>();

        /// <summary>
        /// Id Saler
        /// </summary>
        public int? SalerID { get; set; }

        /// <summary>
        /// SalerName
        /// </summary>
        [NotMapped]
        public string SalerName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày xác nhận
        /// </summary>
        public DateTime? ConfirmDate { get; set; }
        /// <summary>
        /// Ngày thanh toán
        /// </summary>
        public DateTime? PaidDate { get; set; }
        /// <summary>
        /// Ngày hoàn thành
        /// </summary>
        public DateTime? CompleteDate { get; set; }
        /// <summary>
        /// Ngày hủy
        /// </summary>
        public DateTime? CancelDate { get; set; }

        /// <summary>
        /// Phí dịch vụ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeService { get; set; }
    }
}
