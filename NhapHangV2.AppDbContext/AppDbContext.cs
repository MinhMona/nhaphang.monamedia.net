using Microsoft.EntityFrameworkCore;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Auth;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.AppDbContext
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Catalogue
            modelBuilder.Entity<Bank>(x => x.ToTable("Bank"));
            modelBuilder.Entity<BigPackage>(x => x.ToTable("BigPackage"));
            modelBuilder.Entity<CustomerBenefits>(x => x.ToTable("CustomerBenefits"));
            modelBuilder.Entity<Menu>(x => x.ToTable("Menu"));
            modelBuilder.Entity<NotificationSetting>(x => x.ToTable("NotificationSetting"));
            modelBuilder.Entity<NotificationTemplate>(x => x.ToTable("NotificationTemplate"));
            modelBuilder.Entity<PageSEO>(x => x.ToTable("PageSEO"));
            modelBuilder.Entity<PageType>(x => x.ToTable("PageType"));
            modelBuilder.Entity<Service>(x => x.ToTable("Service"));
            modelBuilder.Entity<ShippingTypeToWareHouse>(x => x.ToTable("ShippingTypeToWareHouse"));
            modelBuilder.Entity<ShippingTypeVN>(x => x.ToTable("ShippingTypeVN"));
            modelBuilder.Entity<Step>(x => x.ToTable("Step"));
            modelBuilder.Entity<Warehouse>(x => x.ToTable("Warehouse"));
            modelBuilder.Entity<WarehouseFrom>(x => x.ToTable("WarehouseFrom"));
            #endregion

            #region Auth
            modelBuilder.Entity<Permissions>(x => x.ToTable("Permissions"));
            modelBuilder.Entity<PermitObjectPermissions>(x => x.ToTable("PermitObjectPermissions"));
            modelBuilder.Entity<PermitObjects>(x => x.ToTable("PermitObjects"));
            modelBuilder.Entity<UserGroups>(x => x.ToTable("UserGroups"));
            modelBuilder.Entity<UserInGroups>(x => x.ToTable("UserInGroups"));
            #endregion

            modelBuilder.Entity<AccountantOutStockPayment>(x => x.ToTable("AccountantOutStockPayment"));
            modelBuilder.Entity<AdminSendUserWallet>(x => x.ToTable("AdminSendUserWallet"));
            modelBuilder.Entity<Complain>(x => x.ToTable("Complain"));
            modelBuilder.Entity<Configurations>(x => x.ToTable("Configurations"));
            modelBuilder.Entity<DeviceToken>(x => x.ToTable("DeviceToken"));
            modelBuilder.Entity<ExportRequestTurn>(x => x.ToTable("ExportRequestTurn"));
            modelBuilder.Entity<FeeBuyPro>(x => x.ToTable("FeeBuyPro"));
            modelBuilder.Entity<FeeCheckProduct>(x => x.ToTable("FeeCheckProduct"));
            modelBuilder.Entity<FeePackaged>(x => x.ToTable("FeePackaged"));
            modelBuilder.Entity<FeeSupport>(x => x.ToTable("FeeSupport"));
            modelBuilder.Entity<HistoryOrderChange>(x => x.ToTable("HistoryOrderChange"));
            modelBuilder.Entity<HistoryPayWallet>(x => x.ToTable("HistoryPayWallet"));
            modelBuilder.Entity<HistoryPayWalletCNY>(x => x.ToTable("HistoryPayWalletCNY"));
            modelBuilder.Entity<HistoryScanPackage>(x => x.ToTable("HistoryScanPackage"));
            modelBuilder.Entity<HistoryServices>(x => x.ToTable("HistoryServices"));
            modelBuilder.Entity<InWareHousePrice>(x => x.ToTable("InWareHousePrice"));
            modelBuilder.Entity<MainOrder>(x => x.ToTable("MainOrder"));
            modelBuilder.Entity<MainOrderCode>(x => x.ToTable("MainOrderCode"));
            modelBuilder.Entity<Notification>(x => x.ToTable("Notification"));
            modelBuilder.Entity<Order>(x => x.ToTable("Order"));
            modelBuilder.Entity<OrderComment>(x => x.ToTable("OrderComment"));
            modelBuilder.Entity<OrderShopTemp>(x => x.ToTable("OrderShopTemp"));
            modelBuilder.Entity<OrderTemp>(x => x.ToTable("OrderTemp"));
            modelBuilder.Entity<OutStockSession>(x => x.ToTable("OutStockSession"));
            modelBuilder.Entity<OutStockSessionPackage>(x => x.ToTable("OutStockSessionPackage"));
            modelBuilder.Entity<Page>(x => x.ToTable("Page"));
            modelBuilder.Entity<PayAllOrderHistory>(x => x.ToTable("PayAllOrderHistory"));
            modelBuilder.Entity<PayHelp>(x => x.ToTable("PayHelp"));
            modelBuilder.Entity<PayHelpDetail>(x => x.ToTable("PayHelpDetail"));
            modelBuilder.Entity<PayOrderHistory>(x => x.ToTable("PayOrderHistory"));
            modelBuilder.Entity<PriceChange>(x => x.ToTable("PriceChange"));
            modelBuilder.Entity<Refund>(x => x.ToTable("Refund"));
            modelBuilder.Entity<RequestOutStock>(x => x.ToTable("RequestOutStock"));
            modelBuilder.Entity<SmallPackage>(x => x.ToTable("SmallPackage"));
            modelBuilder.Entity<StaffIncome>(x => x.ToTable("StaffIncome"));
            modelBuilder.Entity<TransportationOrder>(x => x.ToTable("TransportationOrder"));
            modelBuilder.Entity<UserLevel>(x => x.ToTable("UserLevel"));
            modelBuilder.Entity<Users>(x => x.ToTable("Users"));
            modelBuilder.Entity<WarehouseFee>(x => x.ToTable("WarehouseFee"));
            modelBuilder.Entity<VolumeFee>(x => x.ToTable("VolumeFee"));
            modelBuilder.Entity<Withdraw>(x => x.ToTable("Withdraw"));
            modelBuilder.Entity<CustomerTalk>(x => x.ToTable("CustomerTalk"));
            modelBuilder.Entity<ContactUs>(x => x.ToTable("ContactUs"));
            modelBuilder.Entity<ShipRequest>(x => x.ToTable("ShipRequest"));

            #region Configuration
            modelBuilder.Entity<EmailConfigurations>(x => x.ToTable("EmailConfigurations"));
            modelBuilder.Entity<OTPHistories>(x => x.ToTable("OTPHistories"));
            modelBuilder.Entity<SMSConfigurations>(x => x.ToTable("SMSConfigurations"));
            modelBuilder.Entity<SMSEmailTemplates>(x => x.ToTable("SMSEmailTemplates"));
            #endregion

            #region
            modelBuilder.Entity<ToolConfig>(x => x.ToTable("ToolConfig"));
            #endregion
            //Data seeding (tạo dữ liệu mẫu - ....Extension.ModelBuilderExtensions)
            //modelBuilder.Seed();

            base.OnModelCreating(modelBuilder);
        }

        #region Catalogue
        public DbSet<Bank> Bank { get; set; }
        public DbSet<BigPackage> BigPackage { get; set; }
        public DbSet<CustomerBenefits> CustomerBenefits { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<NotificationSetting> NotificationSetting { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplate { get; set; }
        public DbSet<PageSEO> PageSEO { get; set; }
        public DbSet<PageType> PageType { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<ShippingTypeToWareHouse> ShippingTypeToWareHouse { get; set; }
        public DbSet<ShippingTypeVN> ShippingTypeVN { get; set; }
        public DbSet<Step> Step { get; set; }
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<WarehouseFrom> WarehouseFrom { get; set; }
        #endregion

        #region Auth
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<PermitObjectPermissions> PermitObjectPermissions { get; set; }
        public DbSet<PermitObjects> PermitObjects { get; set; }
        public DbSet<UserGroups> UserGroups { get; set; }
        public DbSet<UserInGroups> UserInGroups { get; set; }
        #endregion

        public DbSet<AccountantOutStockPayment> AccountantOutStockPayment { get; set; }
        public DbSet<AdminSendUserWallet> AdminSendUserWallet { get; set; }
        public DbSet<Complain> Complain { get; set; }
        public DbSet<Configurations> Configurations { get; set; }
        public DbSet<DeviceToken> DeviceToken { get; set; }
        public DbSet<ExportRequestTurn> ExportRequestTurn { get; set; }
        public DbSet<FeeBuyPro> FeeBuyPro { get; set; }
        public DbSet<FeeCheckProduct> FeeCheckProduct { get; set; }
        public DbSet<FeePackaged> FeePackaged { get; set; }
        public DbSet<FeeSupport> FeeSupport { get; set; }
        public DbSet<HistoryOrderChange> HistoryOrderChange { get; set; }
        public DbSet<HistoryPayWallet> HistoryPayWallet { get; set; }
        public DbSet<HistoryPayWalletCNY> HistoryPayWalletCNY { get; set; }
        public DbSet<HistoryScanPackage> HistoryScanPackage { get; set; }
        public DbSet<HistoryServices> HistoryServices { get; set; }
        public DbSet<InWareHousePrice> InWareHousePrice { get; set; }
        public DbSet<MainOrder> MainOrder { get; set; }
        public DbSet<MainOrderCode> MainOrderCode { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderComment> OrderComment { get; set; }
        public DbSet<OrderShopTemp> OrderShopTemp { get; set; }
        public DbSet<OrderTemp> OrderTemp { get; set; }
        public DbSet<OutStockSession> OutStockSession { get; set; }
        public DbSet<OutStockSessionPackage> OutStockSessionPackage { get; set; }
        public DbSet<Page> Page { get; set; }
        public DbSet<PayAllOrderHistory> PayAllOrderHistory { get; set; }
        public DbSet<PayHelp> PayHelp { get; set; }
        public DbSet<PayHelpDetail> PayHelpDetail { get; set; }
        public DbSet<PayOrderHistory> PayOrderHistory { get; set; }
        public DbSet<PriceChange> PriceChange { get; set; }
        public DbSet<Refund> Refund { get; set; }
        public DbSet<RequestOutStock> RequestOutStock { get; set; }
        public DbSet<SmallPackage> SmallPackage { get; set; }
        public DbSet<StaffIncome> StaffIncome { get; set; }
        public DbSet<TransportationOrder> TransportationOrder { get; set; }
        public DbSet<UserLevel> UserLevel { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<WarehouseFee> WarehouseFee { get; set; }
        public DbSet<Withdraw> Withdraw { get; set; }

        public DbSet<ShipRequest> ShipRequest { get; set; }

        #region Configuration
        public DbSet<EmailConfigurations> EmailConfigurations { get; set; }
        public DbSet<OTPHistories> OTPHistories { get; set; }
        public DbSet<SMSConfigurations> SMSConfigurations { get; set; }
        public DbSet<SMSEmailTemplates> SMSEmailTemplates { get; set; }
        #endregion
    }
}
