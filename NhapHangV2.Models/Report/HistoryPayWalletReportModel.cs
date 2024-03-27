using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models.Report
{
    public class HistoryPayWalletReportModel : AppDomainReportModel
    {
        /// <summary>
        /// 1: Trừ, 2: Cộng
        /// </summary>
        public int? Type { get; set; } = 0;
        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Số tiền
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Loại giao dịch
        /// </summary>
        public int TradeType { get; set; }

        /// <summary>
        /// Tên loại giao dịch
        /// </summary>
        public string TradeTypeName
        {
            get
            {
                switch (TradeType)
                {
                    case (int)HistoryPayWalletContents.NapTien:
                        return "Nạp tiền";
                    case (int)HistoryPayWalletContents.RutTien:
                        return "Rút tiền";
                    case (int)HistoryPayWalletContents.DatCocMuaHo:
                        return "Đặt cọc đơn mua hộ";
                    case (int)HistoryPayWalletContents.ThanhToanMuaHo:
                        return "Thanh toán đơn mua hộ";
                    case (int)HistoryPayWalletContents.ThanhToanKyGui:
                        return "Thanh toán đơn ký gửi";
                    case (int)HistoryPayWalletContents.ThanhToanThanhToanHo:
                        return "Thanh toán đơn thanh toán hộ";
                    case (int)HistoryPayWalletContents.HoanTienKhieuNaiMuaHo:
                        return "Hoàn tiền khiếu nại đơn mua hộ";
                    case (int)HistoryPayWalletContents.HoanTienKhieuNaiKyGui:
                        return "Hoàn tiền khiếu nại đơn ký gửi";
                    case (int)HistoryPayWalletContents.HoanTienThanhToanHo:
                        return "Hoàn tiền đơn thanh toán hộ";
                    case (int)HistoryPayWalletContents.HoanTienSanPham:
                        return "Hoàn tiền sản phẩm";
                    case (int)HistoryPayWalletContents.HoanTienDaTraMuaHo:
                        return "Hoàn tiền đã trả đơn mua hộ";
                    case (int)HistoryPayWalletContents.HoanTienDaTraKyGui:
                        return "Hoàn tiền đã trả đơn ký gửi";
                    case (int)HistoryPayWalletContents.HoaHongMuaHo:
                        return "Hoa hồng mua hộ";
                    case (int)HistoryPayWalletContents.HoaHongKyGui:
                        return "Hoa hồng ký gửi";
                    case (int)HistoryPayWalletContents.HoaHongThanhToanHo:
                        return "Hoa hồng thanh toán hộ";
                    case (int)HistoryPayWalletContents.HuyRutTien:
                        return "Hủy lệnh rút tiền";
                    case (int)HistoryPayWalletContents.ThanhToanXuatKho:
                        return "Thanh toán xuất kho";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Số dư
        /// </summary>
        public decimal MoneyLeft { get; set; }

        /// <summary>
        /// Tổng số tiền giao dịch
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Tổng tiền đặt cọc
        /// </summary>
        public decimal TotalDeposit { get; set; }

        /// <summary>
        /// Tổng tiền nhận lại đặt cọc
        /// </summary>
        public decimal TotalReciveDeposit { get; set; }

        /// <summary>
        /// Tổng tiền thanh toán hóa đơn
        /// </summary>
        public decimal TotalPaymentBill { get; set; }

        /// <summary>
        /// Tổng tiền admin nạp tiền
        /// </summary>
        public decimal TotalAdminSend { get; set; }

        /// <summary>
        /// Tổng rút tiền
        /// </summary>
        public decimal TotalWithDraw { get; set; }

        /// <summary>
        /// Tổng hủy rút tiền
        /// </summary>
        public decimal TotalCancelWithDraw { get; set; }

        /// <summary>
        /// Tổng nhận tiền khiếu nại
        /// </summary>
        public decimal TotalComplain { get; set; }

        /// <summary>
        /// Tổng tiền thanh toán vận chuyển hộ
        /// </summary>
        public decimal TotalPaymentTransport { get; set; }

        /// <summary>
        /// Tổng tiền thanh toán hộ
        /// </summary>
        public decimal TotalPaymentHo { get; set; }

        /// <summary>
        /// Tổng tiền thanh toán lưu kho
        /// </summary>
        public decimal TotalPaymentSaveWare { get; set; }

        /// <summary>
        /// Tổng tiền nhận lại vận chuyển hộ
        /// </summary>
        public decimal TotalRecivePaymentTransport { get; set; }
    }
}
