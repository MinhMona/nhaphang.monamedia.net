using System;
using System.Collections.Generic;
using System.Text;

namespace NhapHangV2.Utilities
{
    public class CoreContants
    {
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

        //Url thông báo user
        public const string Add_Product_Success = "/user/cart";
        public const string Complain_List = "/user/report";
        public const string Detail_MainOrder = "/user/order-list/detail?id={0}";
        public const string Detail_Payhelp = "/user/request-list/detail?id={0}";
        public const string Detail_Transportorder = "/user/deposit-list";
        public const string Transaction_History = "/user/history-transaction-vnd";
        public const string Widthdraw_History = "/user/withdrawal-vnd";
        public const string Recharge_History = "/user/recharge-vnd";
        //Url thông báo manager
        public const string New_Contact_Admin = "/manager/client/contact";
        public const string New_User_Admin = "/manager/client/client-list/detail?id={0}";
        public const string Complain_Admin = "/manager/order/complain-list";
        public const string Detail_MainOrder_Admin = "/manager/order/order-list/detail?id={0}";
        public const string Detail_Payhelp_Admin = "/manager/order/request-payment/detail?id={0}";
        public const string Detail_Transportorder_Admin = "/manager/deposit/deposit-list/detail?id={0}";
        public const string Add_Money_Admin = "/manager/money/recharge-history";
        public const string Sub_Money_Admin = "/manager/money/withdrawal-history";

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
            DatCoc = 1,
            NhanLaiTienDatCoc = 2,
            ThanhToanHoaDon = 3,
            AdminChuyenTien = 4,
            RutTien = 5,
            HuyLenhRutTien = 6,
            HoanTienKhieuNai = 7,
            ThanhToanVanChuyenHo = 8,
            ThanhToanHo = 9,
            HoaHong = 14,
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
            ChuaDatCoc = 0,
            Huy = 1,
            DaDatCoc = 2,
            ChoDuyetDon = 3,
            DaDuyetDon = 4,
            DaMuaHang = 5,
            DaVeKhoTQ = 6,
            DaVeKhoVN = 7,
            ChoThanhToan = 8,
            KhachDaThanhToan = 9,
            DaHoanThanh = 10,
            DaKhieuNai = 11,
            ChoBaoGia = 100 //Cho đơn mua hộ khác
            //DonMoi = 1,
            //DaCoc = 2,
            //DaMuaHang = 3,
            //ShopPhatHang = 4,
            //HangVeTQ = 5,
            //HangVeVN = 6,
            //DaThanhToan = 7,
            //HoanThanh = 8,
            //KhieuNai = 9,
            //Huy = 10,
        }

        /// <summary>
        /// Các trạng thái của lịch sử đơn hàng mua hộ
        /// </summary>
        public enum StatusPayOrderHistoryContants
        {
            DatCoc2 = 2,
            DatCoc3 = 3,
            HuyHoanTien = 4,
            ThanhToan = 9,
            SanPhamHetHang = 12
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
            Huy = 1,
            ChoDuyet = 2,
            DaDuyet = 3,
            VeKhoTQ = 4,
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
            ChuaThanhToan = 1,
            DaThanhToan = 2,
            DaHuy = 3,
            DaHoanThanh = 4,
            DaXacNhan = 5

            //ChoDuyet = 1,
            //DaDuyet = 2,
            //DaThanhToan = 3,
            //HoanThanh = 4,
            //Huy = 5
        }

        /// <summary>
        /// Trạng thái của kiện trôi nổi
        /// </summary>
        public enum StatusSmallPackage
        {
            DaHuy = 0,
            MoiDat = 1,
            DaVeKhoTQ = 2,
            DaVeKhoVN = 3,
            DaThanhToan = 4,
            DaGiao = 5
            //MoiDat = 1,
            //ShopPhatHang = 2,
            //DaVeKhoTQ = 3,
            //DaVeKhoVN = 4,
            //DaThanhToan = 5,
            //DaGiao = 6,
            //DaHuy = 7,

        }

        /// <summary>
        /// Trạng thái của kiện trôi nổi
        /// </summary>
        public enum StatusBigPackage
        {
            DangChuyenVe = 1,
            DaNhanHang = 2,
            Huy = 3,
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
            ChuaDuyet = 1,
            DangXuLy = 2,
            DaXuLy = 3
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

        #region Catalogue Name

        #endregion

        #region SMS Template
        /// <summary>
        /// Xác nhận OTP SMS
        /// </summary>
        public const string SMS_XNOTP = "XNOTP";
        #endregion

        #region Email Template
        #endregion
    }
}
