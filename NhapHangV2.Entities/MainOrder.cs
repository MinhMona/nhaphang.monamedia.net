
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Entities
{
    public class MainOrder : DomainEntities.AppDomain
    {
        public string ImageOrigin { get; set; }
        public bool? IsEditFeeBuyProPercent { get; set; } = false;
        /// <summary>
        /// Phí bảo hiểm (%)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EditedFeeBuyProPercent { get; set; } = 0;
        /// <summary>
        /// Ngày shop phát hàng
        /// </summary>
        public DateTime? DateSendGoods { get; set; }

        /// <summary>
        /// Ngày đang về VN
        /// </summary>
        public DateTime? DateComingVN { get; set; }

        /// <summary>
        /// ID User người đặt hàng
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// Đổi cân nặng trên View
        /// </summary>
        public bool? IsChangeTQVNWeight { get; set; } = false;

        /// <summary>
        /// Đổi phí vận chuyển trên View
        /// </summary>
        public bool? IsChangeFeeWeight { get; set; } = false;

        /// <summary>
        /// ID Shop
        /// </summary>
        [StringLength(50)]
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// Tên shop
        /// </summary>
        [StringLength(50)]
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// Website
        /// </summary>
        [StringLength(10)]
        public string Site { get; set; } = string.Empty;

        /// <summary>
        /// Giao hàng tận nhà
        /// </summary>
        public bool? IsFastDelivery { get; set; } = false;

        /// <summary>
        /// Phí giao hàng tận nhà
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? IsFastDeliveryPrice { get; set; } = 0;

        /// <summary>
        /// Kiểm đếm
        /// </summary>
        public bool? IsCheckProduct { get; set; } = false;

        /// <summary>
        /// Phí kiểm đếm (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? IsCheckProductPrice { get; set; } = 0;

        /// <summary>
        /// Phí kiểm đếm (Tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IsCheckProductPriceCNY { get; set; } = 0;

        /// <summary>
        /// Đóng gỗ
        /// </summary>
        public bool? IsPacked { get; set; } = false;

        /// <summary>
        /// Phí đóng gỗ (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? IsPackedPrice { get; set; } = 0;

        /// <summary>
        /// Phí đóng gỗ (Tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IsPackedPriceCNY { get; set; } = 0;

        /// <summary>
        /// Tiền hàng (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceVND { get; set; } = 0;

        /// <summary>
        /// Tiền hàng (tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PriceCNY { get; set; } = 0;

        /// <summary>
        /// Phí ship TQ (Tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeShipCNCNY { get; set; } = 0;

        /// <summary>
        /// Phí ship TQ (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeShipCN { get; set; } = 0;

        /// <summary>
        /// Phí mua hàng (Phí mua hàng - Chiết khấu (VNĐ))
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeBuyPro { get; set; } = 0;

        /// <summary>
        /// Phần trăm chiết khẩu phí mua hàng
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeBuyProCK { get; set; } = 0;

        /// <summary>
        /// Phí mua hàng tệ
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CKFeeBuyPro { get; set; } = 0;

        /// <summary>
        /// Phí vận chuyển TQ - VN - VNĐ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeWeight { get; set; } = 0;

        /// <summary>
        /// Chiết khấu - Phí vận chuyển TQ - VN
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeWeightCK { get; set; } = 0;

        /// <summary>
        /// Họ và tên người nhận
        /// </summary>
        [StringLength(100)]
        public string ReceiverFullName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại người nhận
        /// </summary>
        [StringLength(20)]
        public string ReceiverPhone { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ nhận hàng
        /// </summary>
        [StringLength(1000)]
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Email người nhận
        /// </summary>
        [StringLength(1000)]
        public string ReceiverEmail { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = 0;

        /// <summary>
        /// Đã thanh toán (Số tiền đã cọc - Đã trả)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Deposit { get; set; } = 0;

        /// <summary>
        /// Tỉ giá
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? CurrentCNYVN { get; set; } = 0;

        /// <summary>
        /// Tổng tiền (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceVND { get; set; } = 0;

        /// <summary>
        /// ID nhân viên kinh doanh
        /// </summary>
        public int? SalerId { get; set; } = 0;

        /// <summary>
        /// Id nhân viên đặt hàng
        /// </summary>
        public int? DatHangId { get; set; } = 0;

        /// <summary>
        /// ID Kho nhận (VN)
        /// </summary>
        public int? ReceivePlace { get; set; } = 0;

        /// <summary>
        /// ID kho TQ
        /// </summary>
        public int? FromPlace { get; set; } = 0;

        /// <summary>
        /// Phương thức vận chuyển
        /// </summary>
        public int? ShippingType { get; set; } = 0;

        /// <summary>
        /// Tổng tiền phải cọc (Số tiền phải cọc)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? AmountDeposit { get; set; } = 0;

        /// <summary>
        /// Tổng cân nặng (Kg)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OrderWeight { get; set; } = 0;

        /// <summary>
        /// TQ - VN - Cân nặng
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TQVNWeight { get; set; } = 0;

        /// <summary>
        /// TQ - VN - Cân khối
        /// </summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal? TQVNVolume { get; set; } = 0;

        /// <summary>
        /// Tổng số tiền mua thật (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceReal { get; set; } = 0;

        /// <summary>
        /// Tổng số tiền mua thật (Tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalPriceRealCNY { get; set; } = 0;

        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public int? OrderType { get; set; } = 0;

        /// <summary>
        /// Báo giá
        /// </summary>
        public bool? IsCheckNotiPrice { get; set; } = false;

        /// <summary>
        /// Phí lưu kho
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeInWareHouse { get; set; } = 0;

        /// <summary>
        /// Ngày đặt cọc
        /// </summary>
        public DateTime? DepositDate { get; set; }

        /// <summary>
        /// Ngày khách thanh toán
        /// </summary>
        public DateTime? PayDate { get; set; }

        /// <summary>
        /// Ngày đặt hàng
        /// </summary>
        public DateTime? DateBuy { get; set; }

        /// <summary>
        /// Ngày đã về kho TQ
        /// </summary>
        public DateTime? DateTQ { get; set; }

        /// <summary>
        /// Ngày đã về kho VN
        /// </summary>
        public DateTime? DateVN { get; set; }

        /// <summary>
        /// Ngày hoàn thành
        /// </summary>
        public DateTime? CompleteDate { get; set; }

        /// <summary>
        /// Ngày hủy
        /// </summary>
        public DateTime? CancelDate { get; set; }

        /// <summary>
        /// Ngày khiếu nại
        /// </summary>
        public DateTime? ComplainDate { get; set; }

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
        /// Phí bảo hiểm (%)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? InsurancePercent { get; set; } = 0;

        /// <summary>
        /// Phí ship TQ thật (tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeShipCNRealCNY { get; set; } = 0;

        /// <summary>
        /// Đủ mã vận đơn
        /// </summary>
        public bool? IsDoneSmallPackage { get; set; } = false;

        /// <summary>
        /// Phí ship TQ thật (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeShipCNReal { get; set; } = 0;

        /// <summary>
        /// Ngày dự kiến
        /// </summary>
        public DateTime? ExpectedDate { get; set; }

        /// <summary>
        /// Đặt cọc (%)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LessDeposit { get; set; } = 0;

        /// <summary>
        /// Phí dịch vụ (%)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeBuyProPT { get; set; } = 0;

        /// <summary>
        /// Phí mua hàng của User
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeBuyProUser { get; set; } = 0;

        /// <summary>
        /// Đơn có đang bị khiếu nại
        /// </summary>
        public bool? IsComplain { get; set; } = false;

        /// <summary>
        /// Đơn có cập nhật sản phẩm trong đơn hàng
        /// </summary>
        public bool? IsUpdatePrice { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeShipCNToVN { get; set; } = 0;

        /// <summary>
        /// (Tổng phụ phí)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Surcharge { get; set; } = 0;

        /// <summary>
        /// Tổng link
        /// </summary>
        [NotMapped]
        public int TotalLink { get; set; } = 0;

       

        /// <summary>
        /// Tên kho VN
        /// </summary>
        [NotMapped]
        public string ReceivePlaceName { get; set; } = string.Empty;

        /// <summary>
        /// Tên kho TQ
        /// </summary>
        [NotMapped]
        public string FromPlaceName { get; set; } = string.Empty;

        /// <summary>
        /// Tên phương thức vận chuyển
        /// </summary>
        [NotMapped]
        public string ShippingTypeName { get; set; } = string.Empty;

        /// <summary>
        /// UserName đặt hàng
        /// </summary>
        [NotMapped]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Số dư người đặt hàng
        /// </summary>
        [NotMapped]
        public decimal Wallet { get; set; } = 0;

        /// <summary>
        /// Họ và tên người đặt hàng
        /// </summary>
        [NotMapped]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ người đặt hàng
        /// </summary>
        [NotMapped]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Email người đặt hàng
        /// </summary>
        [NotMapped]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại người đặt hàng
        /// </summary>
        [NotMapped]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Tổng tiền hàng (Danh sách)
        /// </summary>
        [NotMapped]
        public decimal? TotalAllPrice { get; set; } = 0;

        /// <summary>
        /// Tổng tiền hàng mua thật (Danh sách)
        /// </summary>
        [NotMapped]
        public decimal? TotalAllPriceReal { get; set; } = 0;

        /// <summary>
        /// Tổng tiền hàng đã thanh toán (Danh sách)
        /// </summary>
        [NotMapped]
        public decimal? TotalAllDeposit { get; set; } = 0;

        /// <summary>
        /// Tổng đơn mới
        /// </summary>
        [NotMapped]
        public int? TotalStatus0 { get; set; } = 0;

        /// <summary>
        /// Tổng đơn đã đặt cọc
        /// </summary>
        [NotMapped]
        public int? TotalStatus1 { get; set; } = 0;

        /// <summary>
        /// Tổng đơn đã mua hàng
        /// </summary>
        [NotMapped]
        public int? TotalStatus2 { get; set; } = 0;

        /// <summary>
        /// Tổng đơn về kho TQ
        /// </summary>
        [NotMapped]
        public int? TotalStatus5 { get; set; } = 0;

        /// <summary>
        /// Tổng đơn về kho VN
        /// </summary>
        [NotMapped]
        public int? TotalStatus6 { get; set; } = 0;

        /// <summary>
        /// Tổng đơn đã thanh toán
        /// </summary>
        [NotMapped]
        public int? TotalStatus7 { get; set; } = 0;

        /// <summary>
        /// Tổng đơn đã hoàn thành
        /// </summary>
        [NotMapped]
        public int? TotalStatus9 { get; set; } = 0;

        /// <summary>
        /// Tổng đơn hủy
        /// </summary>
        [NotMapped]
        public int? TotalStatus10 { get; set; } = 0;

        /// <summary>
        /// Mã đơn hàng - Mã vận đơn
        /// </summary>
        [NotMapped]
        public string MainOrderTransactionCode { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách phụ phí
        /// </summary>
        [NotMapped]
        public List<FeeSupport> FeeSupports { get; set; } = new List<FeeSupport>();

        /// <summary>
        /// Danh sách mã đơn hàng
        /// </summary>
        [NotMapped]
        public List<MainOrderCode> MainOrderCodes { get; set; } = new List<MainOrderCode>();

        /// <summary>
        /// Danh sách mã vận đơn
        /// </summary>
        [NotMapped]
        public List<SmallPackage> SmallPackages { get; set; } = new List<SmallPackage>();

        /// <summary>
        /// Danh sách sản phẩm
        /// </summary>
        [NotMapped]
        public List<Order> Orders { get; set; } = new List<Order>();

        /// <summary>
        /// Lịch sử thay đổi
        /// </summary>
        [NotMapped]
        public List<HistoryOrderChange> HistoryOrderChanges { get; set; } = new List<HistoryOrderChange>();

        /// <summary>
        /// Lịch sử thanh toán
        /// </summary>
        [NotMapped]
        public List<PayOrderHistory> PayOrderHistories { get; set; } = new List<PayOrderHistory>();

        /// <summary>
        /// Lịch sử khiếu nại
        /// </summary>
        [NotMapped]
        public List<Complain> Complains { get; set; } = new List<Complain>();

        /// <summary>
        /// Tính hoa hồng cho đặt hàng và saler
        /// </summary>
        [NotMapped]
        public List<StaffIncome> StaffIncomes { get; set; } = new List<StaffIncome>();

        /// <summary>
        /// Nhắn tin với khách hàng, với nội bộ
        /// </summary>
        [NotMapped]
        public List<OrderComment> OrderComments { get; set; } = new List<OrderComment>();

        /// <summary>
        /// UserName nhân viên đặt hàng
        /// </summary>
        [NotMapped]
        public string OrdererUserName { get; set; } = string.Empty;

        /// <summary>
        /// UserName Nhân viên kinh doanh
        /// </summary>
        [NotMapped]
        public string SalerUserName { get; set; } = string.Empty;

        /// <summary>
        /// Id để khi đặt hàng xong sẽ xóa trong OrderTemp
        /// </summary>
        [NotMapped]
        public int ShopTempId { get; set; } = 0;

        [NotMapped]
        public int? OldStatus { get; set; }
    }
}
