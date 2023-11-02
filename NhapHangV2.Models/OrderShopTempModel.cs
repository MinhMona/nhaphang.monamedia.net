using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Models
{
    public class OrderShopTempModel : AppDomainModel
    {
        public decimal? FeeBuyProPercent { get; set; }
        /// <summary>
        /// ID user
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// Id của shop
        /// </summary>
        [StringLength(500)]
        public string ShopId { get; set; }

        /// <summary>
        /// Tên shop
        /// </summary>
        [StringLength(500)]
        public string ShopName { get; set; }

        /// <summary>
        /// Website
        /// </summary>
        [StringLength(10)]
        public string Site { get; set; }

        /// <summary>
        /// Giao tận nhà
        /// </summary>
        public bool? IsFastDelivery { get; set; }

        /// <summary>
        /// Phí giao tận nhà
        /// </summary>
        public decimal? IsFastDeliveryPrice { get; set; }

        /// <summary>
        /// Kiểm hàng
        /// </summary>
        public bool? IsCheckProduct { get; set; }

        /// <summary>
        /// Phí kiểm tra hàng
        /// </summary>
        public decimal? IsCheckProductPrice { get; set; }

        /// <summary>
        /// Đóng gỗ
        /// </summary>
        public bool? IsPacked { get; set; }

        /// <summary>
        /// Phí đóng gỗ
        /// </summary>
        public decimal? IsPackedPrice { get; set; }

        /// <summary>
        /// Giao nhanh
        /// </summary>
        public bool? IsFast { get; set; }

        /// <summary>
        /// Phí giao nhanh
        /// </summary>
        public decimal? IsFastPrice { get; set; }

        /// <summary>
        /// Bảo hiểm
        /// </summary>
        public bool? IsInsurance { get; set; }

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        public decimal? InsuranceMoney { get; set; }

        /// <summary>
        /// Tổng giá của shop (VNĐ)
        /// </summary>
        public decimal? PriceVND { get; set; }

        /// <summary>
        /// Tổng giá của shop (Tệ)
        /// </summary>
        public decimal? PriceCNY { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; }

        /// <summary>
        /// Phí mua hàng
        /// </summary>
        public decimal? FeeBuyPro { get; set; }

        /// <summary>
        /// Tổng tiền (bao gồm phí)
        /// </summary>
        public decimal? TotalPriceVND { get; set; }

        /// <summary>
        /// Họ tên người đặt hàng
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Số điện thoại người đặt hàng
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email người đặt hàng
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Địa chỉ người đặt hàng
        /// </summary>
        public string Address { get; set; }

        public List<OrderTempModel> OrderTemps { get; set; }

        public string OrderTempsJson { get; set; }
    }
}
