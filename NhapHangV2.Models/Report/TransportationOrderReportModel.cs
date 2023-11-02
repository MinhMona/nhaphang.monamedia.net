using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models.Report
{
    public class TransportationOrderReportModel : AppDomainReportModel
    {
        public string UserName { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string OrderTransactionCode { get; set; }

        /// <summary>
        /// Cân nặng
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Kho TQ
        /// </summary>
        public string WareHouseFrom { get; set; }

        /// <summary>
        /// Kho VN
        /// </summary>
        public string WareHouseTo { get; set; }

        /// <summary>
        /// Phương thức vận chuyển
        /// </summary>
        public string ShippingTypeName { get; set; }

        /// <summary>
        /// Cước vật tư (VNĐ)
        /// </summary>
        public decimal? SensorFeeVND { get; set; }

        /// <summary>
        /// Phụ phí hàng đặc biệt (VNĐ)
        /// </summary>
        public decimal? AdditionFeeVND { get; set; }

        /// <summary>
        /// Tiền cân / Kg
        /// </summary>
        public decimal? FeeWeightPerKg { get; set; }

        /// <summary>
        /// Tổng tiền (VNĐ)
        /// </summary>
        public decimal? TotalPriceVND { get; set; }

        /// <summary>
        /// Ngày về TQ
        /// </summary>
        public DateTime? DateInTQWarehouse { get; set; }

        /// <summary>
        /// Ngày về VN
        /// </summary>
        public DateTime? DateInLasteWareHouse { get; set; }

        /// <summary>
        /// Ngày về YKXK
        /// </summary>
        public DateTime? DateExportRequest { get; set; }

        /// <summary>
        /// Ngày XK
        /// </summary>
        public DateTime? DateExport { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int Status { get; set; }

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
                    case (int)StatusGeneralTransportationOrder.DonMoi:
                        return "Đơn mới";
                    case (int)StatusGeneralTransportationOrder.VeKhoTQ:
                        return "Đã về kho TQ";
                    case (int)StatusGeneralTransportationOrder.DangVeVN:
                        return "Đang về VN";
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
        /// Tổng cân nặng
        /// </summary>
        public decimal MaxWeight { get; set; }

        /// <summary>
        /// Tổng tiền
        /// </summary>
        public decimal MaxTotalPriceVND { get; set; }
    }
}
