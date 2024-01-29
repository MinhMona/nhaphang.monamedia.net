
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class Configurations : DomainEntities.AppDomain
    {
        public bool? IsAppAccepted { get; set; } = false;

        #region Cấu hình chung
        /// <summary>
        /// Tên Url
        /// </summary>
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Tên website
        /// </summary>
        public string WebsiteName { get; set; } = string.Empty;

        /// <summary>
        /// Hình Logo
        /// </summary>
        public string LogoIMG { get; set; } = string.Empty;

        /// <summary>
        /// Tên công ty viết tắt
        /// </summary>
        public string CompanyShortName { get; set; } = string.Empty;

        /// <summary>
        /// Tên công ty dài
        /// </summary>
        public string CompanyLongName { get; set; } = string.Empty;

        /// <summary>
        /// Tên công ty ngắn
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Mã số thuế
        /// </summary>
        public string TaxCode { get; set; } = string.Empty;

        /// <summary>
        /// Banner
        /// </summary>
        public string BannerIMG { get; set; } = string.Empty;

        /// <summary>
        /// Chữ trên banner
        /// </summary>
        public string BannerText { get; set; } = string.Empty;

        /// <summary>
        /// Link Extension (Chrome)
        /// </summary>
        public string ChromeExtensionLink { get; set; } = string.Empty;

        /// <summary>
        /// Link Extension (CocCoc)
        /// </summary>
        public string CocCocExtensionLink { get; set; } = string.Empty;

        /// <summary>
        /// Về chúng tôi
        /// </summary>
        public string AboutText { get; set; } = string.Empty;

        /// <summary>
        /// Email hỗ trợ
        /// </summary>
        public string EmailSupport { get; set; } = string.Empty;

        /// <summary>
        /// Email liên lạc
        /// </summary>
        public string EmailContact { get; set; } = string.Empty;

        /// <summary>
        /// Hotline
        /// </summary>
        public string Hotline { get; set; } = string.Empty;

        /// <summary>
        /// Hotline hỗ trợ
        /// </summary>
        public string HotlineSupport { get; set; } = string.Empty;

        /// <summary>
        /// Hotline phản hồi
        /// </summary>
        public string HotlineFeedback { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ 1
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ 2 
        /// </summary>
        public string Address2 { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ 3
        /// </summary>
        public string Address3 { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian làm việc
        /// </summary>
        public string TimeWork { get; set; } = string.Empty;

        /// <summary>
        /// Ảnh đăng nhập, đăng ký
        /// </summary>
        public string BackgroundAuth { get; set; }
        #endregion

        #region Cấu hình tài khoản MXH

        /// <summary>
        /// Link facebook Fanpage
        /// </summary>
        public string FacebookFanpage { get; set; } = string.Empty;

        /// <summary>
        /// Link facebook
        /// </summary>
        public string Facebook { get; set; } = string.Empty;

        /// <summary>
        /// Link twitter
        /// </summary>
        public string Twitter { get; set; } = string.Empty;

        /// <summary>
        /// Link google
        /// </summary>
        public string GooglePlus { get; set; } = string.Empty;

        /// <summary>
        /// Link Instagram
        /// </summary>
        public string Instagram { get; set; } = string.Empty;

        /// <summary>
        /// Link Skype
        /// </summary>
        public string Skype { get; set; } = string.Empty;

        /// <summary>
        /// Link Pinterest
        /// </summary>
        public string Pinterest { get; set; } = string.Empty;

        /// <summary>
        /// Link LinkedIn
        /// </summary>
        public string LinkedIn { get; set; } = string.Empty;

        /// <summary>
        /// Link Youtube
        /// </summary>
        public string Youtube { get; set; } = string.Empty;

        /// <summary>
        /// Link Zalo
        /// </summary>
        public string ZaloLink { get; set; } = string.Empty;

        /// <summary>
        /// Link WeChat
        /// </summary>
        public string WechatLink { get; set; } = string.Empty;

        /// <summary>
        /// Link Google Map
        /// </summary>
        public string GoogleMapLink { get; set; } = string.Empty;
        /// <summary>
        /// Số ngày tự động xóa giỏ hàng
        /// </summary>
        public int RemoveCartDay { get; set; }
        #endregion

        #region Cấu hình tỉ giá và hoa hồng
        /// <summary>
        /// Tỷ giá mua hàng hộ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Currency { get; set; } = 0;

        /// <summary>
        /// Tỉ giá thanh toán hộ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? PricePayHelpDefault { get; set; } = 0;

        /// <summary>
        /// Tỉ giá ký gửi
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? AgentCurrency { get; set; } = 0;

        /// <summary>
        /// Phần trăm nhân viên sale trong 3 tháng đầu
        /// </summary>
        public int? SalePercent { get; set; } = 0;

        /// <summary>
        /// Phần trăm nhân viên sale sau 3 tháng
        /// </summary>
        public int? SalePercentAfter3Month { get; set; } = 0;

        /// <summary>
        /// Phần trăm nhân viên đặt hàng
        /// </summary>
        public int? DatHangPercent { get; set; } = 0;

        /// <summary>
        /// Phần trăm bảo hiểm đơn hàng mua hộ
        /// </summary>
        public int? InsurancePercent { get; set; } = 0;

        /// <summary>
        /// Phần trăm bảo hiểm đơn hàng ký gửi
        /// </summary>
        public int? InsurancePercentTransport { get; set; } = 0;

        /// <summary>
        /// Số lượng link trong 1 đơn
        /// </summary>
        public int? NumberLinkOfOrder { get; set; } = 0;

        /// <summary>
        /// Phần trăm hồng sale đơn vận chuyển hộ
        /// </summary>
        public int? SaleTranportationPersent { get; set; } = 0;

        /// <summary>
        /// Phần trăm hoa hồng sale đơn thanh toán hộ
        /// </summary>
        public int? SalePayHelpPersent { get; set; } = 0;

        /// <summary>
        /// Phí mua hàng tối thiểu
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FeeBuyProMin { get; set; } = 0;
        #endregion

        #region Cấu hình thông báo
        /// <summary>
        /// Nội dung thông báo Popup
        /// </summary>
        public string NotiPopup { get; set; } = string.Empty;

        /// <summary>
        /// Tiêu đề thông báo Popup
        /// </summary>
        public string NotiPopupTitle { get; set; } = string.Empty;

        /// <summary>
        /// Email liên hệ Popup
        /// </summary>
        public string NotiPopupEmail { get; set; } = string.Empty;

        /// <summary>
        /// Thông báo chạy giao diện user
        /// </summary>
        public string NotiRun { get; set; } = string.Empty;
        #endregion

        #region Cấu hình Footer
        public string FooterLeft { get; set; } = string.Empty;
        public string FooterRight { get; set; } = string.Empty;
        #endregion

        #region Cấu hình SEO trang chủ và mặc định
        /// <summary>
        /// Title
        /// </summary>
        public string MetaTitle { get; set; } = string.Empty;

        /// <summary>
        /// Meta keyword
        /// </summary>
        public string MetaKeyword { get; set; } = string.Empty;

        /// <summary>
        /// Meta description
        /// </summary>
        public string MetaDescription { get; set; } = string.Empty;

        /// <summary>
        /// OG Url
        /// </summary>
        public string OGUrl { get; set; } = string.Empty;

        /// <summary>
        /// OG Title
        /// </summary>
        public string OGTitle { get; set; } = string.Empty;

        /// <summary>
        /// OG Description
        /// </summary>
        public string OGDescription { get; set; } = string.Empty;

        /// <summary>
        /// OG Image
        /// </summary>
        public string OGImage { get; set; } = string.Empty;

        /// <summary>
        /// OG FB Title
        /// </summary>
        public string OGFBTitle { get; set; } = string.Empty;

        /// <summary>
        /// OG FB Description
        /// </summary>
        public string OGFBDescription { get; set; } = string.Empty;

        /// <summary>
        /// OG FB Image
        /// </summary>
        public string OGFBImage { get; set; } = string.Empty;

        /// <summary>
        /// OG Twitter Title
        /// </summary>
        public string OGTwitterTitle { get; set; } = string.Empty;

        /// <summary>
        /// OG Twitter Description
        /// </summary>
        public string OGTwitterDescription { get; set; } = string.Empty;

        /// <summary>
        /// OG Twitter Image
        /// </summary>
        public string OGTwitterImage { get; set; } = string.Empty;

        /// <summary>
        /// Google Analytics (Đặt nội dung trong thẻ script)
        /// </summary>
        public string GoogleAnalytics { get; set; } = string.Empty;

        /// <summary>
        /// Webmaster Tools (Đặt nội dung trong thẻ script)
        /// </summary>
        public string WebmasterTools { get; set; } = string.Empty;

        /// <summary>
        /// Header Script Code (Đặt nội dung trong thẻ script)
        /// </summary>
        public string HeaderScriptCode { get; set; } = string.Empty;

        /// <summary>
        /// Footer Script Code (Đặt nội dung trong thẻ script)
        /// </summary>
        public string FooterScriptCode { get; set; } = string.Empty;

        /// <summary>
        /// application-name
        /// </summary>
        public string ApplicationName { get; set; } = string.Empty;

        /// <summary>
        /// google-site-verification
        /// </summary>
        public string GoogleSiteVerification { get; set; } = string.Empty;

        /// <summary>
        /// reply-to
        /// </summary>
        public string ReplyTo { get; set; } = string.Empty;

        /// <summary>
        /// robots
        /// </summary>
        public string Robots { get; set; } = string.Empty;

        /// <summary>
        /// og:locale
        /// </summary>
        public string OGLocale { get; set; } = string.Empty;

        /// <summary>
        /// og:site_name
        /// </summary>
        public string OGSiteName { get; set; } = string.Empty;

        /// <summary>
        /// og:type
        /// </summary>
        public string OGType { get; set; } = string.Empty;

        #endregion

        #region Cấu hình OneSignal
        /// <summary>
        /// OneSignal App ID
        /// </summary>
        public string OneSignalAppID { get; set; } = string.Empty;

        /// <summary>
        /// Rest API Key
        /// </summary>
        public string RestAPIKey { get; set; } = string.Empty;
        #endregion

        public string InfoContent { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? WeightPrice { get; set; } = 0;

        public int? PercentOrder { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceSendDefaultHN { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceSendDefaultSG { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? CurrencyIncome { get; set; } = 0;
        public int? ChietKhauPercent { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal? PriceCheckOutWareDefault { get; set; } = 0;
    }
}
