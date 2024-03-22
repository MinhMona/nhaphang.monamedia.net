using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class MainOrderRequest : AppDomainRequest
    {
        public DateTime? DateShipRequest { get; set; }

        public bool? IsShipRequest { get; set; }
        public string? ImageOrigin { get; set; }
        public bool? IsEditFeeBuyProPercent { get; set; }
        public decimal? EditedFeeBuyProPercent { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        //[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn trạng thái")]
        public int? Status { get; set; }

        /// <summary>
        /// Nhận hàng tại (ID kho VN) 
        /// </summary>
        //[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn kho VN")]
        public int? ReceivePlace { get; set; }

        /// <summary>
        /// ID Kho TQ
        /// </summary>
        //[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn kho TQ")]
        public int? FromPlace { get; set; }

        /// <summary>
        /// Phương thức vận chuyển
        /// </summary>
        //[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phương thức vận chuyển")]
        public int? ShippingType { get; set; }

        /// <summary>
        /// Danh sách mã đơn hàng
        /// </summary>
        public List<MainOrderCodeRequest>? MainOrderCodes { get; set; }

        /// <summary>
        /// Danh sách sản phẩm
        /// </summary>
        public List<OrderRequest>? Orders { get; set; }

        /// <summary>
        /// Danh sách mã vận đơn
        /// </summary>
        public List<SmallPackageRequest>? SmallPackages { get; set; }

        /// <summary>
        /// Đủ mã vận đơn
        /// </summary>
        public bool? IsDoneSmallPackage { get; set; }

        /// <summary>
        /// Danh sách phụ phí
        /// </summary>
        public List<FeeSupportRequest>? FeeSupports { get; set; }

        /// <summary>
        /// Tổng số tiền mua thật (VNĐ)
        /// </summary>
        public decimal? TotalPriceReal { get; set; }

        /// <summary>
        /// Tổng số tiền mua thật (Tệ)
        /// </summary>
        public decimal? TotalPriceRealCNY { get; set; }

        /// <summary>
        /// Phí ship TQ (Tệ)
        /// </summary>
        public decimal? FeeShipCNCNY { get; set; }

        /// <summary>
        /// Phí ship TQ (VNĐ)
        /// </summary>
        public decimal? FeeShipCN { get; set; }

        /// <summary>
        /// Phí ship TQ thật (tệ)
        /// </summary>
        public decimal? FeeShipCNRealCNY { get; set; }

        /// <summary>
        /// Phí ship TQ thật (VNĐ)
        /// </summary>
        public decimal? FeeShipCNReal { get; set; }

        /// <summary>
        /// Phí mua hàng (Phí mua hàng - Chiết khấu (VNĐ))
        /// </summary>
        public decimal? FeeBuyPro { get; set; }

        /// <summary>
        /// Phí mua hàng tệ
        /// </summary>
        public decimal? CKFeeBuyPro { get; set; } 

        /// <summary>
        /// Phần trăm chiết khẩu phí mua hàng
        /// </summary>
        public decimal? FeeBuyProCK { get; set; }

        /// <summary>
        /// Phí vận chuyển TQ - VN - VNĐ
        /// </summary>
        public decimal? FeeWeight { get; set; }

        /// <summary>
        /// Tỉ giá
        /// </summary>
        public decimal? CurrentCNYVN { get; set; }

        /// <summary>
        /// Đặt cọc (%)
        /// </summary>
        public decimal? LessDeposit { get; set; }

        /// <summary>
        /// Phí dịch vụ (%)
        /// </summary>
        public decimal? FeeBuyProPT { get; set; }

        /// <summary>
        /// Kiểm đếm
        /// </summary>
        public bool? IsCheckProduct { get; set; }

        /// <summary>
        /// Phí kiểm đếm (VNĐ)
        /// </summary>
        public decimal? IsCheckProductPrice { get; set; }

        /// <summary>
        /// Phí kiểm đếm (Tệ)
        /// </summary>
        public decimal? IsCheckProductPriceCNY { get; set; }

        /// <summary>
        /// Đóng gỗ
        /// </summary>
        public bool? IsPacked { get; set; }

        /// <summary>
        /// Phí đóng gỗ (VNĐ)
        /// </summary>
        public decimal? IsPackedPrice { get; set; }

        /// <summary>
        /// Phí đóng gỗ (Tệ)
        /// </summary>
        public decimal? IsPackedPriceCNY { get; set; }

        /// <summary>
        /// Bảo hiểm
        /// </summary>
        public bool? IsInsurance { get; set; }

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        public decimal? InsuranceMoney { get; set; }

        /// <summary>
        /// Phí bảo hiểm (%)
        /// </summary>
        public decimal? InsurancePercent { get; set; }

        /// <summary>
        /// Giao hàng tận nhà
        /// </summary>
        public bool? IsFastDelivery { get; set; }

        /// <summary>
        /// Phí giao hàng tận nhà
        /// </summary>
        public decimal? IsFastDeliveryPrice { get; set; }

        /// <summary>
        /// Tổng tiền phải cọc (Số tiền phải cọc)
        /// </summary>
        public decimal? AmountDeposit { get; set; }

        /// <summary>
        /// Đã thanh toán (Đã trả)
        /// </summary>
        public decimal? Deposit { get; set; }

        /// <summary>
        /// Tổng tiền VND
        /// </summary>
        public decimal? TotalPriceVND { get; set; }
        /// <summary>
        /// Tổng tiền Order
        /// </summary>
        public decimal? TotalOrderAmount { get; set; }

        /// <summary>
        /// Tổng tiền hàng
        /// </summary>
        public decimal? PriceVND { get; set; }

        /// <summary>
        /// Tổng tiền hàng tệ
        /// </summary>
        public decimal? PriceCNY { get; set; }

        /// <summary>
        /// Id nhân viên kinh doanh
        /// </summary>
        //[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nhân viên kinh doanh")]
        public int? SalerId { get; set; }

        /// <summary>
        /// Id nhân viên đặt hàng
        /// </summary>
        //[Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nhân viên đặt hàng")]
        public int? DatHangId { get; set; }

        /// <summary>
        /// Đơn hàng TMĐT khác chờ báo giá
        /// </summary>
        public bool? IsCheckNotiPrice { get; set; }

        /// <summary>
        /// TQ - VN - Cân nặng
        /// </summary>
        public decimal? TQVNWeight { get; set; }

        /// <summary>
        /// Đổi cân nặng trên View
        /// </summary>
        public bool? IsChangeTQVNWeight { get; set; }

        /// <summary>
        /// Đổi phí vận chuyển trên View
        /// </summary>
        public bool? IsChangeFeeWeight { get; set; }
    }
}
