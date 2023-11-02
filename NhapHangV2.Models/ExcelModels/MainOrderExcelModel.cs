using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models.ExcelModels
{
    public class MainOrderExcelModel : AppDomainModel
    {
        /// <summary>
        /// User đặt hàng
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Tổng tiền đơn hàng
        /// </summary>
        public decimal TotalPriceVND { get; set; } = 0;

        /// <summary>
        /// Tiền đã trả
        /// </summary>
        public decimal Deposit { get; set; } = 0;

        /// <summary>
        /// Tiền còn lại
        /// </summary>
        public decimal RemainingAmount
        {
            get
            {
                return TotalPriceVND - Deposit;
            }
        }

        /// <summary>
        /// Tiền hàng VNĐ - Tiền hàng trên web (VNĐ)
        /// </summary>
        public decimal? PriceVND { get; set; }

        /// <summary>
        /// Phí dịch vụ (%)
        /// </summary>
        public decimal? FeeBuyProPT { get; set; }

        /// <summary>
        /// Phí ship TQ (VNĐ)
        /// </summary>
        public decimal? FeeShipCN { get; set; }

        /// <summary>
        /// Phí kiểm đếm (VNĐ)
        /// </summary>
        public decimal? IsCheckProductPrice { get; set; }

        /// <summary>
        /// Phí đóng gỗ (VNĐ)
        /// </summary>
        public decimal? IsPackedPrice { get; set; }

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        public decimal? InsuranceMoney { get; set; }

        /// <summary>
        /// Phụ phí (Tổng phụ phí)
        /// </summary>
        public decimal? Surcharge { get; set; }

        /// <summary>
        /// Phí vận chuyển TQ - VN - VNĐ
        /// </summary>
        public decimal? FeeWeight { get; set; }

        /// <summary>
        /// Tổng cân nặng (kg)
        /// </summary>
        public decimal? OrderWeight { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Tên trạng thái đơn hàng
        /// </summary>
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case (int)StatusOrderContants.DonHuy:
                        return "Hủy";
                    case (int)StatusOrderContants.ChoBaoGia:
                        return "Chờ báo giá";
                    case (int)StatusOrderContants.DonMoi:
                        return "Đơn mới";
                    case (int)StatusOrderContants.DaCoc:
                        return "Đã đặt cọc";
                    case (int)StatusOrderContants.DaMuaHang:
                        return "Đã mua hàng";
                    case (int)StatusOrderContants.ShopPhatHang:
                        return "Shop phát hàng";
                    case (int)StatusOrderContants.VeTQ:
                        return "Đã về kho TQ";
                    case (int)StatusOrderContants.DangVeVN:
                        return "Đang về kho VN";
                    case (int)StatusOrderContants.VeVN:
                        return "Đã về kho VN";
                    case (int)StatusOrderContants.DaThanhToan:
                        return "Đã thanh toán";
                    case (int)StatusOrderContants.HoanThanh:
                        return "Đã hoàn thành";
                    case (int)StatusOrderContants.KhieuNai:
                        return "Đã khiếu nại";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Loại đơn hàng
        /// </summary>
        public int? OrderType { get; set; }

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
                        return "Đơn hàng mua hộ";
                    case (int)TypeOrder.DonKyGui:
                        return "Đơn hàng vận chuyển hộ";
                    case (int)TypeOrder.KhongXacDinh:
                        return "Đơn mua hộ khác";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Báo giá
        /// </summary>
        public bool? IsCheckNotiPrice { get; set; }

        /// <summary>
        /// Phí lưu kho
        /// </summary>
        public decimal? FeeInWareHouse { get; set; }

        /// <summary>
        /// ID nhân viên kinh doanh
        /// </summary>
        public int? SalerId { get; set; }

        /// <summary>
        /// Id nhân viên đặt hàng
        /// </summary>
        public int? DatHangId { get; set; }

        /// <summary>
        /// UserName nhân viên đặt hàng
        /// </summary>
        public string OrdererUserName { get; set; } = "";

        /// <summary>
        /// UserName Nhân viên kinh doanh
        /// </summary>
        public string SalerUserName { get; set; } = "";

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
        /// Ngày dự kiến
        /// </summary>
        public DateTime? ExpectedDate { get; set; }

    }
}
