using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Repository;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.BackgroundServices;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.Services.Report;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service;
using NhapHangV2.Service.Repository;
using NhapHangV2.Service.Services;
using NhapHangV2.Service.Services.Auth;
using NhapHangV2.Service.Services.BackgroundServices;
using NhapHangV2.Service.Services.Catalogue;
using NhapHangV2.Service.Services.Configurations;
using NhapHangV2.Service.Services.Report;
using System;
using System.Globalization;
using System.IO;

namespace NhapHangV2.BaseAPI
{
    public static class ServiceExtensions
    {
        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services.AddScoped<IAppDbContext, NhapHangV2.AppDbContext.AppDbContext>();
            services.AddScoped(typeof(IDomainRepository<>), typeof(DomainRepository<>));
            services.AddScoped(typeof(ICatalogueRepository<>), typeof(CatalogueRepository<>));
            services.AddScoped(typeof(IAppRepository<>), typeof(AppRepository<>));
            services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();
        }

        public static void ConfigureService(this IServiceCollection services)
        {
            services.AddLocalization(o => { o.ResourcesPath = "Resources"; });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                CultureInfo[] supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("he")
                };

                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddTransient<ITokenManagerService, TokenManagerService>();

            #region Authenticate

            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPermitObjectPermissionService, PermitObjectPermissionService>();
            services.AddScoped<IPermitObjectService, PermitObjectService>();
            services.AddScoped<IUserGroupService, UserGroupService>();
            services.AddScoped<IUserInGroupService, UserInGroupService>();

            #endregion

            #region Catalogue

            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IBigPackageService, BigPackageService>();
            services.AddScoped<ICustomerBenefitsService, CustomerBenefitsService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<INotificationSettingService, NotificationSettingService>();
            services.AddScoped<IPageSEOService, PageSEOService>();
            services.AddScoped<IPageTypeService, PageTypeService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IShippingTypeToWareHouseService, ShippingTypeToWareHouseService>();
            services.AddScoped<IShippingTypeVNService, ShippingTypeVNService>();
            services.AddScoped<IStepService, StepService>();
            services.AddScoped<IWarehouseFromService, WarehouseFromService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<ICustomerTalkService, CustomerTalkService>();

            #endregion

            #region Configuration

            services.AddScoped<IEmailConfigurationService, EmailConfigurationService>();
            services.AddScoped<IOTPHistoryService, OTPHistoryService>();
            services.AddScoped<ISMSConfigurationService, SMSConfigurationService>();
            services.AddScoped<ISMSEmailTemplateService, SMSEmailTemplateService>();
            services.AddScoped<ISendNotificationService, SendNotificationService>();

            #endregion

            #region Report

            services.AddScoped<IAdminSendUserWalletReportService, AdminSendUserWalletReportService>();
            services.AddScoped<IHistoryPayWalletReportService, HistoryPayWalletReportService>();
            services.AddScoped<IMainOrderRealReportService, MainOrderRealReportService>();
            services.AddScoped<IMainOrderReportService, MainOrderReportService>();
            services.AddScoped<IMainOrderRevenueReportService, MainOrderRevenueReportService>();
            services.AddScoped<IOutStockSessionReportService, OutStockSessionReportService>();
            services.AddScoped<IPayHelpReportService, PayHelpReportService>();
            services.AddScoped<IPayOrderHistoryReportService, PayOrderHistoryReportService>();
            services.AddScoped<ITransportationOrderReportService, TransportationOrderReportService>();
            services.AddScoped<IUserReportService, UserReportService>();
            services.AddScoped<IWithdrawReportService, WithdrawReportService>();

            #endregion

            services.AddScoped<IAccountantOutStockPaymentService, AccountantOutStockPaymentService>();
            services.AddScoped<IAdminSendUserWalletService, AdminSendUserWalletService>();
            services.AddScoped<IComplainService, ComplainService>();
            services.AddScoped<IConfigurationsService, ConfigurationsService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IDeviceTokenService, DeviceTokenService>();
            services.AddScoped<IExportRequestTurnService, ExportRequestTurnService>();
            services.AddScoped<IFeeBuyProService, FeeBuyProService>();
            services.AddScoped<IFeeCheckProductService, FeeCheckProductService>();
            services.AddScoped<IFeePackagedService, FeePackagedService>();
            services.AddScoped<IFeeSupportService, FeeSupportService>();
            services.AddScoped<IHistoryOrderChangeService, HistoryOrderChangeService>();
            services.AddScoped<IHistoryPayWalletCNYService, HistoryPayWalletCNYService>();
            services.AddScoped<IHistoryPayWalletService, HistoryPayWalletService>();
            services.AddScoped<IHistoryScanPackageService, HistoryScanPackageService>();
            services.AddScoped<IHistoryServicesService, HistoryServicesService>();
            services.AddScoped<IInWareHousePriceService, InWareHousePriceService>();
            services.AddScoped<IMainOrderCodeService, MainOrderCodeService>();
            services.AddScoped<IMainOrderService, MainOrderService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
            services.AddScoped<IOrderCommentService, OrderCommentService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderShopTempService, OrderShopTempService>();
            services.AddScoped<IOrderTempService, OrderTempService>();
            services.AddScoped<IOutStockSessionPackageService, OutStockSessionPackageService>();
            services.AddScoped<IOutStockSessionService, OutStockSessionService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IPayAllOrderHistoryService, PayAllOrderHistoryService>();
            services.AddScoped<IPayHelpDetailService, PayHelpDetailService>();
            services.AddScoped<IPayHelpService, PayHelpService>();
            services.AddScoped<IPayOrderHistoryService, PayOrderHistoryService>();
            services.AddScoped<IPriceChangeService, PriceChangeService>();
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<IRequestOutStockService, RequestOutStockService>();
            services.AddScoped<ISmallPackageService, SmallPackageService>();
            services.AddScoped<IStaffIncomeService, StaffIncomeService>();
            services.AddScoped<ITransportationOrderService, TransportationOrderService>();
            services.AddScoped<IUserLevelService, UserLevelService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWarehouseFeeService, WarehouseFeeService>();
            services.AddScoped<IVolumeFeeService, VolumeFeeService>();
            services.AddScoped<IWithdrawService, WithdrawService>();
            services.AddScoped<IContactUsService, ContactUsService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<ICrawlProductService, CrawlProductService>();
            services.AddScoped<IDevLoginService, DevLoginService>();

            #region ToolConfig
            services.AddScoped<IToolConfigService, ToolConfigService>();
            #endregion
            services.AddScoped(typeof(IStoreSqlService<>), typeof(StoreSqlService<>));
            services.AddScoped<ISearchService, SearchService>();

        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "NhapHangV2 API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                   {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      new string[] { }
                    }
                  });

                var dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory));
                foreach (var fi in dir.EnumerateFiles("*.xml"))
                {
                    c.IncludeXmlComments(fi.FullName);
                }

                c.OrderActionsBy((apiDesc) => $"{apiDesc.RelativePath}");

                c.EnableAnnotations();
            });
        }

    }
}
