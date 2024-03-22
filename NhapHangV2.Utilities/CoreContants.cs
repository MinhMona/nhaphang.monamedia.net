using System;
using System.Collections.Generic;
using System.Text;

namespace NhapHangV2.Utilities
{
    public class CoreContants
    {
        /// <summary>
        /// Trạng thái yêu cầu giao
        /// </summary>
        public enum RequestShipStatus
        {
            /// <summary>
            /// Chưa duyệt
            /// </summary>
            Cancel = 0,
            /// <summary>
            /// Chưa duyệt
            /// </summary>
            UnAccept = 1,
            /// <summary>
            /// Đã duyệt
            /// </summary>
            Accept = 2
        }

        public const int AddNew = 1;
        public const int Update = 2;
        public const int Delete = 3;
        public const int View = 4;
        public const int Download = 5;
        public const int Upload = 6;
        public const int Import = 7;
        public const int Export = 8;
        public const int ViewAll = 9;

        //public const string FullControl = "FullControl";
        //public const string Approve = "Approve";
        //public const string DeleteFile = "DeleteFile";

        public const string UPLOAD_FOLDER_NAME = "Upload";
        public const string EXCEL_FOLDER_NAME = "Excels";
        public const string TEMP_FOLDER_NAME = "Temp";
        public const string TEMPLATE_FOLDER_NAME = "Template";
        public const string CATALOGUE_TEMPLATE_NAME = "CatalogueTemplate.xlsx";
        public const string USER_FOLDER_NAME = "User";
        public const string QR_CODE_FOLDER_NAME = "QRCode";

        public const string GET_TOTAL_NOTIFICATION = "get-total-notification";

        public enum NotificationSettingId
        {
            DangKy = 1,
            LienHe = 2,
            YeuCauNap = 3,
            HuyYeuCauNap = 4,
            DuyetYeuCauNap = 5,
            YeuCauRut = 6,
            HuyYeuCauRut = 7,
            DuyetYeuCauRut = 8,
            AdminChuyenTien = 9,
            AdminRutTien = 10,
            TaoDonMuaHo = 11,
            DatCocMuaHo = 12,
            ThanhToanMuaHo = 13,
            TrangThaiMuaHo = 14,
            SanPhamMuaHo = 15,
            TaoDonKyGui = 16,
            ThanhToanKyGui = 17,
            TrangThaiKyGui = 18,
            TaoThanhToanHo = 19,
            ThanhToanThanhToanHo = 20,
            TrangThaiThanhToanHo = 21,
            TaoKhieuNai = 22,
            TrangThaiKhieuNai = 23,
            TrangThaiMaVanDon = 24,
            YeuCauGiaoMoi = 25,
            TrangThaiYeuCauGiao = 26,
        }

        /// <summary>
        /// Trạng thái của hoa hồng
        /// </summary>
        public enum StatusStaffIncome
        {
            /// <summary>
            /// Chưa thanh toán
            /// </summary>
            Unpaid = 1,
            /// <summary>
            /// Đã thanh toán
            /// </summary>
            Paid = 2
        }

        /// <summary>
        /// Trạng thái của người dùng
        /// </summary>
        public enum StatusUser
        {
            /// <summary>
            /// Đã kích hoạt
            /// </summary>
            Active = 1,
            /// <summary>
            /// Chưa kích hoạt
            /// </summary>
            NotActive = 2,
            /// <summary>
            /// Đang bị khóa
            /// </summary>
            Locked = 3
        }

        /// <summary>
        /// Các quyền
        /// </summary>
        public enum PermissionTypes
        {
            /// <summary>
            /// Admin
            /// </summary>
            Admin = 1,
            /// <summary>
            /// User
            /// </summary>
            User = 2,
            /// <summary>
            /// Quản lý
            /// </summary>
            Manager = 3,
            /// <summary>
            /// Đặt hàng
            /// </summary>
            Orderer = 4,
            /// <summary>
            /// Kho TQ
            /// </summary>
            ChinaWarehouseManager = 5,
            /// <summary>
            /// Kho VN
            /// </summary>
            VietNamWarehouseManager = 6,
            /// <summary>
            /// Saler (nhân viên kinh doanh)
            /// </summary>
            Saler = 7,
            /// <summary>
            /// Kế toán
            /// </summary>
            Accountant = 8,
            /// <summary>
            /// Thủ kho
            /// </summary>
            Storekeepers = 9
        }

        /// <summary>
        /// Các loại giao dịch tệ
        /// </summary>
        public enum WithdrawTypes
        {
            /// <summary>
            /// Rút tiền
            /// </summary>
            RutTien = 2,
            /// <summary>
            /// Nạp tiền
            /// </summary>
            NapTien = 3
        }

        /// <summary>
        /// Danh mục quyền
        /// </summary>
        public enum PermissionContants
        {
            ViewAll = 1,
            View = 2,
            AddNew = 3,
            Update = 4,
            Delete = 5,
            Import = 6,
            Upload = 7,
            Download = 8,
            Export = 9
        }

        /// <summary>
        /// Các trạng thái của lịch sử ví tiền
        /// </summary>
        public enum HistoryPayWalletContents
        {
            NapTien = 1,
            RutTien = 2,
            DatCocMuaHo = 3,
            ThanhToanMuaHo = 4,
            ThanhToanKyGui = 5,
            ThanhToanThanhToanHo = 6,
            HoanTienKhieuNaiMuaHo = 7,
            HoanTienKhieuNaiKyGui = 8,
            HoanTienThanhToanHo = 9,
            HoanTienSanPham = 10,
            HoanTienDaTraMuaHo = 11,
            HoanTienDaTraKyGui = 12,
            HoaHongMuaHo = 13,
            HoaHongKyGui = 14,
            HoaHongThanhToanHo = 15,
            HuyRutTien = 16,
            ThanhToanXuatKho = 17,

        }

