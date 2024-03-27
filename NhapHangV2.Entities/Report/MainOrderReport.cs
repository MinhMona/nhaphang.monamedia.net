using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Report
{
    public class MainOrderReport : AppDomainReport
    {
        /// <summary>
        /// (Tổng phụ phí)
        /// </summary>
        public decimal? Surcharge { get; set; } = 0;

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Tên shop
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// NV kinh doanh
        /// </summary>
        public string SalerUserName { get; set; } = string.Empty;

        /// <summary>
        /// Phí ship TQ
        /// </summary>
        [Column(TypeName = "decimal(18,2)")] 
        public decimal? FeeShipCN { get; set; } = 0;

        /// <summary>
        /// Phí ship TQ thật
        /// </summary>
        [Column(TypeName = "decimal(18,2)")] 
        public decimal? FeeShipCNReal { get; set; } = 0;

        /// <summary>
        /// Phí ship TQ - VN
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? FeeWeight { get; set; } = 0;

        /// <summary>
        /// Phí mua hàng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? FeeBuyPro { get; set; } = 0;

        /// <summary>
        /// Phí cân nặng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? TQVNWeight { get; set; } = 0;

        /// <summary>
        /// Phí giao hàng tận nhà
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? IsFastDeliveryPrice { get; set; } = 0;

        /// <summary>
        /// Phí kiểm đếm
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? IsCheckProductPrice { get; set; } = 0;

        /// <summary>
        /// Phí đóng gỗ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? IsPackedPrice { get; set; } = 0;

        /// <summary>
        /// Tổng tiền (VNĐ) - Tổng tiền đã mua
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? TotalPriceVND { get; set; } = 0;

        /// <summary>
        /// Tổng tiền thật
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceReal { get; set; } = 0;

        /// <summary>
        /// Tổng tiền hàng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceVND { get; set; } = 0;

        /// <summary>
        /// Đặt cọc
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? Deposit { get; set; } = 0;

        /// <summary>
        /// Còn lại
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MustPay { get; set; } = 0;

        /// <summary>
        /// Tiền lời
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? Profit { get; set; } = 0;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = 0;

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? InsuranceMoney { get; set; } = 0;

        /// <summary>
        /// Phí lưu kho
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? FeeInWareHouse { get; set; } = 0;

        /// <summary>
        /// Tổng tiền đơn hàng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxTotalPriceVND { get; set; } = 0;

        /// <summary>
        /// Tổng tiền cần thanh toán
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxMustPay { get; set; } = 0;

        /// <summary>
        /// Tổng tiền thật
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxTotalPriceReal { get; set; } = 0;

        /// <summary>
        /// Tổng tiền lời
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxProfit { get; set; } = 0;

        /// <summary>
        /// Tổng tiền hàng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxPriceVND { get; set; } = 0;

        /// <summary>
        /// Tổng tiền ship TQ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxFeeShipCN { get; set; } = 0;

        /// <summary>
        /// Tổng tiền ship TQ thật
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxFeeShipCNReal { get; set; } = 0;
         
        /// <summary>
        /// Tổng tiền TQ - VN
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxFeeWeight { get; set; } = 0;

        /// <summary>
        /// Tổng tiền mua hàng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxFeeBuyPro { get; set; } = 0;

        /// <summary>
        /// Tổng tiền kiểm đếm
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxIsCheckProductPrice { get; set; } = 0;

        /// <summary>
        /// Tổng tiền đóng gỗ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxIsPackedPrice { get; set; } = 0;

        /// <summary>
        /// Tổng tiền bảo hiểm
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxInsuranceMoney { get; set; } = 0;

        /// <summary>
        /// Tổng tiền lưu kho
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? MaxFeeInWareHouse { get; set; } = 0;
    }
}
