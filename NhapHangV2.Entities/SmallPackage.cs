
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class SmallPackage : DomainEntities.AppDomain
    {
        /// <summary>
        /// Nhân viên xuất kho TQ
        /// </summary>
        public string StaffOutTQ { get; set; } = string.Empty;
        /// <summary>
        /// Ngày xuất kho TQ
        /// </summary>
        public DateTime? DateOutTQ { get; set; }
        /// <summary>
        /// Khối tính tiền
        /// </summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal? VolumePayment { get; set; } = 0;

        /// <summary>
        /// Tiền /m3
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceVolume { get; set; } = 0;

        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        public bool? IsPayment { get; set; } = false;

        /// <summary>
        /// Id đơn vận chuyển hộ
        /// </summary>
        public int? TransportationOrderId { get; set; } = 0;

        /// <summary>
        /// Id đơn mua hộ
        /// </summary>
        public int? MainOrderId { get; set; } = 0;

        /// <summary>
        /// Id bao lớn
        /// </summary>
        public int? BigPackageId { get; set; } = 0;

        /// <summary>
        /// Id mã đơn hàng
        /// </summary>
        public int? MainOrderCodeId { get; set; } = 0;

        /// <summary>
        /// UID (SmallPackage = MainOrder = TransportationOrder)
        /// </summary>
        public int? UID { get; set; } = 0;

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        [StringLength(50)]
        public string OrderTransactionCode { get; set; } = string.Empty;

        /// <summary>
        /// Loại hàng
        /// </summary>
        [StringLength(50)]
        public string ProductType { get; set; } = string.Empty;

        /// <summary>
        /// Cân nặng - Cân thực (kg)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Weight { get; set; } = 0;

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; } = 1; //Mặc định là "Chưa về kho TQ"

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Kiện trôi nổi - UserName xác nhận
        /// </summary>
        public string FloatingUserName { get; set; } = string.Empty;

        /// <summary>
        /// Kiện trôi nổi - Số điện thoại
        /// </summary>
        public string FloatingUserPhone { get; set; } = string.Empty;

        /// <summary>
        /// Kiện trôi nội - Trạng thái xác nhận
        /// </summary>
        public int? FloatingStatus { get; set; } = 0;

        /// <summary>
        /// Kiện trôi nổi - Phí ship (tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeShip { get; set; } = 0;

        /// <summary>
        /// Khách hàng ghi chú (Kiện trôi nổi)
        /// </summary>
        public string UserNote { get; set; } = string.Empty;

        /// <summary>
        /// Nhân viên thay đổi trạng thái thành "Đến kho VN"
        /// </summary>
        [StringLength(50)]
        public string StaffVNWarehouse { get; set; } = string.Empty;

        /// <summary>
        /// Ngày xuất kho
        /// </summary>
        public DateTime? DateOutWarehouse { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        public string IMG { get; set; } = string.Empty;

        /// <summary>
        /// Dài
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Length { get; set; } = 0;

        /// <summary>
        /// Rộng
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Height { get; set; } = 0;

        /// <summary>
        /// Cao
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Width { get; set; } = 0;

        /// <summary>
        /// Nhân viên kho kinh doanh (Ghi chú nội bộ)
        /// </summary>
        [StringLength(100)]
        public string StaffNoteCheck { get; set; } = string.Empty;

        /// <summary>
        /// Nhân viên thay đổi trạng thái thành "Đến kho TQ"
        /// </summary>
        [StringLength(50)]
        public string StaffTQWarehouse { get; set; } = string.Empty;

        /// <summary>
        /// Nhân viên thay đổi trạng thái thành "Đã thanh toán"
        /// </summary>
        [StringLength(50)]
        public string StaffVNOutWarehouse { get; set; } = string.Empty;

        /// <summary>
        /// Tổng tiền
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPrice { get; set; } = 0;

        /// <summary>
        /// Ngày kiểm hàng ở TQ
        /// </summary>
        public DateTime? DateScanTQ { get; set; }

        /// <summary>
        /// Ngày đến kho TQ
        /// </summary>
        public DateTime? DateInTQWarehouse { get; set; }

        /// <summary>
        /// Ngày kiểm hàng ở VN
        /// </summary>
        public DateTime? DateScanVN { get; set; }

        /// <summary>
        /// Ngày lưu kho (Ngày đến kho VN)
        /// </summary>
        public DateTime? DateInLasteWareHouse { get; set; }

        /// <summary>
        /// Tổng ngày lưu kho
        /// </summary>
        [NotMapped]
        public decimal? TotalDateInLasteWareHouse
        {
            get
            {
                if (DateInLasteWareHouse == null)
                    return 0;
                return Math.Floor(Convert.ToDecimal(DateTime.Now.Subtract((DateTime)DateInLasteWareHouse).TotalDays)); //Kiểm tra cái này lại
            }
        }

        /// <summary>
        /// Phụ phí hàng đặc biệt (tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdditionFeeCNY { get; set; } = 0;

        /// <summary>
        /// Phụ phí hàng đặc biệt (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? AdditionFeeVND { get; set; } = 0;

        /// <summary>
        /// Cước vật tư (tệ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SensorFeeCNY { get; set; } = 0;

        /// <summary>
        /// Cước vật tư (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? SensorFeeVND { get; set; } = 0;

        /// <summary>
        /// Cờ kiện thất lạc
        /// </summary>
        public bool? IsLost { get; set; } = false;

        /// <summary>
        /// Id nơi mà hàng đang ở đó
        /// </summary>
        public int? CurrentPlaceId { get; set; } = 0;

        /// <summary>
        /// Kiểm đếm
        /// </summary>
        [NotMapped]
        public bool? IsCheckProduct { get; set; } = false;

        /// <summary>
        /// Đóng gỗ
        /// </summary>
        [NotMapped]
        public bool? IsPackged { get; set; } = false;

        /// <summary>
        /// Bảo hiểm
        /// </summary>
        [NotMapped]
        public bool? IsInsurance { get; set; } = false;

        [NotMapped]
        public int? TotalStatus0 { get; set; } = 0;

        [NotMapped]
        public int? TotalStatus1 { get; set; } = 0;

        [NotMapped]
        public int? TotalStatus2 { get; set; } = 0;

        [NotMapped]
        public int? TotalStatus3 { get; set; } = 0;

        [NotMapped]
        public int? TotalStatus4 { get; set; } = 0;

        /// <summary>
        /// Loại đơn hàng
        /// </summary>
        [NotMapped]
        public int? OrderType { get; set; } = 0;

        /// <summary>
        /// Bao hàng
        /// </summary>
        [NotMapped]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Khối (m3) - Cân quy đổi (kg)
        /// </summary>
        [NotMapped]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Volume
        {
            get
            {
                //if (Length > 0 && Width > 0 && Height > 0) {
                //    var volume = (Length * Width * Height) ?? 0;
                //    return Math.Round(volume / 6000, 2);
                //}
                //else
                //    return 0;
                return Weight;
            }
        }

        /// <summary>
        /// Cân tính tiền (Kg)
        /// </summary>
        [NotMapped]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PayableWeight
        {
            get
            {
                return Math.Round((Weight > Volume ? Weight : Volume) ?? 0, 2);
            }
        }

        /// <summary>
        /// Số lượng sản phẩm
        /// </summary>
        [NotMapped]
        public int? TotalOrder { get; set; } = 0;

        /// <summary>
        /// Số lượng sản phẩm của sản phẩm
        /// </summary>
        [NotMapped]
        public int? TotalOrderQuantity { get; set; } = 0;

        /// <summary>
        /// (PUT) Cờ cập nhập - xác nhận kiện trôi nổi
        /// </summary>
        [NotMapped]
        public bool IsFloating { get; set; } = false;

        /// <summary>
        /// (POST) Cờ gán đơn cho khách
        /// </summary>
        [NotMapped]
        public bool IsAssign { get; set; } = false;

        /// <summary>
        /// Loại gán đơn
        /// 1. Gán đơn cho khách mua hộ
        /// 2. Gán đơn cho khách ký gửi
        /// </summary>
        [NotMapped]
        public int AssignType { get; set; } = 0;

        /// <summary>
        /// (POST) Gán đơn cho khách mua hộ - Id đơn hàng gán
        /// </summary>
        [NotMapped]
        public int AssignMainOrderId { get; set; }

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Id User
        /// </summary>
        [NotMapped]
        public int AssignUID { get; set; }

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Ghi chú
        /// </summary>
        [NotMapped]
        public string AssignNote { get; set; }

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Kho TQ
        /// </summary>
        [NotMapped]
        public int? WareHouseFromId { get; set; } = 0;

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Kho đích
        /// </summary>
        [NotMapped]
        public int? WareHouseId { get; set; } = 0;

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Phương thức vận chuyển
        /// </summary>
        [NotMapped]
        public int? ShippingTypeId { get; set; } = 0;

        /// <summary>
        /// Tỷ giá
        /// </summary>
        [NotMapped]
        public decimal? Currency { get; set; } = 0;

        /// <summary>
        /// User Name của UID (SmallPackage = MainOrder = TransportationOrder)
        /// </summary>
        [NotMapped]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Phone của UID (SmallPackage = MainOrder = TransportationOrder)
        /// </summary>
        [NotMapped]
        public string Phone { get; set; } = string.Empty;

        public bool? IsTemp { get; set; } = false;

        public bool? IsHelpMoving { get; set; } = false;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? DonGia { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceWeight { get; set; } = 0;

        public DateTime? DateInVNTemp { get; set; }
    }
}
