using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class PayHelpRequest : AppDomainRequest
    {
        public decimal? TotalPriceVNDGiaGoc { get; set; }
        public decimal? CurrencyConfig { get; set; }
        public decimal? Currency { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Note { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Chưa hoàn thiện
        /// </summary>
        public bool? IsComplete { get; set; }

        /// <summary>
        /// Tổng tiền
        /// </summary>
        public decimal? TotalPrice { get; set; }

        /// <summary>
        /// Tổng tiền (VNĐ)
        /// </summary>
        public decimal? TotalPriceVND { get; set; }

        /// <summary>
        /// Hóa đơn thanh toán hộ
        /// </summary>
        public List<PayHelpDetailRequest>? PayHelpDetails { get; set; }

        /// <summary>
        /// ID Saler
        /// </summary>
        public int? SalerID { get; set; }
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
        public decimal? FeeService { get; set; }

    }
}
