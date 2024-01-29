using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Models
{
    public class ConfigurationsModel : AppDomainModel
    {
        public bool? IsAppAccepted { get; set; }

        #region Cấu hình chung
        /// <summary>
        /// Tên Url
        /// </summary>
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Tên website
        /// </summary>
        public string WebsiteName { get; set; }

        /// <summary>
        /// Hình Logo
        /// </summary>
        public string LogoIMG { get; set; }

        /// <summary>
        /// Tên công ty viết tắt
        /// </summary>
        public string CompanyShortName { get; set; }

        /// <summary>
        /// Tên công ty dài
        /// </summary>
        public string CompanyLongName { get; set; }

        /// <summary>
        /// Tên công ty ngắn
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Mã số thuế
        /// </summary>
        public string TaxCode { get; set; }

        /// <summary>
        /// Banner
        /// </summary>
        public string BannerIMG { get; set; }

        /// <summary>
        /// Chữ trên banner
        /// </summary>
        public string BannerText { get; set; }

        /// <summary>
        /// Link Extension (Chrome)
        /// </summary>
        public string ChromeExtensionLink { get; set; }

        /// <summary>
        /// Link Extension (CocCoc)
        /// </summary>
        public string CocCocExtensionLink { get; set; }

        /// <summary>
        /// Về chúng tôi
        /// </summary>
        public string AboutText { get; set; }

        /// <summary>
        /// Email hỗ trợ
        /// </summary>
        public string EmailSupport { get; set; }

        /// <summary>
        /// Email liên lạc
        /// </summary>
        public string EmailContact { get; set; }

        /// <summary>
        /// Hotline
        /// </summary>
        public string Hotline { get; set; }

        /// <summary>
        /// Hotline hỗ trợ
        /// </summary>
        public string HotlineSupport { get; set; }

        /// <summary>
        /// Hotline phản hồi
        /// </summary>
        public string HotlineFeedback { get; set; }

        /// <summary>
        /// Địa chỉ 1
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Địa chỉ 2 
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        /// Địa chỉ 3
        /// </summary>
        public string Address3 { get; set; }

        /// <summary>
        /// Thời gian làm việc
        /// </summary>
        public string TimeWork { get; set; }

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
        public string Facebook { get; set; }

        /// <summary>
        /// Link twitter
        /// </summary>
        public string Twitter { get; set; }

        /// <summary>
        /// Link google
        /// </summary>
        public string GooglePlus { get; set; }

        /// <summary>
        /// Link Instagram
        /// </summary>
        public string Instagram { get; set; }

        /// <summary>
        /// Link Skype
        /// </summary>
        public string Skype { get; set; }

        /// <summary>
        /// Link Pinterest
        /// </summary>
        public string Pinterest { get; set; }

        /// <summary>
        /// Link LinkedIn
        /// </summary>
        public string LinkedIn { get; set; }

        /// <summary>
        /// Link Youtube
        /// </summary>
        public string Youtube { get; set; }

        /// <summary>
        /// Link Zalo
        /// </summary>
        public string ZaloLink { get; set; }

        /// <summary>
        /// Link WeChat
        /// </summary>
        public string WechatLink { get; set; }

        /// <summary>
        /// Link Google Map
        /// </summary>
        public string GoogleMapLink { get; set; }
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
        #region Cấu hình tỉ giá và hoa hồng
        /// <summary>
        /// Tỷ giá mua hàng hộ
        /// </summary>
        public decimal? Currency { get; set; }

        /// <summary>
        /// Tỉ giá thanh toán hộ
        /// </summary>
        public decimal? PricePayHelpDefault { get; set; }

        /// <summary>
        /// Tỉ giá ký gửi
        /// </summary>
        public decimal? AgentCurrency { get; set; }

        /// <summary>
        /// Phần trăm nhân viên sale trong 3 tháng đầu
        /// </summary>
        public int? SalePercent { get; set; }

        /// <summary>
        /// Phần trăm nhân viên sale sau 3 tháng
        /// </summary>
        public int? SalePercentAfter3Month { get; set; }

        /// <summary>
        /// Phần trăm nhân viên đặt hàng
        /// </summary>
        public int? DatHangPercent { get; set; }

        /// <summary>
        /// Phần trăm bảo hiểm đơn hàng mua hộ
        /// </summary>
        public int? InsurancePercent { get; set; }

        /// <summary>
        /// Phần trăm bảo hiểm đơn hàng ký gửi
        /// </summary>
        public int? InsurancePercentTransport { get; set; }

        /// <summary>
        /// Số lượng link trong 1 đơn
        /// </summary>
        public int? NumberLinkOfOrder { get; set; }

        /// <summary>
        /// Phần trăm hoa hồng sale đơn vận chuyển hộ
        /// </summary>
        public int? SaleTranportationPersent { get; set; }

        /// <summary>
        /// Phần trăm hoa hồng sale đơn thanh toán hộ
        /// </summary>
        public int? SalePayHelpPersent { get; set; }

        /// <summary>
        /// Phí mua hàng tối thiểu
        /// </summary>
        public decimal? FeeBuyProMin { get; set; }
        #endregion

        #region Cấu hình thông báo
        /// <summary>
        /// Nội dung thông báo Popup
        /// </summary>
        public string NotiPopup { get; set; }

        /// <summary>
        /// Tiêu đề thông báo Popup
        /// </summary>
        public string NotiPopupTitle { get; set; }

        /// <summary>
        /// Email liên hệ Popup
        /// </summary>
        public string NotiPopupEmail { get; set; }

        /// <summary>
        /// Thông báo chạy giao diện user
        /// </summary>
        public string NotiRun  { get; set; } = string.Empty;
        #endregion

        #region Cấu hình Footer
        public string FooterLeft { get; set; }
        public string FooterRight { get; set; }
        #endregion

        #region Cấu hình SEO trang chủ và mặc định
        /// <summary>
        /// Title
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Meta keyword
        /// </summary>
        public string MetaKeyword { get; set; }

        /// <summary>
        /// Meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// OG Url
        /// </summary>
        public string OGUrl { get; set; }

        /// <summary>
        /// OG Title
        /// </summary>
        public string OGTitle { get; set; }

        /// <summary>
        /// OG Description
        /// </summary>
        public string OGDescription { get; set; }

        /// <summary>
        /// OG Image
        /// </summary>
        public string OGImage { get; set; }

        /// <summary>
        /// OG FB Title
        /// </summary>
        public string OGFBTitle { get; set; }

        /// <summary>
        /// OG FB Description
        /// </summary>
        public string OGFBDescription { get; set; }

        /// <summary>
        /// OG FB Image
        /// </summary>
        public string OGFBImage { get; set; }

        /// <summary>
        /// OG Twitter Title
        /// </summary>
        public string OGTwitterTitle { get; set; }

        /// <summary>
        /// OG Twitter Description
        /// </summary>
        public string OGTwitterDescription { get; set; }

        /// <summary>
        /// OG Twitter Image
        /// </summary>
        public string OGTwitterImage { get; set; }

        /// <summary>
        /// Google Analytics (Đặt nội dung trong thẻ script)
        /// </summary>
        public string GoogleAnalytics { get; set; }

        /// <summary>
        /// Webmaster Tools (Đặt nội dung trong thẻ script)
        /// </summary>
        public string WebmasterTools { get; set; }

        /// <summary>
        /// Header Script Code (Đặt nội dung trong thẻ script)
        /// </summary>
        public string HeaderScriptCode { get; set; }

        /// <summary>
        /// Footer Script Code (Đặt nội dung trong thẻ script)
        /// </summary>
        public string FooterScriptCode { get; set; }

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

        public string InfoContent { get; set; }
        public decimal? WeightPrice { get; set; }
        public int? PercentOrder { get; set; }
        public decimal? PriceSendDefaultHN { get; set; }
        public decimal? PriceSendDefaultSG { get; set; }
        public decimal? CurrencyIncome { get; set; }
        public int? ChietKhauPercent { get; set; }
        public decimal? PriceCheckOutWareDefault { get; set; }

        /// <summary>
        /// Số ngày tự động xóa giỏ hàng
        /// </summary>
        public int RemoveCartDay { get; set; }
    }
}
