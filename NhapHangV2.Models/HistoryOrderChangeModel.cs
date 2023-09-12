using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class HistoryOrderChangeModel : AppDomainModel
    {
        public int? TransportationOrderId { get; set; }
        public int? MainOrderId { get; set; }

        public int? UID { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string HistoryContent { get; set; }

        /// <summary>
        /// Loại
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// Tên loại
        /// </summary>
        public string TypeName
        {
            get
            {
                switch (Type)
                {
                    case (int)TypeHistoryOrderChange.TienDatCoc:
                        return "Tiền đặt cọc";
                    case (int)TypeHistoryOrderChange.PhiShipTQ:
                        return "Phí ship TQ";
                    case (int)TypeHistoryOrderChange.PhiMuaSanPham:
                        return "Phí mua sản phẩm";
                    case (int)TypeHistoryOrderChange.PhiCanNang:
                        return "Phí cân nặng";
                    case (int)TypeHistoryOrderChange.PhiKiemKe:
                        return "Phí kiểm kê";
                    case (int)TypeHistoryOrderChange.PhiDongGoi:
                        return "Phí đóng gói";
                    case (int)TypeHistoryOrderChange.PhiGiaoTanNha:
                        return "Phí giao tận nhà";
                    case (int)TypeHistoryOrderChange.MaVanDon:
                        return "Mã vận đơn";
                    case (int)TypeHistoryOrderChange.CanNangDonHang:
                        return "Cân nặng đơn hàng";
                    case (int)TypeHistoryOrderChange.MaDonHang:
                        return "Mã đơn hàng";
                    default:
                        return String.Empty;
                }
            }
        }

        /// <summary>
        /// Quyền hạn
        /// </summary>
        public string UserGroupName { get; set; }
    }
}
