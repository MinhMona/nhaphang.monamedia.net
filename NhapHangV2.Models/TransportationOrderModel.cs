using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class TransportationOrderModel : AppDomainModel
    {
        /// <summary>
        /// ID User
        /// </summary>
        public Nullable<int> UID { get; set; }

        /// <summary>
        /// User đặt hàng
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Tỉ giá
        /// </summary>
        public decimal? Currency { get; set; }

        public int? SmallPackageId { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Tên trạng thái
        /// </summary>
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case (int)StatusGeneralTransportationOrder.Huy:
                        return "Hủy";
                    case (int)StatusGeneralTransportationOrder.ChoDuyet:
                        return "Chờ duyệt";
                    case (int)StatusGeneralTransportationOrder.DaDuyet:
                        return "Đã duyệt";
                    case (int)StatusGeneralTransportationOrder.VeKhoTQ:
                        return "Đã về kho TQ";
                    case (int)StatusGeneralTransportationOrder.VeKhoVN:
                        return "Đã về kho VN";
                    case (int)StatusGeneralTransportationOrder.DaThanhToan:
                        return "Đã thanh toán";
                    case (int)StatusGeneralTransportationOrder.DaHoanThanh:
                        return "Đã hoàn thành";
                    case (int)StatusGeneralTransportationOrder.DaKhieuNai:
                        return "Đã khiếu nại";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Ghi chú khách hàng
        /// </summary>
        [StringLength(1000)]
        public string Note { get; set; }

        /// <summary>
        /// Ghi chú nhân viên
        /// </summary>
        [StringLength(1000)]
        public string StaffNote { get; set; }

        /// <summary>
        /// Tổng tiền (Tệ)
        /// </summary>
        public decimal? TotalPriceCNY { get; set; }

        /// <summary>
        /// Tổng tiền (VNĐ)
        /// </summary>
        public decimal? TotalPriceVND { get; set; }

        [StringLength(1000)]
        public string ExportRequestNote { get; set; }

        /// <summary>
        /// Ngày về YKXK
        /// </summary>
        public DateTime? DateExportRequest { get; set; }

        /// <summary>
        /// Ngày XK
        /// </summary>
        public DateTime? DateExport { get; set; }

        /// <summary>
        /// Ghi chú hủy đơn
        /// </summary>
        public string CancelReason { get; set; }

        public int? ShippingTypeVN { get; set; }

        /// <summary>
        /// ID Kho TQ
        /// </summary>
        public int? WareHouseFromId { get; set; }

        /// <summary>
        /// Kho TQ
        /// </summary>
        public string WareHouseFrom { get; set; }

        /// <summary>
        /// ID Kho VN
        /// </summary>
        public int? WareHouseId { get; set; }

        /// <summary>
        /// Kho VN
        /// </summary>
        public string WareHouseTo { get; set; }

        /// <summary>
        /// ID Phương thức vận chuyển
        /// </summary>
        public int? ShippingTypeId { get; set; }

        /// <summary>
        /// Phương thức vận chuyển
        /// </summary>
        public string ShippingTypeName { get; set; }

        /// <summary>
        /// HTVC
        /// </summary>
        public string ShippingTypeVNName { get; set; }

        /// <summary>
        /// Phí lưu kho
        /// </summary>
        public decimal? WarehouseFee { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string OrderTransactionCode { get; set; }

        /// <summary>
        /// Cân tính tiền (Kg) - Lấy từ SmallPackage
        /// </summary>
        public decimal? PayableWeight { get; set; }

        /// <summary>
        /// Thể tích tính tiền (m3) - Lấy từ SmallPackage
        /// </summary>
        public decimal? VolumePayment { get; set; }

        /// <summary>
        /// Phụ phí hàng đặc biệt (VNĐ) - Lấy từ SmallPackage
        /// </summary>
        public decimal? AdditionFeeVND { get; set; }

        /// <summary>
        /// Cước vật tư (VNĐ) - Lấy từ SmallPackage
        /// </summary>
        public decimal? SensorFeeVND { get; set; }

        /// <summary>
        /// Ngày về kho TQ - Lấy từ SmallPackage
        /// </summary>
        public DateTime? DateInTQWarehouse { get; set; }

        /// <summary>
        /// Ngày về kho VN - Lấy từ SmallPackage
        /// </summary>
        public DateTime? DateInLasteWareHouse { get; set; }

        public List<SmallPackageModel> SmallPackages { get; set; }

        /// <summary>
        /// Tiền cân / Kg
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
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
        /// Số lượng
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Amount { get; set; } = 0;

        /// <summary>
        /// Loại sản phẩm
        /// </summary>
        [StringLength(1000)]
        public string Category { get; set; } = string.Empty;

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
        /// ID Saler tạo dùm
        /// </summary>
        public int? SalerID { get; set; }

        public decimal? FeeService { get; set; }
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
        public decimal? ActualShippingCost { get; set; } 
    }
}
