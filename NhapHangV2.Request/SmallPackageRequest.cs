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
    public class SmallPackageRequest : AppDomainRequest
    {
        /// <summary>
        /// Nhân viên xuất kho TQ
        /// </summary>
        public string? StaffOutTQ { get; set; }
        /// <summary>
        /// Ngày xuất kho TQ
        /// </summary>
        public DateTime? DateOutTQ { get; set; }

        /// <summary>
        /// Khối tính tiền
        /// </summary>
        public decimal? VolumePayment { get; set; }

        /// <summary>
        /// Mã vận đơn ((POST) Thêm mã kiện ở trang "Kiểm hàng TQ", (PUT) Cập nhật kiện trôi nổi)
        /// </summary>
        [StringLength(50)]
        public string? OrderTransactionCode { get; set; }

        /// <summary>
        /// Id Đơn hàng mua hộ
        /// </summary>
        public int? MainOrderId { get; set; }

        /// <summary>
        /// Id Đơn ký gửi
        /// </summary>
        public int? TransportationOrderId { get; set; }

        /// <summary>
        /// Id Mã đơn hàng
        /// </summary>
        public int? MainOrderCodeId { get; set; }

        /// <summary>
        /// Id bao lớn
        /// </summary>
        public int? BigPackageId { get; set; }

        /// <summary>
        /// Cân nặng - Cân thực (kg)
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Dài
        /// </summary>
        public decimal? Length { get; set; }

        /// <summary>
        /// Rộng
        /// </summary>
        public decimal? Height { get; set; }

        /// <summary>
        /// Cao
        /// </summary>
        public decimal? Width { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Loại sản phẩm
        /// </summary>
        [StringLength(50)]
        public string? ProductType { get; set; }

        /// <summary>
        /// Ghi chú ((POST) Thêm mã kiện ở trang "Kiểm hàng TQ", (PUT) Xác nhận kiện trôi nổi)
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Phụ phí hàng đặc biệt
        /// </summary>
        public decimal? AdditionFeeCNY { get; set; }

        /// <summary>
        /// Phụ phí hàng đặc biệt (VNĐ)
        /// </summary>
        public decimal? AdditionFeeVND { get; set; }

        /// <summary>
        /// Cước vật tư
        /// </summary>
        public decimal? SensorFeeCNY { get; set; }

        /// <summary>
        /// Cước vật tư (VNĐ)
        /// </summary>
        public decimal? SensorFeeVND { get; set; }

        /// <summary>
        /// Nhân viên kho kinh doanh (Ghi chú nội bộ)
        /// </summary>
        [StringLength(100)]
        public string? StaffNoteCheck { get; set; }

        /// <summary>
        /// Khách hàng ghi chú
        /// </summary>
        public string? UserNote { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        public string? IMG { get; set; }

        /// <summary>
        /// Phí ship (Tệ) - ((PUT) Cập nhật kiện trôi nổi)
        /// </summary>
        public decimal? FeeShip { get; set; } = 0;

        /// <summary>
        /// (POST) Thêm mã kiện ở trang "Kiểm hàng TQ"
        /// </summary>
        public bool IsWarehouseTQ { get; set; } = false;

        /// <summary>
        /// (POST) Thêm mã kiện ở trang "Kiểm hàng VN"
        /// </summary>
        public bool IsWarehouseVN { get; set; } = false;

        /// <summary>
        /// (POST) Cờ gán đơn cho khách
        /// </summary>
        public bool IsAssign { get; set; } = false;

        /// <summary>
        /// (PUT) Cờ cập nhập - xác nhận kiện trôi nổi
        /// </summary>
        public bool IsFloating { get; set; } = false;

        /// <summary>
        /// (PUT) Cập nhật - xác nhận kiện trôi nổi - Trạng thái xác nhận
        /// </summary>
        public int FloatingStatus { get; set; }

        /// <summary>
        /// Loại gán đơn
        /// 1. Gán đơn cho khách mua hộ
        /// 2. Gán đơn cho khách ký gửi
        /// </summary>
        public int AssignType { get; set; } = 0;

        /// <summary>
        /// (POST) Gán đơn cho khách mua hộ - Id đơn hàng gán
        /// </summary>
        public int AssignMainOrderId { get; set; } = 0;

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Id User
        /// </summary>
        public int AssignUID { get; set; } = 0;

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Ghi chú
        /// </summary>
        public string? AssignNote { get; set; }

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Kho TQ
        /// </summary>
        public int? WareHouseFromId { get; set; }

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Kho đích
        /// </summary>
        public int? WareHouseId { get; set; }

        /// <summary>
        /// (POST) Gán đơn cho khách ký gửi - Phương thức vận chuyển
        /// </summary>
        public int? ShippingTypeId { get; set; }

        /// <summary>
        /// UID (dành để mapper)
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// Ngày tạo (dành để mapper)
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// Tạo bởi (dành để mapper)
        /// </summary>
        public string? CreatedBy { get; set; }

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
        [Column(TypeName = "decimal(18,0)")]
        public string? Category { get; set; } = string.Empty;

        /// <summary>
        /// Tổng tiền
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalPrice { get; set; } = 0;
    }
}