        /// <summary>
        /// Các trạng thái của lịch sử ví tiền
        /// </summary>
        public enum HistoryPayWalletCNYContents
        {
            ThanhToanVanChuyenHo = 1,
            RutTien = 2,
            NapTien = 3
        }

        /// <summary>
        /// Các trạng thái của đơn hàng
        /// </summary>
        public enum StatusOrderContants
        {
            DonHuy = 0,
            ChoBaoGia = 1,
            DonMoi = 2,
            DaCoc = 3,
            DaMuaHang = 4,
            ShopPhatHang = 5,
            VeTQ = 6,
            DangVeVN = 7,
            VeVN = 8,
            DaThanhToan = 9,
            HoanThanh = 10,
            KhieuNai = 11,
        }

        /// <summary>
        /// Các trạng thái của lịch sử đơn hàng mua hộ
        /// </summary>
        public enum StatusPayOrderHistoryContants
        {
            DatCoc = 1,
            ThanhToan = 2,
            HoanTienSanPham = 3,
            HoanTienHuyDon = 4
        }

        /// <summary>
        /// Các trạng thái của lịch sử đơn hàng mua hộ
        /// </summary>
        public enum TypePayOrderHistoryContants
        {
            TrucTiep = 1,
            ViDienTu = 2,
        }

        /// <summary>
        /// Các trạng thái của kiện yêu cầu ký gửi
        /// </summary>
        public enum StatusGeneralTransportationOrder
        {
            Huy = 0,
            ChoDuyet = 1,
            DonMoi = 2,
            VeKhoTQ = 3,
            DangVeVN = 4,
            VeKhoVN = 5,
            DaThanhToan = 6,
            DaHoanThanh = 7,
            DaKhieuNai = 8
        }

        /// <summary>
        /// Các trạng thái của kiện thanh toán hộ
        /// </summary>
        public enum StatusPayHelp
        {
            DonHuy = 0,
            ChoDuyet = 1,
            DaDuyet = 2,
            DaThanhToan = 3,
            DaHoanThanh = 4,
        }

        /// <summary>
        /// Trạng thái của kiện trôi nổi
        /// </summary>
        public enum StatusSmallPackage
        {
            DaHuy = 0,
            MoiTao = 1,
            VeKhoTQ = 2,
            XuatKhoTQ = 3,
            VeKhoVN = 4,
            DaGiao = 5,
        }

        /// <summary>
        /// Trạng thái bao lon
        /// </summary>
        public enum StatusBigPackage
        {
            DaHuy = 0,
            MoiTao = 1,
            XuatKhoTQ = 2,
            TrongKhoVN = 3,
        }

        /// <summary>
        /// Trạng thái xác nhận của kiện trôi nổi
        /// </summary>
        public enum StatusConfirmSmallPackage
        {
            ChuaCoNguoiNhan = 1,
            DangChoXacNhan = 2,
            DaCoNguoiNhan = 3,
        }

        /// <summary>
        /// Trạng thái khiếu nại
        /// </summary>
        public enum StatusComplain
        {
            DaHuy = 0,
            MoiTao = 1,
            DaXacNhan = 2,
            DangXuLy = 3,
            HoanThanh = 4
        }

        public enum WalletStatus
        {
            DangChoDuyet = 1,
            DaDuyet = 2,
            Huy = 3
        }

        /// <summary>
        /// Cộng trừ số tiền
        /// </summary>
        public enum DauCongVaTru
        {
            Tru = 1,
            Cong = 2,
        }

        public enum ExportRequestTurnStatus
        {
            ChuaThanhToan = 1,
            DaThanhToan = 2,
            Huy = 3,
        }

        public enum ExportRequestTurnStatusExport
        {
            ChuaXuatKho = 1,
            DaXuatKho = 2
        }

        /// <summary>
        /// Các loại của lịch sử đơn hàng
        /// </summary>
        public enum TypeHistoryOrderChange
        {
            TienDatCoc = 1,
            PhiShipTQ = 2,
            PhiMuaSanPham = 3,
            PhiCanNang = 4,
            PhiKiemKe = 5,
            PhiDongGoi = 6,
            PhiGiaoTanNha = 7,
            MaVanDon = 8,
            CanNangDonHang = 9,
            MaDonHang = 10

            //TienDatCoc = 1,
            //PhiShipTQ = 2,
            //PhiMuaSanPham = 3,
            //PhiCanNang = 4,
            //PhiKiemKe = 5,
            //PhiDongGoi = 6,
            //PhiGiaoTanNha = 7,
            //MaVanDon = 8,
            //CanNangDonHang = 9,
            //MaDonHang = 10,
            //PhiBaoHiem = 11
        }

        /// <summary>
        /// Loại đơn hàng
        /// </summary>
        public enum TypeOrder
        {
            DonHangMuaHo = 1,
            DonKyGui = 2,
            KhongXacDinh = 3
        }

        /// <summary>
        /// Loại lịch sử thay đổi
        /// </summary>
        public enum TypeHistoryServices
        {
            VanChuyen = 1,
            ThanhToanHo = 2
        }
        #region SMS Template
        /// <summary>
        /// Xác nhận OTP SMS
        /// </summary>
        public const string SMS_XNOTP = "XNOTP";
        #endregion

    }
}
