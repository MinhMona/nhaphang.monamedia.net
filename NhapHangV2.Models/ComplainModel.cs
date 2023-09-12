using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class ComplainModel : AppDomainModel
    {
        /// <summary>
        /// UID
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// Mã shop
        /// </summary>
        public int? MainOrderId { get; set; }

        /// <summary>
        /// Tiền bồi thường
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        public string IMG { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string ComplainText { get; set; }

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
                    case (int)StatusComplain.DaHuy:
                        return "Đã hủy";
                    case (int)StatusComplain.MoiTao:
                        return "Mới tạo";
                    case (int)StatusComplain.DaXacNhan:
                        return "Đã xác nhận";
                    case (int)StatusComplain.DangXuLy:
                        return "Đang xử lý";
                    case (int)StatusComplain.HoanThanh:
                        return "Hoàn thành";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Tỉ giá theo Shop
        /// </summary>
        public decimal? CurrentCNYVN { get; set; }

        /// <summary>
        /// Id đơn ký gửi
        /// </summary>
        public int? TransportationOrderId { get; set; }
    }
}
