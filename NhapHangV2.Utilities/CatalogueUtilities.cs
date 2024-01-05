using System;
using System.Collections.Generic;
using System.Text;

namespace NhapHangV2.Utilities
{
    public class CatalogueUtilities
    {

    }

    public class AmountStatistic
    {
        public decimal? TotalOrderPrice { get; set; }
        public decimal? TotalPaidPrice { get; set; }
    }

    public class MainOrdersInfor
    {
        /// <summary>
        /// Tổng số đơn hàng 
        /// </summary>
        public int TotalOrders { get; set; }
        /// <summary>
        /// Đơn hàng chưa đặt cọc
        /// </summary>
        public int TotalUnpaidOrders { get; set; }
        /// <summary>
        /// Đơn hàng đã đặt cọc
        /// </summary>
        public int ToTalPaidOrders { get; set; }
        /// <summary>
        /// Đơn hàng đã đặt hàng
        /// </summary>
        public int ToTalPlacedOrders { get; set; }
        /// <summary>
        /// Đơn hàng đã có tại kho TQ
        /// </summary>
        public int TotalInChinaOrders { get; set; }
        /// <summary>
        /// Đơn hàng đã có tại kho VN
        /// </summary>
        public int TotalInVietnamOrders { get; set; }
        /// <summary>
        /// Đơn hàng đã nhận
        /// </summary>
        public int TotalReceivedOrders { get; set; }
    }
    public class MainOrdersAmount
    {
        /// <summary>
        /// Tổng tiền hàng chưa giao
        /// </summary>
        public decimal AmountNotDelivery { get; set; }
        /// <summary>
        /// Tổng tiền hàng cần đặt cọc
        /// </summary>
        public decimal AmountMustDeposit { get; set; }
        /// <summary>
        /// Tổng tiền hàng (đơn hàng cần đặt cọc)
        /// </summary>
        public decimal AmountOrderRequireDeposit { get; set; }
        /// <summary>
        /// Tổng tiền hàng chờ về kho TQ
        /// </summary>
        public decimal AmoutWattingToChina { get; set; }
        /// <summary>
        /// Tổng tiền hàng đã về kho TQ
        /// </summary>
        public decimal AmountInChina { get; set; }
        /// <summary>
        /// Tổng tiền hàng đang ở kho đích
        /// </summary>
        public decimal AmountInVietnam { get; set; }
        /// <summary>
        /// Tổng tiền cần thanh toán để lấy hàng trong kho
        /// </summary>
        public decimal AmountPay { get; set; }
        /// <summary>
        /// Tổng tiền của những đơn đã cọc
        /// </summary>
        public decimal AmountOfDeposit { get; set; }
        /// <summary>
        /// Tổng tiền đơn đã hoàn thành
        /// </summary>
        public decimal AmountCompleted { get; set; }
    }

    public class BillInfor
    {
        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Địa chỉ người dùng
        /// </summary>
        public string UserAddress { get; set; }

        /// <summary>
        /// Tiền bằng chữ
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Tiền bằng chữ
        /// </summary>
        public string AmountString
        {
            get
            {
                return ConvertAmountToSentence.NumberToText(Amount);
            }
        }

        /// <summary>
        /// Lý do
        /// </summary>
        public string Note { get; set; }

    }

