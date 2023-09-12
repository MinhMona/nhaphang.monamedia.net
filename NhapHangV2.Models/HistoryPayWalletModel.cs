using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class HistoryPayWalletModel : AppDomainModel
    {
        /// <summary>
        /// UID
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// Id Shop
        /// </summary>
        public int? MainOrderId { get; set; }

        /// <summary>
        /// Số tiền
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Số dư
        /// </summary>
        public decimal? MoneyLeft { get; set; }

        /// <summary>
        /// 1 là trừ, 2 là cộng
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// Loại giao dịch
        /// </summary>
        public int? TradeType { get; set; }

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
                        return "Đặt cọc mua hộ";
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
                    case (int)HistoryPayWalletContents.HoanTienSanPham:
                        return "Hoàn tiền sản phẩm";
                    case (int)HistoryPayWalletContents.HoanTienDaTraMuaHo:
                        return "Hoàn tiền đã trả đơn mua hộ";
                    case (int)HistoryPayWalletContents.HoanTienDaTraKyGui:
                        return "Hoàn tiền đã trả đơn ký gửi";
                    case (int)HistoryPayWalletContents.HoaHongMuaHo:
                        return "Hoa hồng đơn mua hộ";
                    case (int)HistoryPayWalletContents.HoaHongKyGui:
                        return "Hoa hồng đơn ký gửi";
                    case (int)HistoryPayWalletContents.HoaHongThanhToanHo:
                        return "Hoa hồng đơn thanh toán hộ";
                    case (int)HistoryPayWalletContents.HuyRutTien:
                        return "Hủy lệnh rút tiền";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Tổng tiền đã nạp (User)
        /// </summary>
        public decimal TotalAmount4 { get; set; }

        /// <summary>
        /// Số dư hiện tại (User)
        /// </summary>
        public decimal Wallet { get; set; }
    }
}
