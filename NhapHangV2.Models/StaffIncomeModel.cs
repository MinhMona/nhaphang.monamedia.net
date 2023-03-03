using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class StaffIncomeModel : AppDomainModel
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int? MainOrderId { get; set; }

        /// <summary>
        /// Mã đơn hàng ký gửi
        /// </summary>
        public int? TransportationOrderId { get; set; }

        /// <summary>
        /// Mã đơn hàng thanh toán hộ
        /// </summary>
        public int? PayHelpOrderId { get; set; }

        public decimal? OrderTotalPrice { get; set; }

        /// <summary>
        /// Phần trăm
        /// </summary>
        public decimal? PercentReceive { get; set; }

        /// <summary>
        /// ID người dùng
        /// </summary>
        public int? UID { get; set; }

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
                    case (int)StatusStaffIncome.Unpaid:
                        return "Chưa thanh toán";
                    case (int)StatusStaffIncome.Paid:
                        return "Đã thanh toán";
                    default:
                        return String.Empty;
                }
            }
        }

        /// <summary>
        /// Hoa hồng (VNĐ)
        /// </summary>
        public decimal? TotalPriceReceive { get; set; }

        public DateTime? OrderCreatedDate { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Quyền hạn
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Tổng tiền đã thanh toán
        /// </summary>
        public decimal? MaxTotalPriceReceivePayment { get; set; }

        /// <summary>
        /// Tổng tiền chưa thanh toán
        /// </summary>
        public decimal? MaxTotalPriceReceiveNotPayment { get; set; }

        /// <summary>
        /// Ngày hoàn thành của đơn mua hộ
        /// </summary>
        public DateTime? MainOrderCompleteDate { get; set; }
    }
}
