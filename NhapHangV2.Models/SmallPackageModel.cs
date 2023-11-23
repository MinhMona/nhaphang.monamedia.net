using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class SmallPackageModel : AppDomainModel
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
        public decimal? VolumePayment { get; set; }

        /// <summary>
        /// Tiền /m3
        /// </summary>
        public decimal? PriceVolume { get; set; }

        /// <summary>
        /// Trạng thái thanh toán
        /// </summary>
        public bool? IsPayment { get; set; }

        /// <summary>
        /// UID (SmallPackage = MainOrder = TransportationOrder)
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// Id đơn hàng mua hộ
        /// </summary>
        public int? MainOrderId { get; set; }

        /// <summary>
        /// Id bao lớn
        /// </summary>
        public int? BigPackageId { get; set; }

        /// <summary>
        /// Tên bao lớn
        /// </summary>
        public string BigPackageName { get; set; }

        /// <summary>
        /// Id mã đơn hàng
        /// </summary>
        public int? MainOrderCodeId { get; set; }

        /// <summary>
        /// Id đơn hàng vận chuyển hộ
        /// </summary>
        public int? TransportationOrderId { get; set; }

        public string MainOrderCode
        {
            get
            {
                if (MainOrderId != 0)
                    return string.Format("Mua hộ: {0}", MainOrderId);
                if (TransportationOrderId != 0)
                    return string.Format("Ký gửi: {0}", TransportationOrderId);
                return string.Format("Trôi nổi: {0}", MainOrderId);
            }
        }

        /// <summary>
        /// Mã vận đơn (Barcode)
        /// </summary>
        public string OrderTransactionCode { get; set; }

        /// <summary>
        /// Loại hàng
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// Cân nặng (kg)
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Khối (m3) - Cân quy đổi (kg)
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Cân tính tiền (Kg)
        /// </summary>
        public decimal? PayableWeight { get; set; }

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
                    case (int)StatusSmallPackage.DaHuy:
                        return "Đã hủy";
                    case (int)StatusSmallPackage.MoiTao:
                        return "Mới tạo";
                    case (int)StatusSmallPackage.VeKhoTQ:
                        return "Đã về kho TQ";
                    case (int)StatusSmallPackage.XuatKhoTQ:
                        return "Xuất kho TQ";
                    case (int)StatusSmallPackage.VeKhoVN:
                        return "Đã về kho VN";
                    case (int)StatusSmallPackage.DaGiao:
                        return "Đã giao";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Description { get; set; }

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
        /// Dài x rộng x cao
        /// </summary>
        public string LWH
        {
            get
            {
                return string.Format("{0} x {1} x {2}", Length, Width, Height);
            }
        }

        /// <summary>
        /// Trạng thái xác nhận
        /// </summary>
        public int? FloatingStatus { get; set; }

        /// <summary>
        /// Tên trạng thái xác nhận
        /// </summary>
        public string FloatingStatusName
        {
            get
            {
                switch (FloatingStatus)
                {
                    case (int)StatusConfirmSmallPackage.ChuaCoNguoiNhan:
                        return "Chưa có người nhận";
                    case (int)StatusConfirmSmallPackage.DangChoXacNhan:
                        return "Đang chờ xác nhận";
                    case (int)StatusConfirmSmallPackage.DaCoNguoiNhan:
                        return "Đã có người nhận";
                    default:
                        return String.Empty;
                }
            }
        }

        /// <summary>
        /// Nhân viên kho kinh doanh (ghi chú nội bộ)
        /// </summary>
        public string StaffNoteCheck { get; set; }

        /// <summary>
        /// Kiện trôi nổi - UserName xác nhận
        /// </summary>
        public string FloatingUserName { get; set; }

        /// <summary>
        /// Kiện trôi nổi - Số điện thoại
        /// </summary>
        public string FloatingUserPhone { get; set; }

        /// <summary>
        /// Kiện trôi nổi - Phí ship (tệ)
        /// </summary>
        public decimal? FeeShip { get; set; }

        /// <summary>
        /// Khách hàng ghi chú (Kiện trôi nổi)
        /// </summary>
        public string UserNote { get; set; }

        /// <summary>
        /// Tổng tiền
        /// </summary>
        public decimal? TotalPrice { get; set; }

        /// <summary>
        /// Ngày đến kho TQ
        /// </summary>
        public DateTime? DateInTQWarehouse { get; set; }

        /// <summary>
        /// Nhân viên thay đổi trạng thái thành "Đến kho TQ"
        /// </summary>
        public string StaffTQWarehouse { get; set; }

        /// <summary>
        /// Ngày Scan kiểm hàng TQ
        /// </summary>
        public DateTime? DateScanTQ { get; set; }

        /// <summary>
        /// Ngày Scan kiểm hàng VN
        /// </summary>
        public DateTime? DateScanVN { get; set; }

        /// <summary>
        /// Số lượng sản phẩm
        /// </summary>
        public int? TotalOrder { get; set; }

        /// <summary>
        /// Số lượng sản phẩm của sản phẩm
        /// </summary>
        public int? TotalOrderQuantity { get; set; }

        public int? TotalStatus0 { get; set; }

        public int? TotalStatus1 { get; set; }

        public int? TotalStatus2 { get; set; }

        public int? TotalStatus3 { get; set; }

        public int? TotalStatus4 { get; set; }

        /// <summary>
        /// Loại đơn hàng
        /// </summary>
        public int? OrderType
        {
            get
            {
                if (MainOrderCodeId != null && MainOrderCodeId > 0)
                    return 1;
                else if (TransportationOrderId != null && TransportationOrderId > 0)
                    return 2;
                else return 3;
            }
        }

        /// <summary>
        /// Tên loại đơn hàng
        /// </summary>
        public string OrderTypeName
        {
            get
            {
                switch (OrderType)
                {
                    case (int)TypeOrder.DonHangMuaHo:
                        return "Mua hộ";
                    case (int)TypeOrder.DonKyGui:
                        return "Ký gửi";
                    case (int)TypeOrder.KhongXacDinh:
                        return "Trôi nổi";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Bao hàng
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Kiểm đếm
        /// </summary>
        public bool? IsCheckProduct { get; set; }

        /// <summary>
        /// Đóng gỗ
        /// </summary>
        public bool? IsPackged { get; set; }

        /// <summary>
        /// Bảo hiểm
        /// </summary>
        public bool? IsInsurance { get; set; }

        /// <summary>
        /// Ngày lưu kho
        /// </summary>
        public DateTime? DateInLasteWareHouse { get; set; }

        /// <summary>
        /// Tổng ngày lưu kho
        /// </summary>
        public decimal? TotalDateInLasteWareHouse { get; set; }

        /// <summary>
        /// Ngày xuất kho
        /// </summary>
        public DateTime? DateOutWarehouse { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        public string IMG { get; set; }

        /// <summary>
        /// Phụ phí hàng đặc biệt (tệ)
        /// </summary>
        public decimal? AdditionFeeCNY { get; set; }

        /// <summary>
        /// Phụ phí hàng đặc biệt (VNĐ)
        /// </summary>
        public decimal? AdditionFeeVND { get; set; }

        /// <summary>
        /// Cước vật tư (tệ)
        /// </summary>
        public decimal? SensorFeeCNY { get; set; }

        /// <summary>
        /// Cước vật tư (VNĐ)
        /// </summary>
        public decimal? SensorFeeVND { get; set; }

        /// <summary>
        /// Tỷ giá
        /// </summary>
        public decimal? Currency { get; set; }

        /// <summary>
        /// Cờ kiện thất lạc
        /// </summary>
        public bool? IsLost { get; set; }

        /// <summary>
        /// Nhân viên thay đổi trạng thái thành "Đến kho VN"
        /// </summary>
        public string StaffVNWarehouse { get; set; }

        /// <summary>
        /// Nhân viên thay đổi trạng thái thành "Đã thanh toán"
        /// </summary>
        public string StaffVNOutWarehouse { get; set; }

        /// <summary>
        /// Id nơi mà hàng đang ở đó
        /// </summary>
        public int? CurrentPlaceId { get; set; }

        /// <summary>
        /// User Name của UID (SmallPackage = MainOrder = TransportationOrder)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Phone của UID (SmallPackage = MainOrder = TransportationOrder)
        /// </summary>
        public string Phone { get; set; }

        public bool? IsTemp { get; set; }

        public string PackageCodeTemp { get; set; }

        public bool? IsHelpMoving { get; set; }

        public decimal? DonGia { get; set; }

        public decimal? PriceWeight { get; set; }

        public DateTime? DateInVNTemp { get; set; }
    }
}
