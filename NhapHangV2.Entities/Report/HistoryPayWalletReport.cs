using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Report
{
    public class HistoryPayWalletReport : AppDomainReport
    {
        /// <summary>
        /// 1: Trừ, 2: Cộng
        /// </summary>
        public int? Type { get; set; } = 0;
        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal Amount { get; set; } = 0;

        /// <summary>
        /// Loại giao dịch
        /// </summary>
        public int TradeType { get; set; } = 0;

        /// <summary>
        /// Số dư
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal MoneyLeft { get; set; } = 0;

        /// <summary>
        /// Tổng số tiền giao dịch
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalAmount { get; set; } = 0;

        /// <summary>
        /// Tổng tiền đặt cọc
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalDeposit { get; set; } = 0;

        /// <summary>
        /// Tổng tiền nhận lại đặt cọc
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalReciveDeposit { get; set; } = 0;

        /// <summary>
        /// Tổng tiền thanh toán hóa đơn
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalPaymentBill { get; set; } = 0;

        /// <summary>
        /// Tổng tiền admin nạp tiền
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalAdminSend { get; set; } = 0;

        /// <summary>
        /// Tổng rút tiền
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalWithDraw { get; set; } = 0;

        /// <summary>
        /// Tổng hủy rút tiền
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalCancelWithDraw { get; set; } = 0;

        /// <summary>
        /// Tổng nhận tiền khiếu nại
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalComplain { get; set; } = 0;

        /// <summary>
        /// Tổng tiền thanh toán vận chuyển hộ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalPaymentTransport { get; set; } = 0;

        /// <summary>
        /// Tổng tiền thanh toán hộ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalPaymentHo { get; set; } = 0;

        /// <summary>
        /// Tổng tiền thanh toán lưu kho
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalPaymentSaveWare { get; set; } = 0;

        /// <summary>
        /// Tổng tiền nhận lại vận chuyển hộ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal TotalRecivePaymentTransport { get; set; } = 0;
    }
}