    /// <summary>
    /// Số lượng các đơn hàng theo trạng thái
    /// </summary>
    public class NumberOfOrders
    {
        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }
        ///// <summary>
        ///// Chưa đặt cọc
        ///// </summary>
        //public int? UnDeposit { get; set; } = 0;
        ///// <summary>
        ///// Hủy
        ///// </summary>
        //public int? Cancel { get; set; } = 0;
        ///// <summary>
        ///// Đã đặt cọc
        ///// </summary>
        //public int? Deposit { get; set; } = 0;
        ///// <summary>
        ///// Chờ duyệt đơn
        ///// </summary>
        //public int? WaitConfirm { get; set; } = 0;
        ///// <summary>
        ///// Đã duyệt đơn
        ///// </summary>
        //public int? Comfirmed { get; set; } = 0;
        ///// <summary>
        ///// Đã mua hàng
        ///// </summary>
        //public int? PurchaseOrder { get; set; } = 0;
        ///// <summary>
        ///// Đã về kho TQ
        ///// </summary>
        //public int? InChinaWarehoue { get; set; } = 0;
        ///// <summary>
        ///// Đã về kho VN
        ///// </summary>
        //public int? InVietnamWarehoue { get; set; } = 0;
        ///// <summary>
        ///// Chờ thanh toán
        ///// </summary>
        //public int? WaitPayment { get; set; } = 0;
        ///// <summary>
        ///// Khách đã thanh toán
        ///// </summary>
        //public int? Paid { get; set; } = 0;
        ///// <summary>
        ///// Đã hoàn thành
        ///// </summary>
        //public int? Completed { get; set; } = 0;
        ///// <summary>
        ///// Đã khiếu nại
        ///// </summary>
        //public int? Complained { get; set; } = 0;

        //public int? WaitingQuote { get; set; }
        ///// <summary>
        ///// Tất cả
        ///// </summary>
        //public int? AllOrders
        //{
        //    get
        //    {
        //        return (UnDeposit + Cancel + Deposit + WaitConfirm + Comfirmed + PurchaseOrder + InChinaWarehoue + InVietnamWarehoue + WaitPayment + Paid + Completed);
        //    }
        //}
    }

    public class TransportationsInfor
    {
        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }
        ///// <summary>
        ///// Tổng số đơn hàng 
        ///// </summary>
        //public int TotalOrders { get; set; }
        ///// <summary>
        ///// Đơn hàng mới
        ///// </summary>
        //public int TotalNewOrders { get; set; }
        ///// <summary>
        ///// Đơn hàng đã duyệt
        ///// </summary>
        //public int ToTalConfimed { get; set; }
        ///// <summary>
        ///// Đơn hàng đã có tại kho TQ
        ///// </summary>
        //public int TotalInChina { get; set; }
        ///// <summary>
        ///// Đơn hàng đã có tại kho VN
        ///// </summary>
        //public int TotalInVietnam { get; set; }
        ///// <summary>
        ///// Đơn hàng đã thanh toán
        ///// </summary>
        //public int TotalPaid { get; set; }
        ///// <summary>
        ///// Đơn hàng đã nhận
        ///// </summary>
        //public int TotalCompleted { get; set; }
        ///// <summary>
        ///// Đơn hàng đã hủy
        ///// </summary>
        //public int TotalCancled { get; set; }
    }
    public class TransportationsAmount
    {

        /// <summary>
        /// Tổng tiền hàng tất cả
        /// </summary>
        public decimal AmountAll { get; set; }
        /// <summary>
        /// Tổng tiền hàng chưa giao
        /// </summary>
        public decimal AmountNotDelivery { get; set; }
        /// <summary>
        /// Tổng tiền hàng chờ về kho TQ
        /// </summary>
        public decimal AmoutWattingToChina { get; set; }
        /// <summary>
        /// Tổng tiền hàng đã về kho TQ
        /// </summary>
        public decimal AmountInChina { get; set; }
        /// <summary>
        /// Tổng tiền hàng đang ở kho đích
        /// </summary>
        public decimal AmountInVietnam { get; set; }
        /// <summary>
        /// Tổng tiền cần thanh toán để lấy hàng trong kho
        /// </summary>
        public decimal AmountPay { get; set; }
        /// <summary>
        /// Tổng tiền hàng đã thanh toán
        /// </summary>
        public decimal AmountPaid { get; set; }
        /// <summary>
        /// Tổng tiền hàng đã hoàn thành
        /// </summary>
        public decimal AmountCompleted { get; set; }
    }

    public class CountStatusData
    {
        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }
    }
}
