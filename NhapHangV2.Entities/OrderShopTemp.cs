
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class OrderShopTemp : DomainEntities.AppDomain
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeBuyProPercent { get; set; } = 0;

        /// <summary>
        /// ID user
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// Id của shop trên website
        /// </summary>
        [StringLength(500)]
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// Tên shop
        /// </summary>
        [StringLength(500)]
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Website
        /// </summary>
        [StringLength(10)]
        public string Site { get; set; } = string.Empty;

        /// <summary>
        /// Giao tận nhà
        /// </summary>
        public bool? IsFastDelivery { get; set; } = false;

        /// <summary>
        /// Phí giao tận nhà
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? IsFastDeliveryPrice { get; set; } = 0;

        /// <summary>
        /// Kiểm hàng
        /// </summary>
        public bool? IsCheckProduct { get; set; } = false;

        /// <summary>
        /// Phí kiểm tra hàng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? IsCheckProductPrice { get; set; } = 0;

        /// <summary>
        /// Đóng gỗ
        /// </summary>
        public bool? IsPacked { get; set; } = false;

        /// <summary>
        /// Phí đóng gỗ
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IsPackedPrice { get; set; } = 0;

        /// <summary>
        /// Giao nhanh
        /// </summary>
        public bool? IsFast { get; set; } = false;

        /// <summary>
        /// Phí giao nhanh
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? IsFastPrice { get; set; } = 0;

        /// <summary>
        /// Bảo hiểm
        /// </summary>
        public bool? IsInsurance { get; set; } = false;

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? InsuranceMoney { get; set; } = 0;

        /// <summary>
        /// Tổng giá của shop (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceVND { get; set; } = 0;

        /// <summary>
        /// Tổng giá của shop (tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PriceCNY { get; set; } = 0;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Phí mua hàng (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeBuyPro { get; set; } = 0;

        /// <summary>
        /// Họ tên người đặt hàng
        /// </summary>
        [NotMapped]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại người đặt hàng
        /// </summary>
        [NotMapped]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Email người đặt hàng
        /// </summary>
        [NotMapped]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ người đặt hàng
        /// </summary>
        [NotMapped]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Tổng tiền (bao gồm phí)
        /// </summary>
        [NotMapped]
        public decimal? TotalPriceVND
        {
            get
            {
                decimal? toTalPriceVND = 0;
                if (PriceVND != null) toTalPriceVND = PriceVND;
                if (FeeBuyPro != null) toTalPriceVND += FeeBuyPro;
                if (IsFastDeliveryPrice != null) toTalPriceVND += IsFastDeliveryPrice;
                if (IsCheckProductPrice != null) toTalPriceVND += IsCheckProductPrice;
                if (IsPackedPrice != null) toTalPriceVND += IsPackedPrice;
                if (IsFastDeliveryPrice != null) toTalPriceVND += IsFastDeliveryPrice;
                if (InsuranceMoney != null) toTalPriceVND += InsuranceMoney;

                return toTalPriceVND;
            }
        }

        [NotMapped]
        public List<OrderTemp> OrderTemps { get; set; } = new List<OrderTemp>();

        [NotMapped]
        public string OrderTempsJson { get; set; }

    }
}
