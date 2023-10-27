using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Models
{
    public class MainOrderModel : AppDomainModel
    {
        public bool? IsEditFeeBuyProPercent { get; set; }
        public decimal? EditedFeeBuyProPercent { get; set; }

        /// <summary>
        /// Ngày shop phát hàng
        /// </summary>
        public DateTime? DateSendGoods { get; set; }

        /// <summary>
        /// Ngày đang về VN
        /// </summary>
        public DateTime? DateComingVN { get; set; }

        #region Danh sách mã đơn hàng
        public List<MainOrderCodeModel> MainOrderCodes { get; set; }
        #endregion

        #region Tổng quan đơn hàng

        /// <summary>
        /// Đổi cân nặng trên View
        /// </summary>
        public bool? IsChangeTQVNWeight { get; set; }

        /// <summary>
        /// Đổi phí vận chuyển trên View
        /// </summary>
        public bool? IsChangeFeeWeight { get; set; }

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
        /// Tổng tiền (Còn lại)
        /// </summary>
        public decimal? TotalPriceVND { get; set; }

        /// <summary>
        /// Tổng tiền đơn hàng (Cộng với thằng FeeInWareHouse để ra tổng tiền)
        /// </summary>
        public decimal? TotalOrderAmount
        {
            get
            {
                return TotalPriceVND + FeeInWareHouse;
            }
        }

        /// <summary>
        /// Số tiền phải cọc
        /// </summary>
        public decimal? AmountDeposit { get; set; }

        /// <summary>
        /// Đã thanh toán (Đã trả)
        /// </summary>
        public decimal? Deposit { get; set; }

        /// <summary>
        /// Còn lại (công thức là TotalPriceVND + FeeInWareHouse - Deposit) (Số tiền phải thanh toán)
        /// </summary>
        public decimal? RemainingAmount
        {
            get
            {
                return TotalPriceVND + FeeInWareHouse - Deposit;
            }
        }

        /// <summary>
        /// ID Kho VN
        /// </summary>
        public int? ReceivePlace { get; set; }

        /// <summary>
        /// Kho VN
        /// </summary>
        public string ReceivePlaceName { get; set; }

        /// <summary>
        /// ID kho TQ
        /// </summary>
        public int? FromPlace { get; set; }

        /// <summary>
        /// Kho TQ
        /// </summary>
        public string FromPlaceName { get; set; }

        /// <summary>
        /// ID Phương thức vận chuyển
        /// </summary>
        public int? ShippingType { get; set; }

        /// <summary>
        /// Phương thức VC
        /// </summary>
        public string ShippingTypeName { get; set; }

        /// <summary>
        /// Kiểm đếm
        /// </summary>
        public bool? IsCheckProduct { get; set; }

        /// <summary>
        /// Đóng gỗ
        /// </summary>
        public bool? IsPacked { get; set; }

        /// <summary>
        /// Bảo hiểm
        /// </summary>
        public bool? IsInsurance { get; set; }

        /// <summary>
        /// Phí bảo hiểm (%)
        /// </summary>
        public decimal? InsurancePercent { get; set; }

        /// <summary>
        /// Giao hàng tận nhà
        /// </summary>
        public bool? IsFastDelivery { get; set; }
        #endregion

        #region Danh sách mã vận đơn
        public List<SmallPackageModel> SmallPackages { get; set; }

        /// <summary>
        /// Đủ mã vận đơn
        /// </summary>
        public bool? IsDoneSmallPackage { get; set; }
        #endregion

        #region Thông tin đơn hàng
        /// <summary>
        /// Tiền hàng VNĐ - Tiền hàng trên web (VNĐ)
        /// </summary>
        public decimal? PriceVND { get; set; }

        /// <summary>
        /// Tiền hàng trên web (tệ) - Tiền hàng CNY - Tiền tệ sản phẩm
        /// </summary>
        public decimal? PriceCNY { get; set; }

        /// <summary>
        /// Tỉ giá
        /// </summary>
        public decimal? CurrentCNYVN { get; set; }

        /// <summary>
        /// Phí mua hàng (VNĐ)
        /// </summary>
        public decimal? FeeBuyPro { get; set; }

        /// <summary>
        /// Phí mua hàng - chiết khấu (%)
        /// </summary>
        public decimal? CKFeeBuyPro { get; set; }

        /// <summary>
        /// Phí kiểm đếm (VNĐ)
        /// </summary>
        public decimal? IsCheckProductPrice { get; set; }

        /// <summary>
        /// Phí kiểm đếm (tệ)
        /// </summary>
        public decimal? IsCheckProductPriceCNY { get; set; }

        /// <summary>
        /// Phí đóng gỗ (VNĐ)
        /// </summary>
        public decimal? IsPackedPrice { get; set; }

        /// <summary>
        /// Phí đóng gỗ (tệ)
        /// </summary>
        public decimal? IsPackedPriceCNY { get; set; }

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        public decimal? InsuranceMoney { get; set; }

        /// <summary>
        /// Phí giao hàng tận nhà
        /// </summary>
        public decimal? IsFastDeliveryPrice { get; set; }

        /// <summary>
        /// Phụ phí (Tổng phụ phí)
        /// </summary>
        public decimal? Surcharge { get; set; }

        //Total giống thằng Tổng quan đơn hàng
        #endregion

        #region Thông tin người đặt hàng
        /// <summary>
        /// UserName người đặt hàng
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// UserName đặt hàng
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Số dư người đặt hàng
        /// </summary>
        public decimal Wallet { get; set; }

        /// <summary>
        /// Họ và tên người đặt hàng
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Địa chỉ người đặt hàng
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Email người đặt hàng
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Số điện thoại người đặt hàng
        /// </summary>
        public string Phone { get; set; }

        #endregion

        #region Thông tin người nhận hàng
        /// <summary>
        /// Họ và tên người nhận
        /// </summary>
        public string ReceiverFullName { get; set; }

        /// <summary>
        /// Số điện thoại người nhận
        /// </summary>
        public string ReceiverPhone { get; set; }

        /// <summary>
        /// Địa chỉ nhận hàng
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Email người nhận
        /// </summary>
        public string ReceiverEmail { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }
        #endregion

        #region Danh sách sản phẩm
        public List<OrderModel> Orders { get; set; }
        #endregion

        #region Lịch sử thanh toán
        public List<PayOrderHistoryModel> PayOrderHistories { get; set; }
        #endregion

        #region Lịch sử thay đổi
        public List<HistoryOrderChangeModel> HistoryOrderChanges { get; set; }
        #endregion

        #region Lịch sử khiếu nại
        public List<ComplainModel> Complains { get; set; }
        #endregion

        #region Danh sách phụ phí
        public List<FeeSupportModel> FeeSupports { get; set; }
        #endregion

        #region Nhắn tin với khách hàng, với nội bộ
        public List<OrderCommentModel> OrderComments { get; set; }
        #endregion

        #region Chi phí đơn hàng
        /// <summary>
        /// Tổng số tiền mua thật (VNĐ)
        /// </summary>
        public decimal? TotalPriceReal { get; set; }

        /// <summary>
        /// Tổng số tiền mua thật (Tệ)
        /// </summary>
        public decimal? TotalPriceRealCNY { get; set; }

        /// <summary>
        /// Phí ship TQ (Tệ)
        /// </summary>
        public decimal? FeeShipCNCNY { get; set; }

        /// <summary>
        /// Phí ship TQ (VNĐ)
        /// </summary>
        public decimal? FeeShipCN { get; set; }

        /// <summary>
        /// Phí ship TQ thật (tệ)
        /// </summary>
        public decimal? FeeShipCNRealCNY { get; set; }

        /// <summary>
        /// Phí ship TQ thật (VNĐ)
        /// </summary>
        public decimal? FeeShipCNReal { get; set; }

        /// <summary>
        /// Tiền hoa hồng (tệ)
        /// </summary>
        public decimal? HHCNY
        {
            get
            {
                return PriceCNY + FeeShipCNCNY - TotalPriceRealCNY - FeeShipCNRealCNY;
            }
        }

        /// <summary>
        /// Tiền hoa hồng (VNĐ)
        /// </summary>
        public decimal? HH
        {
            get
            {
                return HHCNY * CurrentCNYVN;
            }
        }

        /// <summary>
        /// Tổng cân nặng (kg)
        /// </summary>
        public decimal? OrderWeight { get; set; }

        /// <summary>
        /// Chiết khấu - Phí vận chuyển TQ - VN
        /// </summary>
        public decimal? FeeWeightCK { get; set; }

        /// <summary>
        /// Phần trăm chiết khẩu phí mua hàng
        /// </summary>
        public decimal? FeeBuyProCK { get; set; } = 0;

        /// <summary>
        /// Phí vận chuyển TQ - VN - Cân nặng
        /// </summary>
        public decimal? TQVNWeight { get; set; }

        /// <summary>
        /// TQ - VN - Cân khối
        /// </summary>
        public decimal? TQVNVolume { get; set; }
        /// <summary>
        /// Phí vận chuyển TQ - VN - VNĐ
        /// </summary>
        public decimal? FeeWeight { get; set; }
        #endregion

        /// <summary>
        /// Ảnh sản phẩm
        /// </summary>
        public string ImageOrigin { get; set; }

        /// <summary>
        /// ID Shop
        /// </summary>
        public string ShopId { get; set; }

        /// <summary>
        /// Tên shop
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// Website
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// ID nhân viên kinh doanh
        /// </summary>
        public int? SalerId { get; set; }

        /// <summary>
        /// Id nhân viên đặt hàng
        /// </summary>
        public int? DatHangId { get; set; }

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
        /// Ngày hủy
        /// </summary>
        public DateTime? CancelDate { get; set; }

        /// <summary>
        /// Ngày khiếu nại
        /// </summary>
        public DateTime? ComplainDate { get; set; }

        /// <summary>
        /// Ngày dự kiến
        /// </summary>
        public DateTime? ExpectedDate { get; set; }

        /// <summary>
        /// Tổng link
        /// </summary>
        public int TotalLink { get; set; }

        /// <summary>
        /// Tổng tiền hàng (Danh sách)
        /// </summary>
        public decimal? TotalAllPrice { get; set; }

        /// <summary>
        /// Tổng tiền hàng mua thật (Danh sách)
        /// </summary>
        public decimal? TotalAllPriceReal { get; set; }

        /// <summary>
        /// Tổng tiền hàng đã thanh toán (Danh sách)
        /// </summary>
        public decimal? TotalAllDeposit { get; set; }

        /// <summary>
        /// Tổng tiền còn lại - (Tổng tiền chưa thanh toán) (Danh sách)
        /// </summary>
        public decimal? TotalAllRemain
        {
            get
            {
                return TotalAllPrice - TotalAllDeposit;
            }
        }

        /// <summary>
        /// Tổng đơn mới
        /// </summary>
        public int? TotalStatus0 { get; set; }

        /// <summary>
        /// Tổng đơn đã đặt cọc
        /// </summary>
        public int? TotalStatus1 { get; set; }

        /// <summary>
        /// Tổng đơn đã mua hàng
        /// </summary>
        public int? TotalStatus2 { get; set; }

        /// <summary>
        /// Tổng đơn về kho TQ
        /// </summary>
        public int? TotalStatus5 { get; set; }

        /// <summary>
        /// Tổng đơn về kho VN
        /// </summary>
        public int? TotalStatus6 { get; set; }

        /// <summary>
        /// Tổng đơn đã thanh toán
        /// </summary>
        public int? TotalStatus7 { get; set; }

        /// <summary>
        /// Tổng đơn đã hoàn thành
        /// </summary>
        public int? TotalStatus9 { get; set; }

        /// <summary>
        /// Tổng đơn hủy
        /// </summary>
        public int? TotalStatus10 { get; set; }

        public string MainOrderTransactionCode { get; set; }

        /// <summary>
        /// Mã đơn hàng - Mã vận đơn
        /// </summary>
        public List<MainOrderTransactionCodeDetail> MainOrderTransactionCodeDetails
        {
            get
            {
                var list = new List<MainOrderTransactionCodeDetail>();
                if (string.IsNullOrEmpty(MainOrderTransactionCode))
                    return null;
                var code = MainOrderTransactionCode.Split(';');
                if (code.Length > 0)
                {
                    for (int i = 0; i < code.Length; i++)
                    {
                        var codeSpl = code[i].Split('_');
                        if (list.Any())
                        {
                            var listCheck = list.Where(x => x.MainOrderCode.Equals(codeSpl[0])).FirstOrDefault();
                            if (listCheck != null)
                            {
                                listCheck.OrderTransactionCode.Add(codeSpl[1]);
                                continue;
                            }
                        }
                        var listOTC = new List<string>();
                        listOTC.Add(codeSpl[1]);
                        list.Add(new MainOrderTransactionCodeDetail
                        {
                            MainOrderCode = codeSpl[0],
                            OrderTransactionCode = listOTC
                        });
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// Đặt cọc (%)
        /// </summary>
        public decimal? LessDeposit { get; set; }

        /// <summary>
        /// Phí dịch vụ (%)
        /// </summary>
        public decimal? FeeBuyProPT { get; set; }

        /// <summary>
        /// Phí mua hàng của User
        /// </summary>
        public decimal? FeeBuyProUser { get; set; }

        /// <summary>
        /// Đơn có bị khiếu nại
        /// </summary>
        public bool? IsComplain { get; set; }

        /// <summary>
        /// Đơn có cập nhật sản phẩm trong đơn hàng
        /// </summary>
        public bool? IsUpdatePrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? FeeShipCNToVN { get; set; }

        /// <summary>
        /// UserName nhân viên đặt hàng
        /// </summary>
        public string OrdererUserName { get; set; } = "";

        /// <summary>
        /// UserName Nhân viên kinh doanh
        /// </summary>
        public string SalerUserName { get; set; } = "";


        public List<string> OrderTransactionCodes { get; set; }
    }

    public class MainOrderTransactionCodeDetail
    {
        public string MainOrderCode { get; set; }
        public List<string> OrderTransactionCode { get; set; }
    }


    /// <summary>
    /// Danh sách sản phẩm (rút gọn) nhưng không xài (đang xài Order)
    /// </summary>
    public class ProductList
    {
        /// <summary>
        /// STT
        /// </summary>
        public int? Number { get; set; }

        /// <summary>
        /// Hình ảnh
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Đường link
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// Đơn giá
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Đơn giá (VNĐ)
        /// </summary>
        public decimal? PriceVND { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? ProductStatus { get; set; }
        public string ProcductStatusName
        {
            get
            {
                switch (ProductStatus)
                {
                    case 0:
                        return "Hết hàng";
                    case 1:
                        return "Còn hàng";
                    default:
                        return "Còn hàng";
                }
            }
        }
    }

    /// <summary>
    /// Danh sách mã vận đơn (rút gọn) nhưng không xài (SmallPackage)
    /// </summary>
    //public class BillOfLading
    //{
    //    /// <summary>
    //    /// Mã vận đơn
    //    /// </summary>
    //    public string OrderTransactionCode { get; set; }

    //    /// <summary>
    //    /// Cân nặng (kg)
    //    /// </summary>
    //    public decimal Weight { get; set; }

    //    /// <summary>
    //    /// Kích thước
    //    /// </summary>
    //    public string Size { get; set; }

    //    /// <summary>
    //    /// Cân quy đổi
    //    /// </summary>
    //    public decimal? WeigthQD { get; set; }

    //    /// <summary>
    //    /// Cân tính tiền
    //    /// </summary>
    //    public decimal? WeigthTT { get; set; }

    //    /// <summary>
    //    /// Ghi chú
    //    /// </summary>
    //    public string Description { get; set; }

    //    /// <summary>
    //    /// Trạng thái
    //    /// </summary>
    //    public int? Status { get; set; }

    //    /// <summary>
    //    /// Tên trạng thái
    //    /// </summary>
    //    public string StatusName
    //    {
    //        get
    //        {
    //            switch (Status)
    //            {
    //                case (int)StatusSmallPackage.DaHuy:
    //                    return "Đã hủy";
    //                case (int)StatusSmallPackage.MoiDat:
    //                    return "Mới đặt";
    //                case (int)StatusSmallPackage.DaVeKhoTQ:
    //                    return "Đã về kho TQ";
    //                case (int)StatusSmallPackage.DaVeKhoVN:
    //                    return "Đã về kho VN";
    //                case (int)StatusSmallPackage.DaThanhToan:
    //                    return "Đã thanh toán";
    //                case (int)StatusSmallPackage.DaGiao:
    //                    return "Đã giao";
    //                default:
    //                    return string.Empty;
    //            }
    //        }
    //    }
    //}

    public class TotalStatus
    {
        public int Status { get; set; }
        public int Total { get; set; }
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case -1: //Tất cả
                        return "Tất cả";
                    case (int)StatusOrderContants.DonMoi:
                        return "Đơn mới";
                    case (int)StatusOrderContants.DonHuy:
                        return "Đơn hàng hủy";
                    case (int)StatusOrderContants.DaCoc:
                        return "Đã đặt cọc";
                    case (int)StatusOrderContants.DaMuaHang:
                        return "Đã đặt hàng";
                    case (int)StatusOrderContants.VeTQ:
                        return "Hàng về kho TQ";
                    case (int)StatusOrderContants.VeVN:
                        return "Hàng về kho VN";
                    case (int)StatusOrderContants.DaThanhToan:
                        return "Đã thanh toán";
                    case (int)StatusOrderContants.HoanThanh:
                        return "Đã hoàn thành";
                    default:
                        return string.Empty;
                }
            }
        }
        public string TotalInfo
        {
            get
            {
                switch (Status)
                {
                    case -1: //Tất cả
                        return "Tổng số đơn hàng";
                    case (int)StatusOrderContants.DonMoi:
                        return "Đơn hàng chưa đặt cọc";
                    case (int)StatusOrderContants.DaCoc:
                        return "Đơn hàng đã đặt cọc";
                    case (int)StatusOrderContants.DaMuaHang:
                        return "Đơn hàng đã đặt hàng";
                    case (int)StatusOrderContants.VeTQ:
                        return "Đơn hàng đã có tại kho TQ";
                    case (int)StatusOrderContants.VeVN:
                        return "Đơn hàng đã có hàng tại VN";
                    case (int)StatusOrderContants.HoanThanh:
                        return "Đơn hàng đã nhận";
                    default:
                        return string.Empty;
                }
            }
        }
    }

    public class TotalAmount
    {
        public int Status { get; set; }
        public bool IsDeposit { get; set; } = false;
        public decimal? Amount { get; set; }
        public string AmountInfo
        {
            get
            {
                switch (Status)
                {
                    case -1: //Tất cả
                        return "Tổng tiền hàng chưa giao";
                    case (int)StatusOrderContants.DonMoi:
                        if (IsDeposit == true)
                            return "Tổng tiền hàng cần đặt cọc";
                        return "Tổng tiền hàng (đơn hàng cần đặt cọc)";
                    case (int)StatusOrderContants.DaMuaHang:
                        return "Tổng tiền hàng chờ về kho TQ";
                    case (int)StatusOrderContants.VeTQ:
                        return "Tổng tiền hàng đã về kho TQ";
                    case (int)StatusOrderContants.VeVN:
                        if (IsDeposit == true)
                            return "Tổng tiền cần thanh toán để lấy hàng trong kho";
                        return "Tổng tiền hàng đang ở kho đích";
                    default:
                        return string.Empty;
                }
            }
        }
    }

}
