
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
    public class TransportationOrder : DomainEntities.AppDomain
    {
        public int? SmallPackageId { get; set; } = 0;

        /// <summary>
        /// ID User
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// Tỷ giá
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Currency { get; set; } = 0;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = (int)StatusGeneralTransportationOrder.ChoDuyet;

        /// <summary>
        /// Loại sản phẩm
        /// </summary>
        [StringLength(1000)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú khách hàng
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú nhân viên
        /// </summary>
        [StringLength(1000)]
        public string StaffNote { get; set; } = string.Empty;

        /// <summary>
        /// Phí vận chuyển(VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? DeliveryPrice { get; set; } = 0;

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
        /// Đóng gỗ
        /// </summary>
        public bool? IsPacked { get; set; } = false;

        /// <summary>
        /// Phí đóng gỗ (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? IsPackedPrice { get; set; } = 0;

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
        /// Tổng tiền (Tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalPriceCNY { get; set; } = 0;

        /// <summary>
        /// Tổng tiền (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPriceVND { get; set; } = 0;

        /// <summary>
        /// Ghi chú
        /// </summary>
        [StringLength(1000)]
        public string ExportRequestNote { get; set; } = string.Empty;

        /// <summary>
        /// Ngày về YKXK
        /// </summary>
        public DateTime? DateExportRequest { get; set; }

        /// <summary>
        /// Ngày XK
        /// </summary>
        public DateTime? DateExport { get; set; }

        /// <summary>
        /// Id Hình thức vận chuyển
        /// </summary>
        public int? ShippingTypeVN { get; set; } = 0;

        /// <summary>
        /// Ghi chú hủy kiện
        /// </summary>
        [StringLength(1000)]
        public string CancelReason { get; set; } = string.Empty;

        /// <summary>
        /// Id kho TQ
        /// </summary>
        public int? WareHouseFromId { get; set; } = 0;

        /// <summary>
        /// Id Kho VN
        /// </summary>
        public int? WareHouseId { get; set; } = 0;

        /// <summary>
        /// ID Phương thức vận chuyển
        /// </summary>
        public int? ShippingTypeId { get; set; } = 0;

        /// <summary>
        /// Tiền cân / Kg
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeWeightPerKg { get; set; } = 0;

        /// <summary>
        /// Tiền thể tích / m3
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeePerVolume { get; set; } = 0;

        /// <summary>
        /// % phí cân nặng chiết khấu
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeWeightCK { get; set; }

        /// <summary>
        /// Phí lưu kho
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? WarehouseFee { get; set; } = 0;

        /// <summary>
        /// Số lượng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Amount { get; set; } = 0;

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string OrderTransactionCode { get; set; } = string.Empty;

        [NotMapped]
        public string UserName { get; set; } = string.Empty;

        [NotMapped]
        public List<SmallPackage> SmallPackages { get; set; } = new List<SmallPackage>();

        /// <summary>
        /// Kho TQ
        /// </summary>
        [NotMapped]
        public string WareHouseFrom { get; set; }

        /// <summary>
        /// Kho VN
        /// </summary>
        [NotMapped]
        public string WareHouseTo { get; set; }

        /// <summary>
        /// Phương thức vận chuyển
        /// </summary>
        [NotMapped]
        public string ShippingTypeName { get; set; }

        /// <summary>
        /// Hình thức vận chuyển
        /// </summary>
        [NotMapped]
        public string ShippingTypeVNName { get; set; }

        /// <summary>
        /// Cân tính tiền (Kg) - Lấy từ SmallPackage
        /// </summary>
        [NotMapped]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PayableWeight { get; set; }
        /// <summary>
        /// Thể tích tính tiền (m3) - Lấy từ SmallPackage
        /// </summary>
        [NotMapped]
        [Column(TypeName = "decimal(18,5)")]
        public decimal? VolumePayment { get; set; }

        /// <summary>
        /// Phụ phí hàng đặc biệt (VNĐ) - Lấy từ SmallPackage
        /// </summary>
        [NotMapped]
        public decimal? AdditionFeeVND { get; set; }

        /// <summary>
        /// Cước vật tư (VNĐ) - Lấy từ SmallPackage
        /// </summary>
        [NotMapped]
        public decimal? SensorFeeVND { get; set; }

        /// <summary>
        /// Ngày về kho TQ - Lấy từ SmallPackage
        /// </summary>
        [NotMapped]
        public DateTime? DateInTQWarehouse { get; set; }

        /// <summary>
        /// Ngày về kho VN - Lấy từ SmallPackage
        /// </summary>
        [NotMapped]
        public DateTime? DateInLasteWareHouse { get; set; }

        /// <summary>
        /// Phí ship nội địa TQ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? CODFee { get; set; } = 0;

        /// <summary>
        /// Phí ship nội địa TQ-Tệ
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CODFeeTQ { get; set; } = 0;

        /// <summary>
        /// ID Saler
        /// </summary>
        public int? SalerID { get; set; }

        /// <summary>
        /// Phí dịch vụ (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeService { get; set; } = 0;

        /// <summary>
        /// Ngày xác nhận
        /// </summary>
        public DateTime? ConfirmDate { get; set; }
        /// <summary>
        /// Ngày về TQ
        /// </summary>
        public DateTime? TQDate { get; set; }
        /// <summary>
        /// Ngày về VN
        /// </summary>
        public DateTime? VNDate { get; set; }
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

        /// <summary>
        /// Ngày khiếu nại
        /// </summary>
        public DateTime? ComplainDate { get; set; }

        /// <summary>
        /// Chi phí vận chuyển thực tế
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? ActualShippingCost { get; set; } = 0;
    }
}
