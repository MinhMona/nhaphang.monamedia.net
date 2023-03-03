
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class OutStockSessionPackage : DomainEntities.AppDomain
    {
        public int? OutStockSessionId { get; set; } = 0;

        public int? SmallPackageId { get; set; } = 0;

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string OrderTransactionCode { get; set; }

        /// <summary>
        /// Mã đơn mua hộ
        /// </summary>
        public int MainOrderID { get; set; }

        /// <summary>
        /// Mã đơn ký gửi
        /// </summary>
        public int TransportationID { get; set; }


        /// <summary>
        /// Ngày xuất kho
        /// </summary>
        public DateTime? DateOutStock { get; set; }

        /// <summary>
        /// Tổng tiền lưu kho
        /// </summary>
        [NotMapped]
        public decimal? WarehouseFee { get; set; } = 0;

        /// <summary>
        /// Tiền cần thanh toán
        /// </summary>
        [NotMapped]
        public decimal? TotalLeftPay { get; set; } = 0;

        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        [NotMapped]
        public bool? IsPayment { get; set; } = false;

        /// <summary>
        /// Tiền hàng
        /// </summary>
        [NotMapped]
        public decimal? TotalPriceVND { get; set; } = 0;

        [NotMapped]
        public SmallPackage SmallPackage { get; set; } = new SmallPackage();

        /// <summary>
        /// Tổng tiền còn lại của đơn mua hộ
        /// </summary>
        /// 
        [NotMapped]
        public decimal? OrderRemaining { get; set; }
    }
}
