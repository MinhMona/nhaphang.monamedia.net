﻿using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models.Report
{
    public class PayOrderHistoryReportModel : AppDomainReportModel
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int MainOrderId { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Loại thanh toán
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Tên loại thanh toán
        /// </summary>
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case (int)StatusPayOrderHistoryContants.DatCoc2:
                    case (int)StatusPayOrderHistoryContants.DatCoc3:
                        return "Đặt cọc";
                    case (int)StatusPayOrderHistoryContants.ThanhToan:
                        return "Thanh toán đơn hàng";
                    case (int)StatusPayOrderHistoryContants.SanPhamHetHang:
                        return "Sản phẩm hết hàng";
                    default:
                        return String.Empty;
                }
                
            }
        }

        /// <summary>
        /// Số tiền
        /// </summary>
        public decimal Amount { get; set; }
    }
}
