
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class OutStockSession : DomainEntities.AppDomain
    {
        /// <summary>
        /// UID người nhập
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// Tổng tiền phải trả (Tiền cần thanh toán)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPay { get; set; } = 0;

        /// <summary>
        /// Là đơn mua hộ
        /// </summary>
        public int? IsOutStockOrder { get; set; } = 0;

        /// <summary>
        /// Là đơn ký gửi
        /// </summary>
        public int? IsOutStockTrans { get; set; } = 0;

        /// <summary>
        /// Họ và tên người nhận
        /// </summary>
        [NotMapped]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại người nhận
        /// </summary>
        [NotMapped]
        public string UserPhone { get; set; } = string.Empty;

        /// <summary>
        /// UserName người nhận
        /// </summary>
        [NotMapped]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái (1: Chưa xác nhận, 2: Đã xác nhận)
        /// </summary>
        public int? Status { get; set; } = 1; //Mặc định "Chưa xác nhận"

        
        /// <summary>
        /// Tổng cân nặng
        /// </summary>
        [NotMapped]
        public decimal? TotalWeight { get; set; } = 0;

        /// <summary>
        /// Tổng tiền lưu kho tất cả đơn
        /// </summary>
        [NotMapped]
        public decimal? TotalWarehouseFee { get; set; } = 0;

        /// <summary>
        /// Trạng thái thanh toán
        /// 1. Thanh toán bằng ví điện tử
        /// 2. Thanh toán bằng tiền mặt
        /// </summary>
        public int? Type { get; set; } = 0;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Chi tiết phiên xuất kho
        /// </summary>
        [NotMapped]
        public List<OutStockSessionPackage> OutStockSessionPackages { get; set; } = new List<OutStockSessionPackage>();

        [NotMapped]
        public List<int> SmallPackageIds { get; set; } = new List<int>();

        /// <summary>
        /// Cân nặng quy đổi
        /// </summary>
        /// 
        [NotMapped]
        public decimal? ExchangeWeight { get; set; } = decimal.Zero;

        /// <summary>
        /// Cân nặng trả tiền
        /// </summary>
        /// 
        [NotMapped]
        public decimal? PayableWeight { get; set; } = decimal.Zero;


        /// <summary>
        /// Số dư tài khoản của khách
        /// </summary>
        /// 
        [NotMapped]
        public decimal? UserWallet { get; set; }

    }
}
