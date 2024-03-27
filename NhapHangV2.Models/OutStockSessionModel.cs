using NhapHangV2.Models.Auth;
using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class OutStockSessionModel : AppDomainModel
    {
        /// <summary>
        /// UID người nhận
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// Là đơn mua hộ
        /// </summary>
        public int? IsOutStockOrder { get; set; } = 0;

        /// <summary>
        /// Là đơn ký gửi
        /// </summary>
        public int? IsOutStockTrans { get; set; } = 0;

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
                    case 1:
                        return "Chưa xử lý";
                    case 2:
                        return "Đã xử lý";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Tổng tiền phải trả (Tiền cần thanh toán)
        /// </summary>
        public decimal? TotalPay { get; set; }

        /// <summary>
        /// Tổng cân nặng
        /// </summary>
        public decimal? TotalWeight { get; set; }

        /// <summary>
        /// Tổng tiền lưu kho tất cả đơn
        /// </summary>
        public decimal? TotalWarehouseFee { get; set; } = 0;

        /// <summary>
        /// Trạng thái thanh toán
        /// 1. Thanh toán bằng ví điện tử
        /// 2. Thanh toán bằng tiền mặt
        /// </summary>
        public int? Type { get; set; } = 0;

        /// <summary>
        /// Tên trạng thái thanh toán
        /// </summary>
        public string TypeName
        {
            get
            {
                switch (Type)
                {
                    case 1:
                        return "Thanh toán bằng ví điện tử";
                    case 2:
                        return "Thanh toán bằng tiền mặt";
                    default:
                        return "Chưa thanh toán";
                }
            }
        }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Họ và tên người nhận
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// UserName người nhận
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Số điện thoại người nhận
        /// </summary>
        public string UserPhone { get; set; }

        /// <summary>
        /// Chi tiết phiên xuất kho
        /// </summary>
        public List<OutStockSessionPackageModel> OutStockSessionPackages { get; set; } = new List<OutStockSessionPackageModel>();

        /// <summary>
        /// Cân nặng quy đổi
        /// </summary>
        public decimal? ExchangeWeight { get; set; }

        /// <summary>
        /// Cân nặng trả tiền
        /// </summary>
        public decimal? PayableWeight { get; set; }


        /// <summary>
        /// Số dư tài khoản của khách
        /// </summary>
        public decimal? UserWallet { get; set; }
    }
}
