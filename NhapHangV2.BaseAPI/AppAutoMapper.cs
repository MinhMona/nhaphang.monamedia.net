using NhapHangV2.Entities;
using NhapHangV2.Models.Auth;
using NhapHangV2.Request;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Utilities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using NhapHangV2.Request.Auth;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.Auth;
using NhapHangV2.Request.Catalogue;
using NhapHangV2.Models.Report;
using NhapHangV2.Entities.Report;

namespace NhapHangV2.Models.AutoMapper
{
    public class AppAutoMapper : Profile
    {
        public AppAutoMapper()
        {
            CreateMap<AdminSendUserWalletModel, AdminSendUserWallet>().ReverseMap();
            CreateMap<AdminSendUserWalletRequest, AdminSendUserWallet>().ReverseMap();
            CreateMap<PagedList<AdminSendUserWalletModel>, PagedList<AdminSendUserWallet>>().ReverseMap();

            CreateMap<ComplainModel, Complain>().ReverseMap();
            CreateMap<ComplainRequest, Complain>().ReverseMap();
            CreateMap<PagedList<ComplainModel>, PagedList<Complain>>().ReverseMap();

            CreateMap<ConfigurationsModel, Configurations>().ReverseMap();
            CreateMap<ConfigurationsRequest, Configurations>().ReverseMap();
            CreateMap<PagedList<ConfigurationsModel>, PagedList<Configurations>>().ReverseMap();

            CreateMap<ExportRequestTurnModel, ExportRequestTurn>().ReverseMap();
            CreateMap<ExportRequestTurnRequest, ExportRequestTurn>().ReverseMap();
            CreateMap<PagedList<ExportRequestTurnModel>, PagedList<ExportRequestTurn>>().ReverseMap();

            CreateMap<FeeBuyProModel, FeeBuyPro>().ReverseMap();
            CreateMap<FeeBuyProRequest, FeeBuyPro>().ReverseMap();
            CreateMap<PagedList<FeeBuyProModel>, PagedList<FeeBuyPro>>().ReverseMap();

            CreateMap<FeeCheckProductModel, FeeCheckProduct>().ReverseMap();
            CreateMap<PagedList<FeeCheckProductModel>, PagedList<FeeCheckProduct>>().ReverseMap();
            CreateMap<FeeCheckProductRequest, FeeCheckProduct>().ReverseMap();

            CreateMap<FeePackagedModel, FeePackaged>().ReverseMap();
            CreateMap<PagedList<FeePackagedModel>, PagedList<FeePackaged>>().ReverseMap();
            CreateMap<FeePackagedRequest, FeePackaged>().ReverseMap();

            CreateMap<FeeSupportModel, FeeSupport>().ReverseMap();
            CreateMap<FeeSupportRequest, FeeSupport>().ReverseMap();
            CreateMap<PagedList<FeeSupportModel>, PagedList<FeeSupport>>().ReverseMap();

            CreateMap<HistoryOrderChangeModel, HistoryOrderChange>().ReverseMap();
            CreateMap<HistoryOrderChangeRequest, HistoryOrderChange>().ReverseMap();
            CreateMap<PagedList<HistoryOrderChangeModel>, PagedList<HistoryOrderChange>>().ReverseMap();

            CreateMap<HistoryPayWalletCNYModel, HistoryPayWalletCNY>().ReverseMap();
            CreateMap<HistoryPayWalletCNYRequest, HistoryPayWalletCNY>().ReverseMap();
            CreateMap<PagedList<HistoryPayWalletCNYModel>, PagedList<HistoryPayWalletCNY>>().ReverseMap();

            CreateMap<HistoryPayWalletModel, HistoryPayWallet>().ReverseMap();
            CreateMap<HistoryPayWalletRequest, HistoryPayWallet>().ReverseMap();
            CreateMap<PagedList<HistoryPayWalletModel>, PagedList<HistoryPayWallet>>().ReverseMap();

            CreateMap<HistoryServicesModel, HistoryServices>().ReverseMap();
            CreateMap<HistoryServicesRequest, HistoryServices>().ReverseMap();
            CreateMap<PagedList<HistoryServicesModel>, PagedList<HistoryServices>>().ReverseMap();

            CreateMap<MainOrderCodeModel, MainOrderCode>().ReverseMap();
            CreateMap<MainOrderCodeRequest, MainOrderCode>().ReverseMap();
            CreateMap<PagedList<MainOrderCodeModel>, PagedList<MainOrderCode>>().ReverseMap();

            CreateMap<MainOrderModel, MainOrder>().ReverseMap();
            CreateMap<MainOrderRequest, MainOrder>().ReverseMap();
            CreateMap<PagedList<MainOrderModel>, PagedList<MainOrder>>().ReverseMap();

            CreateMap<NotificationModel, Notification>().ReverseMap();
            CreateMap<PagedList<NotificationModel>, PagedList<Notification>>().ReverseMap();

            CreateMap<OrderCommentModel, OrderComment>().ReverseMap();
            CreateMap<OrderCommentRequest, OrderComment>().ReverseMap();
            CreateMap<PagedList<OrderCommentModel>, PagedList<OrderComment>>().ReverseMap();

            CreateMap<OrderModel, Order>().ReverseMap();
            CreateMap<OrderRequest, Order>().ReverseMap();
            CreateMap<PagedList<OrderModel>, PagedList<Order>>().ReverseMap();

            CreateMap<OrderShopTempModel, OrderShopTemp>().ReverseMap();

            //CreateMap<OrderShopTempRequest, OrderShopTemp>().ReverseMap();
            CreateMap<UpdateFieldOrderShopTempRequest, OrderShopTemp>().ReverseMap();
            CreateMap<PagedList<OrderShopTempModel>, PagedList<OrderShopTemp>>().ReverseMap();

            CreateMap<OrderTempModel, OrderTemp>().ReverseMap();
            CreateMap<OrderTempRequest, OrderTemp>().ReverseMap();
            CreateMap<PagedList<OrderTempModel>, PagedList<OrderTemp>>().ReverseMap();

            CreateMap<OutStockSessionModel, OutStockSession>().ReverseMap();
            CreateMap<PagedList<OutStockSessionModel>, PagedList<OutStockSession>>().ReverseMap();
            CreateMap<OutStockSessionRequest, OutStockSession>().ReverseMap();

            CreateMap<OutStockSessionPackageModel, OutStockSessionPackage>().ReverseMap();
            CreateMap<PagedList<OutStockSessionPackageModel>, PagedList<OutStockSessionPackage>>().ReverseMap();
            CreateMap<OutStockSessionPackageRequest, OutStockSessionPackage>().ReverseMap();

            CreateMap<PageModel, Page>().ReverseMap();
            CreateMap<PageRequest, Page>().ReverseMap();
            CreateMap<PagedList<PageModel>, PagedList<Page>>().ReverseMap();

            CreateMap<PayHelpDetailModel, PayHelpDetail>().ReverseMap();
            CreateMap<PayHelpDetailRequest, PayHelpDetail>().ReverseMap();
            CreateMap<PagedList<PayHelpDetailModel>, PagedList<PayHelpDetail>>().ReverseMap();

            CreateMap<PayHelpModel, PayHelp>().ReverseMap();
            CreateMap<PayHelpRequest, PayHelp>().ReverseMap();
            CreateMap<PagedList<PayHelpModel>, PagedList<PayHelp>>().ReverseMap();

            CreateMap<PayOrderHistoryModel, PayOrderHistory>().ReverseMap();
            //CreateMap<PayOrderHistoryRequest, PayOrderHistory>().ReverseMap();
            CreateMap<PagedList<PayOrderHistoryModel>, PagedList<PayOrderHistory>>().ReverseMap();

            CreateMap<PriceChangeModel, PriceChange>().ReverseMap();
            CreateMap<PriceChangeRequest, PriceChange>().ReverseMap();
            CreateMap<PagedList<PriceChangeModel>, PagedList<PriceChange>>().ReverseMap();

            CreateMap<RefundModel, Refund>().ReverseMap();
            CreateMap<RefundRequest, Refund>().ReverseMap();
            CreateMap<PagedList<RefundModel>, PagedList<Refund>>().ReverseMap();

            CreateMap<RequestOutStockModel, RequestOutStock>().ReverseMap();
            CreateMap<RequestOutStockRequest, RequestOutStock>().ReverseMap();
            CreateMap<PagedList<RequestOutStockModel>, PagedList<RequestOutStock>>().ReverseMap();

            CreateMap<SmallPackageModel, SmallPackage>().ReverseMap();
            CreateMap<SmallPackageRequest, SmallPackage>().ReverseMap();
            CreateMap<PagedList<SmallPackageModel>, PagedList<SmallPackage>>().ReverseMap();

            CreateMap<StaffIncomeModel, StaffIncome>().ReverseMap();
            CreateMap<StaffIncomeRequest, StaffIncome>().ReverseMap();
            CreateMap<PagedList<StaffIncomeModel>, PagedList<StaffIncome>>().ReverseMap();

            CreateMap<TransportationOrderModel, TransportationOrder>().ReverseMap();
            CreateMap<TransportationOrderRequest, TransportationOrder>().ReverseMap();
            CreateMap<PagedList<TransportationOrderModel>, PagedList<TransportationOrder>>().ReverseMap();

            CreateMap<UserLevelModel, UserLevel>().ReverseMap();
            CreateMap<UserLevelRequest, UserLevel>().ReverseMap();
            CreateMap<PagedList<UserLevelModel>, PagedList<UserLevel>>().ReverseMap();

            CreateMap<UserModel, Users>().ReverseMap();
            CreateMap<UserRequest, Users>().ReverseMap();
            CreateMap<PagedList<UserModel>, PagedList<Users>>().ReverseMap();

            CreateMap<WarehouseFeeModel, WarehouseFee>().ReverseMap();
            CreateMap<WarehouseFeeRequest, WarehouseFee>().ReverseMap();
            CreateMap<PagedList<WarehouseFeeModel>, PagedList<WarehouseFee>>().ReverseMap();

            CreateMap<VolumeFeeModel, VolumeFee>().ReverseMap();
            CreateMap<VolumeFeeRequest, VolumeFee>().ReverseMap();
            CreateMap<PagedList<VolumeFeeModel>, PagedList<VolumeFee>>().ReverseMap();

            CreateMap<WithdrawModel, Withdraw>().ReverseMap();
            CreateMap<WithdrawRequest, Withdraw>().ReverseMap();
            CreateMap<PagedList<WithdrawModel>, PagedList<Withdraw>>().ReverseMap();

            CreateMap<ContactUsModel, ContactUs>().ReverseMap();
            CreateMap<ContactUsRequest, ContactUs>().ReverseMap();
            CreateMap<PagedList<ContactUsModel>, PagedList<ContactUs>>().ReverseMap();

            #region Auth
            CreateMap<PermissionModel, Permissions>().ReverseMap();
            //CreateMap<PermissionRequest, Permissions>().ReverseMap();
            CreateMap<PagedList<PermissionModel>, PagedList<Permissions>>().ReverseMap();

            CreateMap<PermitObjectModel, PermitObjects>().ReverseMap();
            CreateMap<PermitObjectRequest, PermitObjects>().ReverseMap();
            CreateMap<PagedList<PermitObjectModel>, PagedList<PermitObjects>>().ReverseMap();

            CreateMap<PermitObjectPermissionModel, PermitObjectPermissions>().ReverseMap();
            CreateMap<PermitObjectPermissionRequest, PermitObjectPermissions>().ReverseMap();
            CreateMap<PagedList<PermitObjectPermissionModel>, PagedList<PermitObjectPermissions>>().ReverseMap();

            CreateMap<UserGroupForPermitObjectRequest, UserGroupForPermitObject>().ReverseMap();

            CreateMap<UserGroupModel, UserGroups>().ReverseMap();
            CreateMap<UserGroupRequest, UserGroups>().ReverseMap();
            CreateMap<PagedList<UserGroupModel>, PagedList<UserGroups>>().ReverseMap();

            CreateMap<UserInGroupModel, UserInGroups>().ReverseMap();
            CreateMap<PagedList<UserInGroupModel>, PagedList<UserInGroups>>().ReverseMap();
            #endregion

            #region Catalogue
            CreateMap<BankModel, Bank>().ReverseMap();
            CreateMap<BankRequest, Bank>().ReverseMap();
            CreateMap<PagedList<BankModel>, PagedList<Bank>>().ReverseMap();

            CreateMap<BigPackageModel, BigPackage>().ReverseMap();
            CreateMap<BigPackageRequest, BigPackage>().ReverseMap();
            CreateMap<PagedList<BigPackageModel>, PagedList<BigPackage>>().ReverseMap();

            CreateMap<CustomerBenefitsModel, CustomerBenefits>().ReverseMap();
            CreateMap<CustomerBenefitsRequest, CustomerBenefits>().ReverseMap();
            CreateMap<PagedList<CustomerBenefitsModel>, PagedList<CustomerBenefits>>().ReverseMap();

            CreateMap<MenuModel, Menu>().ReverseMap();
            CreateMap<MenuRequest, Menu>().ReverseMap();
            CreateMap<PagedList<MenuModel>, PagedList<Menu>>().ReverseMap();

            CreateMap<NotificationSettingModel, NotificationSetting>().ReverseMap();
            CreateMap<NotificationSettingRequest, NotificationSetting>().ReverseMap();
            CreateMap<PagedList<NotificationSettingModel>, PagedList<NotificationSetting>>().ReverseMap();

            CreateMap<PageTypeModel, PageType>().ReverseMap();
            CreateMap<PageTypeRequest, PageType>().ReverseMap();
            CreateMap<PagedList<PageTypeModel>, PagedList<PageType>>().ReverseMap();

            CreateMap<ServiceModel, Entities.Catalogue.Service>().ReverseMap();
            CreateMap<ServiceRequest, Entities.Catalogue.Service>().ReverseMap();
            CreateMap<PagedList<ServiceModel>, PagedList<Entities.Catalogue.Service>>().ReverseMap();

            CreateMap<ShippingTypeToWareHouseModel, ShippingTypeToWareHouse>().ReverseMap();
            CreateMap<ShippingTypeToWareHouseRequest, ShippingTypeToWareHouse>().ReverseMap();
            CreateMap<PagedList<ShippingTypeToWareHouseModel>, PagedList<ShippingTypeToWareHouse>>().ReverseMap();
            CreateMap<ShippingTypeVNModel, ShippingTypeVN>().ReverseMap();

            CreateMap<StepModel, Step>().ReverseMap();
            CreateMap<StepRequest, Step>().ReverseMap();
            CreateMap<PagedList<StepModel>, PagedList<Step>>().ReverseMap();

            CreateMap<WarehouseFromModel, WarehouseFrom>().ReverseMap();
            CreateMap<WarehouseFromRequest, WarehouseFrom>().ReverseMap();
            CreateMap<PagedList<WarehouseFromModel>, PagedList<WarehouseFrom>>().ReverseMap();

            CreateMap<WarehouseModel, Warehouse>().ReverseMap();
            CreateMap<WarehouseRequest, Warehouse>().ReverseMap();
            CreateMap<PagedList<WarehouseModel>, PagedList<Warehouse>>().ReverseMap();

            CreateMap<CustomerTalkModel, CustomerTalk>().ReverseMap();
            CreateMap<CustomerTalkRequest, CustomerTalk>().ReverseMap();
            CreateMap<PagedList<CustomerTalkModel>, PagedList<CustomerTalk>>().ReverseMap();
            #endregion

            #region Configuration

            #endregion

            #region Report
            CreateMap<AdminSendUserWalletReportModel, AdminSendUserWalletReport>().ReverseMap();
            CreateMap<PagedList<AdminSendUserWalletReportModel>, PagedList<AdminSendUserWalletReport>>().ReverseMap();

            CreateMap<HistoryPayWalletReportModel, HistoryPayWalletReport>().ReverseMap();
            CreateMap<PagedList<HistoryPayWalletReportModel>, PagedList<HistoryPayWalletReport>>().ReverseMap();

            CreateMap<MainOrderRealReportModel, MainOrderRealReport>().ReverseMap();
            CreateMap<PagedList<MainOrderRealReportModel>, PagedList<MainOrderRealReport>>().ReverseMap();

            CreateMap<MainOrderReportModel, MainOrderReport>().ReverseMap();
            CreateMap<PagedList<MainOrderReportModel>, PagedList<MainOrderReport>>().ReverseMap();

            CreateMap<MainOrderRevenueReportModel, MainOrderRevenueReport>().ReverseMap();
            CreateMap<PagedList<MainOrderRevenueReportModel>, PagedList<MainOrderRevenueReport>>().ReverseMap();

            CreateMap<OutStockSessionReportModel, OutStockSessionReport>().ReverseMap();
            CreateMap<PagedList<OutStockSessionReportModel>, PagedList<OutStockSessionReport>>().ReverseMap();

            CreateMap<PayHelpReportModel, PayHelpReport>().ReverseMap();
            CreateMap<PagedList<PayHelpReportModel>, PagedList<PayHelpReport>>().ReverseMap();

            CreateMap<PayOrderHistoryReportModel, PayOrderHistoryReport>().ReverseMap();
            CreateMap<PagedList<PayOrderHistoryReportModel>, PagedList<PayOrderHistoryReport>>().ReverseMap();

            CreateMap<TransportationOrderReportModel, TransportationOrderReport>().ReverseMap();
            CreateMap<PagedList<TransportationOrderReportModel>, PagedList<TransportationOrderReport>>().ReverseMap();

            CreateMap<UserReportModel, UserReport>().ReverseMap();
            CreateMap<PagedList<UserReportModel>, PagedList<UserReport>>().ReverseMap();

            CreateMap<WithdrawReportModel, WithdrawReport>().ReverseMap();
            CreateMap<PagedList<WithdrawReportModel>, PagedList<WithdrawReport>>().ReverseMap();
            #endregion

            #region
            CreateMap<ToolConfigModel, ToolConfig>().ReverseMap();
            CreateMap<ToolConfigRequest, ToolConfig>().ReverseMap();
            CreateMap<PagedList<ToolConfigModel>, PagedList<ToolConfig>>().ReverseMap();
            #endregion
            #region Extension

            CreateMap<OrderShopTempRequest, OrderTemp>()
                .ForMember(dst => dst.TitleOrigin, src => src.MapFrom(i => i.title_origin))
                .ForMember(dst => dst.PriceOrigin, src => src.MapFrom(i => i.price_origin))
                .ForMember(dst => dst.PricePromotion, src => src.MapFrom(i => i.price_promotion))
                .ForMember(dst => dst.Property, src => src.MapFrom(i => i.property))
                .ForMember(dst => dst.ShopId, src => src.MapFrom(i => i.shop_id))
                .ForMember(dst => dst.ShopName, src => src.MapFrom(i => i.shop_name))
                .ForMember(dst => dst.SellerId, src => src.MapFrom(i => i.seller_id))
                .ForMember(dst => dst.Wangwang, src => src.MapFrom(i => i.wangwang))
                .ForMember(dst => dst.Quantity, src => src.MapFrom(i => i.quantity))
                .ForMember(dst => dst.Stock, src => src.MapFrom(i => i.stock))
                .ForMember(dst => dst.ItemId, src => src.MapFrom(i => i.item_id))
                .ForMember(dst => dst.LinkOrigin, src => src.MapFrom(i => i.link_origin))
                .ForMember(dst => dst.Brand, src => src.MapFrom(i => i.brand))
                .ForMember(dst => dst.Site, src => src.MapFrom(i => i.site))
                .ForMember(dst => dst.ImageOrigin, src => src.MapFrom(i => i.image_origin))

                //.ForMember(dst => dst.PropertyTranslated, src => src.MapFrom(i => i.property_translated))
                //.ForMember(dst => dst.TitleTranslated, src => src.MapFrom(i => i.title_translated))
                //.ForMember(dst => dst.LocationSale, src => src.MapFrom(i => i.location_sale))
                //.ForMember(dst => dst.Weight, src => src.MapFrom(i => i.weight))
                //.ForMember(dst => dst.CategoryName, src => src.MapFrom(i => i.category_name))
                //.ForMember(dst => dst.CategoryId, src => src.MapFrom(i => i.category_id))


                //.ForMember(dst => dst.Tool, src => src.MapFrom(i => i.tool))
                //.ForMember(dst => dst.Version, src => src.MapFrom(i => i.version))
                //.ForMember(dst => dst.IsTranslate, src => src.MapFrom(i => i.is_translate))
                .ForMember(dst => dst.StepPrice, src => src.MapFrom(i => i.pricestep))
                .ReverseMap();

            CreateMap<OrderTempRequestNew, OrderTemp>()
                .ForMember(dst => dst.TitleOrigin, src => src.MapFrom(i => i.Title))
                .ForMember(dst => dst.PriceOrigin, src => src.MapFrom(i => i.PriceOrigin))
                .ForMember(dst => dst.PricePromotion, src => src.MapFrom(i => i.PricePromotion))
                .ForMember(dst => dst.Quantity, src => src.MapFrom(i => i.Quantity))
                .ForMember(dst => dst.Stock, src => src.MapFrom(i => i.Stock))
                .ForMember(dst => dst.Property, src => src.MapFrom(i => i.Properties))
                .ForMember(dst => dst.ImageOrigin, src => src.MapFrom(i => i.Image))
                .ReverseMap();

            CreateMap<OrderShopTempRequestNew, OrderShopTemp>()
                .ForMember(dst => dst.ShopId, src => src.MapFrom(i => i.ShopId))
                .ForMember(dst => dst.ShopName, src => src.MapFrom(i => i.ShopName))
                .ForMember(dst => dst.Site, src => src.MapFrom(i => i.Site))
                .ReverseMap();
            #endregion

            #region Add same OrderShopTemp from MainOrder
            CreateMap<Order, OrderTemp>().ForMember(x => x.Id, opt => opt.Ignore()).ReverseMap();
            #endregion
        }
    }
}
