using NhapHangV2.Models.Auth;
using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Text;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class OutStockSessionPackageModel : AppDomainModel
    {
        public int? OutStockSessionId { get; set; }

        public int? SmallPackageId { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string OrderTransactionCode { get; set; }

        /// <summary>
        /// Mã đơn mua hộ
        /// </summary>
        public string MainOrderID { get; set; }

        /// <summary>
        /// Tổng tiền còn lại của đơn mua hộ
        /// </summary>
        public decimal? OrderRemaining { get; set; }

        /// <summary>
        /// Mã đơn ký gửi
        /// </summary>
        public string TransportationID { get; set; }

        /// <summary>
        /// Ngày xuất kho
        /// </summary>
        public DateTime? DateOutStock { get; set; }

        /// <summary>
        /// Tổng tiền lưu kho
        /// </summary>
        public decimal? WarehouseFee { get; set; }

        /// <summary>
        /// Tiền cần thanh toán
        /// </summary>
        public decimal? TotalLeftPay { get; set; }

        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        public bool? IsPayment { get; set; }

        /// <summary>
        /// Tổng tiền hàng
        /// </summary>
        public decimal? TotalPriceVND { get; set; }

        public SmallPackageModel SmallPackage { get; set; }
    }
}
