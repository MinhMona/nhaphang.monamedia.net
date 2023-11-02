using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class PayOrderHistoryModel : AppDomainModel
    {
        public int? MainOrderId { get; set; }

        public int? UID { get; set; }

        public int? Status { get; set; }
        
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case (int)StatusPayOrderHistoryContants.DatCoc:
                        return "Đặt cọc";
                    case (int)StatusPayOrderHistoryContants.ThanhToan:
                        return "Thanh toán đơn hàng";
                    case (int)StatusPayOrderHistoryContants.HoanTienSanPham:
                        return "Sản phẩm hết hàng";
                    case (int)StatusPayOrderHistoryContants.HoanTienHuyDon:
                        return "Hoàn tiền đã trả";
                    default:
                        return String.Empty;
                }
            }
        }

        public decimal? Amount { get; set; }

        public int? Type { get; set; }

        public string TypeName
        {
            get
            {
                switch (Type)
                {
                    case (int)TypePayOrderHistoryContants.TrucTiep:
                        return "Trực tiếp";
                    case (int)TypePayOrderHistoryContants.ViDienTu:
                        return "Ví điện tử";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
