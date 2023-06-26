using AutoMapper;
using BarcodeLib;
using Ganss.Excel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.ExcelMapper;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Request;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Utilities;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace NhapHangV2.Service.Services
{
    public class SmallPackageService : DomainService<SmallPackage, SmallPackageSearch>, ISmallPackageService
    {
        protected readonly IConfiguration configuration;
        protected readonly IAppDbContext Context;
        protected readonly IUserService userService;
        protected readonly IMainOrderService mainOrderService;
        protected readonly ITransportationOrderService transportationOrderService;
        protected readonly INotificationService notificationService;
        protected readonly ISendNotificationService sendNotificationService;
        protected readonly INotificationSettingService notificationSettingService;
        protected readonly INotificationTemplateService notificationTemplateService;

        public SmallPackageService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context, IConfiguration configuration) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            userService = serviceProvider.GetRequiredService<IUserService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            transportationOrderService = serviceProvider.GetRequiredService<ITransportationOrderService>();
            this.configuration = configuration;
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            notificationTemplateService = serviceProvider.GetRequiredService<INotificationTemplateService>();
            notificationService = serviceProvider.GetRequiredService<INotificationService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
        }

        protected override string GetStoreProcName()
        {
            return "SmallPackage_GetPagingData";
        }

        /// <summary>
        /// Xuất kho dựa vào mã barcode (mã vận đơn)
        /// </summary>
        /// <param name="UID">UID người dùng</param>
        /// <param name="BarCode">Mã vận đơn</param>
        /// <param name="StatusType">
        /// 1: Xuất kho ký gửi chưa yêu cầu
        /// 2: Xuất kho ký gửi đã yêu cầu
        /// 3: Xuất kho (UID = 0)
        /// </param>
        /// <returns></returns>
        public async Task<List<SmallPackage>> ExportForBarCode(int UID, string BarCode, int StatusType)
        {
            var smallPackages = new List<SmallPackage>();
            switch (StatusType)
            {
                case 1:
                case 2:
                    smallPackages = await BarCodeFor12(BarCode, UID, StatusType);
                    break;
                case 3:
                    smallPackages = await BarCodeFor3(BarCode, UID);
                    break;
                default:
                    break;
            }
            return smallPackages;
        }

        /// <summary>
        /// Xuất kho dựa vào UserName
        /// </summary>
        /// <param name="UID">UID người dùng</param>
        /// <param name="OrderID">(Xuất kho) Mã đơn hàng </param>
        /// <param name="StatusType">
        /// 1: Xuất kho ký gửi chưa yêu cầu
        /// 2: Xuất kho ký gửi đã yêu cầu
        /// 3: Xuất kho (UID = 0)
        /// </param>
        /// <param name="OrderType">(Xuất kho)
        /// 0: Tất cả
        /// 1: ĐH mua hộ
        /// 2: ĐH vận chuyển hộ (Ký gửi)
        /// </param>
        /// <returns></returns>
        public async Task<List<SmallPackage>> ExportForUserName(int UID, int OrderID, int StatusType, int OrderType)
        {
            var smallPackages = new List<SmallPackage>();
            switch (StatusType)
            {
                case 1:
                case 2:
                    smallPackages = await UserNameFor12(UID, StatusType);
                    break;
                case 3:
                    smallPackages = await UserNameFor3(OrderID, UID, OrderType);
                    break;
                default:
                    break;
            }
            return smallPackages;
        }

        public async Task<List<SmallPackage>> CheckBarCode(List<SmallPackage> items, bool isAssign)
        {
            var listSmallPackageNew = new List<SmallPackage>();
            foreach (var item in items)
            {
                if (item.Status > 0)
                {
                    var user = new Users();

                    var mainOrder = await mainOrderService.GetByIdAsync(item.MainOrderId ?? 0);
                    var trans = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == item.TransportationOrderId).FirstOrDefaultAsync();
                    if (mainOrder != null) //Đơn hàng mua hộ
                    {
                        if (isAssign)
                            throw new AppException("Không phải kiện nổi trổi");

                        user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Id == mainOrder.UID).FirstOrDefaultAsync();
                        if (user == null)
                            throw new KeyNotFoundException("Không tìm thấy User");

                        item.UserName = user.UserName;
                        item.Phone = user.Phone;

                        item.Currency = mainOrder.CurrentCNYVN;

                        item.OrderType = 1;
                        item.TotalOrder = mainOrder.Orders.Count;
                        item.TotalOrderQuantity = mainOrder.Orders.Sum(e => e.Quantity);

                        item.IsCheckProduct = mainOrder.IsCheckProduct;
                        item.IsPackged = mainOrder.IsPacked;
                        item.IsInsurance = mainOrder.IsInsurance;

                        listSmallPackageNew.Add(item);
                        unitOfWork.Repository<MainOrder>().Detach(mainOrder);

                    }
                    else if (trans != null) //Đơn hàng vận chuyển hộ (Ký gửi)
                    {
                        if (isAssign)
                            throw new AppException("Không phải kiện nổi trổi");
                        if (trans.Status == (int)StatusGeneralTransportationOrder.ChoDuyet)
                            continue;
                        //    throw new AppException("Đơn ký gửi chưa được duyệt");
                        user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Id == trans.UID).FirstOrDefaultAsync();
                        if (user == null)
                            throw new KeyNotFoundException("Không tìm thấy User");

                        item.UserName = user.UserName;
                        item.Phone = user.Phone;

                        item.Currency = trans.Currency;
                        item.TotalOrderQuantity = (int)trans.Amount;

                        item.IsCheckProduct = trans.IsCheckProduct;
                        item.IsPackged = trans.IsPacked;
                        item.IsInsurance = trans.IsInsurance;

                        item.OrderType = 2;

                        listSmallPackageNew.Add(item);
                        unitOfWork.Repository<TransportationOrder>().Detach(trans);

                    }
                    else //Kiện trôi nổi
                    {
                        item.OrderType = 3;
                        listSmallPackageNew.Add(item);
                    }
                }
            }
            return listSmallPackageNew;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var user = await unitOfWork.Repository<Users>().GetQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == LoginContext.Instance.CurrentUser.UserId);
            var exists = Queryable
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == id);
            if (exists != null)
            {
                exists.Deleted = true;
                unitOfWork.Repository<SmallPackage>().Update(exists);

                string statusName = "";
                switch (exists.Status)
                {
                    case (int)StatusSmallPackage.DaHuy:
                        statusName = "Đã hủy";
                        break;
                    case (int)StatusSmallPackage.MoiDat:
                        statusName = "Chưa về kho TQ";
                        break;
                    case (int)StatusSmallPackage.DaVeKhoTQ:
                        statusName = "Đã về kho TQ";
                        break;
                    case (int)StatusSmallPackage.DaVeKhoVN:
                        statusName = "Đã về kho VN";
                        break;
                    case (int)StatusSmallPackage.DaThanhToan:
                        statusName = "Đã thanh toán";
                        break;
                    case (int)StatusSmallPackage.DaGiao:
                        statusName = "Đã giao cho khách";
                        break;
                    default:
                        break;
                }

                var mainOrderCode = await unitOfWork.Repository<MainOrderCode>().GetQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == exists.MainOrderCodeId);

                //Thêm lịch sử đơn hàng thay đổi
                await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                {
                    MainOrderId = exists.MainOrderId,
                    UID = user.Id,
                    HistoryContent = String.Format("{0} đã xóa kiện hàng của đơn hàng ID là: {1}, Mã vận đơn: {2}, Mã đơn hàng: {3}, Cân nặng: {4}, Trạng thái kiện: {5}.", user.UserName, exists.MainOrderId, exists.OrderTransactionCode, mainOrderCode == null ? "" : mainOrderCode.Code, exists.Weight, statusName),
                    Type = (int?)TypeHistoryOrderChange.MaVanDon
                });

                await unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new Exception(id + " not exists");
            }
        }

        public override async Task<bool> UpdateAsync(IList<SmallPackage> items)
        {
            var mainOrderList = new List<MainOrder>();
            var transportationOrderList = new List<TransportationOrder>();

            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    DateTime currentDate = DateTime.Now;
                    var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
                    var mainOrderUpdateds = new List<MainOrder>();
                    foreach (var item in items)
                    {
                        var mainOrder = new MainOrder();
                        var transportationOrder = new TransportationOrder();
                        var historyOrderChanges = new List<HistoryOrderChange>();

                        //Kiểm tra phải gán đơn hay không
                        if (item.IsAssign)
                        {
                            if (user.UserGroupId == (int)PermissionTypes.Admin
                                    || user.UserGroupId == (int)PermissionTypes.Manager
                                    || user.UserGroupId == (int)PermissionTypes.VietNamWarehouseManager
                                    || user.UserGroupId == (int)PermissionTypes.ChinaWarehouseManager)
                            {

                                if ((item.MainOrderId != 0 && item.MainOrderId != null) || (item.TransportationOrderId != 0 && item.MainOrderId != null))
                                    throw new AppException("Kiện đã có chủ, vui lòng kiểm tra lại");

                                switch (item.AssignType)
                                {
                                    case 1: //Gán đơn cho khách mua hộ
                                        mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == item.AssignMainOrderId).FirstOrDefaultAsync();
                                        if (item.AssignMainOrderId == 0 && mainOrder == null)
                                            throw new KeyNotFoundException("Không tìm thấy Id, không thể gán kiện");
                                        if (mainOrder == null)
                                            throw new KeyNotFoundException($"Không tìm thấy mã đơn hàng {item.AssignMainOrderId}");
                                        item.UID = mainOrder.UID;
                                        item.MainOrderId = mainOrder.Id;
                                        var mainOrderCode = await unitOfWork.Repository<MainOrderCode>().GetQueryable().FirstOrDefaultAsync(x => x.MainOrderID == mainOrder.Id);
                                        if (mainOrderCode != null)
                                        {
                                            item.MainOrderCodeId = mainOrderCode.Id;
                                        }
                                        item.IsTemp = false;
                                        item.IsHelpMoving = false;

                                        historyOrderChanges.Add(new HistoryOrderChange()
                                        {
                                            MainOrderId = item.MainOrderId,
                                            UID = user.Id,
                                            HistoryContent = String.Format("{0} đã thêm mã vận đơn của đơn hàng ID là: {1}, Mã vận đơn {2}", user.UserName, item.MainOrderId, item.OrderTransactionCode),
                                            Type = (int)TypeHistoryOrderChange.MaVanDon
                                        });

                                        unitOfWork.Repository<SmallPackage>().Update(item);
                                        await unitOfWork.SaveAsync();

                                        break;
                                    case 2: //Gán đơn cho khách ký gửi

                                        transportationOrder.UID = item.AssignUID;
                                        transportationOrder.SmallPackageId = item.Id;
                                        transportationOrder.WareHouseFromId = item.WareHouseFromId;
                                        transportationOrder.WareHouseId = item.WareHouseId;
                                        transportationOrder.ShippingTypeId = item.ShippingTypeId;
                                        transportationOrder.OrderTransactionCode = item.OrderTransactionCode;
                                        switch (item.Status)
                                        {
                                            case (int)StatusSmallPackage.DaVeKhoTQ:
                                                transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoTQ;
                                                break;
                                            case (int)StatusSmallPackage.DaVeKhoVN:
                                                transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoVN;
                                                break;
                                            case (int)StatusSmallPackage.DaThanhToan:
                                                transportationOrder.Status = (int)StatusGeneralTransportationOrder.DaThanhToan;
                                                break;
                                            case (int)StatusSmallPackage.DaHuy:
                                                transportationOrder.Status = (int)StatusGeneralTransportationOrder.Huy;
                                                break;
                                            default:
                                                break;
                                        }

                                        transportationOrder.Note = item.AssignNote;

                                        await unitOfWork.Repository<TransportationOrder>().CreateAsync(transportationOrder);
                                        await unitOfWork.SaveAsync();

                                        item.UID = transportationOrder.UID;
                                        item.TransportationOrderId = transportationOrder.Id;

                                        item.IsTemp = false;
                                        item.IsHelpMoving = true;

                                        unitOfWork.Repository<SmallPackage>().Update(item);
                                        await unitOfWork.SaveAsync();
                                        break;
                                    default:
                                        break;
                                }

                            }
                            else
                                throw new InvalidCastException("Không có quyền gán đơn");
                        }
                        //Kiểm tra phải đang cập nhật kiện trôi nổi
                        else if (item.IsFloating)
                        {
                            mainOrder = null;
                            transportationOrder = null;
                        }
                        else
                        {
                            mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == item.MainOrderId).FirstOrDefaultAsync();
                            transportationOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == item.TransportationOrderId).FirstOrDefaultAsync();
                        }

                        var smallPackages = new List<SmallPackage>();

                        switch (item.Status)
                        {
                            case (int)StatusSmallPackage.DaVeKhoTQ: //Kiểm hàng kho TQ
                                item.DateInTQWarehouse = item.DateScanTQ = currentDate;
                                item.StaffTQWarehouse = user.UserName;
                                item.IsPayment = false;
                                var warehouseFrom = await unitOfWork.CatalogueRepository<WarehouseFrom>().GetQueryable().Where(e => !e.Deleted && user.WarehouseFrom == e.Id).FirstOrDefaultAsync();
                                if (warehouseFrom != null)
                                {
                                    item.CurrentPlaceId = warehouseFrom.Id;

                                    await unitOfWork.Repository<HistoryScanPackage>().CreateAsync(new HistoryScanPackage
                                    {
                                        SmallPackageId = item.Id,
                                        WareHouseId = warehouseFrom.Id
                                    });
                                }
                                //Thông báo cho user đơn hàng đã đến kho TQ
                                var notificationSettingTQ = await notificationSettingService.GetByIdAsync(8);
                                var notiTemplateUserTQ = await notificationTemplateService.GetByIdAsync(20);
                                //Đơn mua hộ
                                if (mainOrder != null && mainOrder.Id != 0)
                                {
                                    mainOrder.Status = (int)StatusOrderContants.DaVeKhoTQ;
                                    mainOrder.DateTQ = currentDate;

                                    smallPackages = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.MainOrderId == mainOrder.Id).ToListAsync();
                                    if (!smallPackages.Any())
                                        throw new KeyNotFoundException("Không tìm thấy smallpackage");

                                    decimal? weightOld = 0;
                                    int index = smallPackages.FindIndex(e => e.Id == item.Id);
                                    if (index != -1)
                                    {
                                        weightOld = smallPackages[index].Weight;
                                        smallPackages[index] = item;
                                    }
                                    else throw new AppException("Đã có lỗi xảy ra");

                                    mainOrder.SmallPackages = smallPackages;

                                    mainOrder = await mainOrderService.PriceAdjustment(mainOrder);

                                    historyOrderChanges.Add(new HistoryOrderChange()
                                    {
                                        MainOrderId = mainOrder.Id,
                                        UID = user.Id,
                                        HistoryContent = String.Format("{0} đã đổi trạng thái của mã vận đơn: {1} của đơn hàng ID: {2} là \"Đã về kho TQ\"", user.UserName, item.OrderTransactionCode, mainOrder.Id),
                                        Type = (int)TypeHistoryOrderChange.MaVanDon
                                    });

                                    if (item.Weight != weightOld)
                                    {
                                        historyOrderChanges.Add(new HistoryOrderChange()
                                        {
                                            MainOrderId = mainOrder.Id,
                                            UID = user.Id,
                                            HistoryContent = String.Format("{0} đã đổi cân nặng của mã vận đơn: {1} của đơn hàng ID: {2}, từ: {3}, sang: {4}", user.UserName, item.OrderTransactionCode, mainOrder.Id, weightOld, item.Weight),
                                            Type = (int)TypeHistoryOrderChange.MaVanDon
                                        });
                                    }

                                    if (smallPackages.Where(e => e.Status < (int)StatusSmallPackage.DaVeKhoTQ).Any())
                                    {
                                        var sp_main = smallPackages.Where(s => s.IsTemp != true).ToList();
                                        var sp_support_isvekhotq = smallPackages.Where(s => s.IsTemp == true && s.Status > (int)StatusSmallPackage.MoiDat).ToList();
                                        var sp_main_isvekhotq = smallPackages.Where(s => s.IsTemp != true && s.Status > (int)StatusSmallPackage.MoiDat).ToList();
                                        decimal che = sp_support_isvekhotq.Count + sp_main_isvekhotq.Count;
                                        if (che >= sp_main.Count)
                                        {
                                            if (mainOrder.Status <= (int)StatusOrderContants.DaVeKhoTQ)
                                            {
                                                if (mainOrder.DateTQ == null) mainOrder.DateTQ = currentDate;
                                                mainOrder.Status = (int)StatusOrderContants.DaVeKhoTQ;

                                                historyOrderChanges.Add(new HistoryOrderChange()
                                                {
                                                    MainOrderId = mainOrder.Id,
                                                    UID = user.Id,
                                                    HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng Id là: {1}, là \"Đã về kho TQ\"", user.UserName, mainOrder.Id),
                                                    Type = (int)TypeHistoryOrderChange.MaVanDon
                                                });
                                            }
                                        }
                                    }
                                    mainOrderList.Add(mainOrder);
                                    mainOrderUpdateds.Add(mainOrderList.LastOrDefault());
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
                                    await sendNotificationService.SendNotification(notificationSettingTQ, notiTemplateUserTQ, mainOrder.Id.ToString(),
                                        String.Format(Detail_MainOrder_Admin, mainOrder.Id), String.Format(Detail_MainOrder, mainOrder.Id), mainOrder.UID, string.Empty, string.Empty);
                                }
                                //Đơn ký gửi
                                else
                                {
                                    if (transportationOrder == null || transportationOrder.Id == 0) break;
                                    if (transportationOrder.Status == (int)StatusGeneralTransportationOrder.ChoDuyet)
                                        throw new AppException("Đơn ký gửi chưa được duyệt");
                                    transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoTQ;

                                    smallPackages = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.TransportationOrderId == transportationOrder.Id).ToListAsync();
                                    if (!smallPackages.Any())
                                        throw new KeyNotFoundException("Không tìm thấy smallpackage");

                                    int index = smallPackages.FindIndex(e => e.Id == item.Id);
                                    if (index != -1)
                                    {
                                        smallPackages[index] = item;
                                    }
                                    else throw new AppException("Đã có lỗi xảy ra");

                                    transportationOrder.SmallPackages = smallPackages;

                                    //Tính tiền
                                    transportationOrder = await transportationOrderService.PriceAdjustment(transportationOrder);

                                    if (!smallPackages.Where(e => e.Status < (int)StatusSmallPackage.DaVeKhoTQ).Any())
                                    {
                                        var sp_main = smallPackages.Where(s => s.IsTemp != true).ToList();
                                        var sp_support_isvekhotq = smallPackages.Where(s => s.IsTemp == true && s.Status > (int)StatusSmallPackage.MoiDat).ToList();
                                        var sp_main_isvekhotq = smallPackages.Where(s => s.IsTemp != true && s.Status > (int)StatusSmallPackage.MoiDat).ToList();
                                        decimal che = sp_support_isvekhotq.Count + sp_main_isvekhotq.Count;
                                        if (che >= sp_main.Count)
                                            transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoTQ;
                                    }

                                    //Detach
                                    if (!transportationOrderList.Select(e => e.Id).Contains(transportationOrder.Id))
                                    {
                                        unitOfWork.Repository<TransportationOrder>().Update(transportationOrder);
                                        //Cập nhật hoa hồng ký gửi
                                        var staffInCome = await unitOfWork.Repository<StaffIncome>()
                                                        .GetQueryable()
                                                        .FirstOrDefaultAsync(x => x.TransportationOrderId == transportationOrder.Id && x.Deleted == false);
                                        if (staffInCome != null)
                                        {
                                            staffInCome.TotalPriceReceive = transportationOrder.DeliveryPrice * staffInCome.PercentReceive / 100;
                                            staffInCome.OrderTotalPrice = transportationOrder.TotalPriceVND;
                                            await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffInCome, new Expression<Func<StaffIncome, object>>[]
                                            {
                                                s => s.TotalPriceReceive,
                                                s => s.OrderTotalPrice
                                            });
                                        }
                                    }
                                    transportationOrderList.Add(transportationOrder);

                                    await sendNotificationService.SendNotification(notificationSettingTQ, notiTemplateUserTQ, transportationOrder.Id.ToString(), String.Format(Detail_Transportorder_Admin, transportationOrder.Id), String.Format(Detail_Transportorder), transportationOrder.UID, string.Empty, string.Empty);
                                }

                                break;
                            case (int)StatusSmallPackage.DaVeKhoVN: //Kiểm hàng kho VN

                                item.DateInLasteWareHouse = item.DateScanVN = currentDate;
                                item.StaffVNWarehouse = user.UserName;
                                item.IsPayment = false;
                                if (item.DateInVNTemp == null)
                                    item.DateInVNTemp = currentDate;

                                var warehouse = await unitOfWork.CatalogueRepository<Warehouse>().GetQueryable().Where(e => !e.Deleted && user.WarehouseTo == e.Id).FirstOrDefaultAsync();
                                if (warehouse != null)
                                {
                                    item.CurrentPlaceId = warehouse.Id;

                                    await unitOfWork.Repository<HistoryScanPackage>().CreateAsync(new HistoryScanPackage
                                    {
                                        SmallPackageId = item.Id,
                                        WareHouseId = warehouse.Id
                                    });
                                }

                                //Thông báo cho user đơn hàng đã đến kho VN
                                var notificationSettingVN = await notificationSettingService.GetByIdAsync(9);
                                var notiTemplateUserVN = await notificationTemplateService.GetByIdAsync(19);

                                if (mainOrder != null && mainOrder.Id != 0)
                                {
                                    mainOrder.Status = (int)StatusOrderContants.DaVeKhoVN;
                                    mainOrder.DateVN = currentDate;

                                    smallPackages = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.MainOrderId == mainOrder.Id).ToListAsync();
                                    if (!smallPackages.Any())
                                        throw new KeyNotFoundException("Không tìm thấy smallpackage");

                                    decimal? weightOld = 0;
                                    int index = smallPackages.FindIndex(e => e.Id == item.Id);
                                    if (index != -1)
                                    {
                                        weightOld = smallPackages[index].Weight;
                                        smallPackages[index] = item;
                                    }
                                    else throw new AppException("Đã có lỗi xảy ra");

                                    mainOrder.SmallPackages = smallPackages;
                                    //Tính tiền
                                    mainOrder = await mainOrderService.PriceAdjustment(mainOrder);

                                    historyOrderChanges.Add(new HistoryOrderChange()
                                    {
                                        MainOrderId = mainOrder.Id,
                                        UID = user.Id,
                                        HistoryContent = String.Format("{0} đã đổi trạng thái của mã vận đơn: {1} của đơn hàng ID: {2} là \"Đã về kho VN\"", user.UserName, item.OrderTransactionCode, mainOrder.Id),
                                        Type = (int)TypeHistoryOrderChange.MaVanDon
                                    });

                                    if (item.Weight != weightOld)
                                    {
                                        historyOrderChanges.Add(new HistoryOrderChange()
                                        {
                                            MainOrderId = mainOrder.Id,
                                            UID = user.Id,
                                            HistoryContent = String.Format("{0} đã đổi cân nặng của mã vận đơn: {1} của đơn hàng ID: {2}, từ: {3}, sang: {4}", user.UserName, item.OrderTransactionCode, mainOrder.Id, weightOld, item.Weight),
                                            Type = (int)TypeHistoryOrderChange.MaVanDon
                                        });
                                    }

                                    if (smallPackages.Where(e => e.Status < (int)StatusSmallPackage.DaVeKhoVN && e.CurrentPlaceId == mainOrder.ReceivePlace).Any())
                                    {
                                        var sp_main = smallPackages.Where(s => s.IsTemp != true).ToList();
                                        var sp_support_isvekhotq = smallPackages.Where(s => s.IsTemp == true && s.Status >= (int)StatusSmallPackage.DaVeKhoVN).ToList();
                                        var sp_main_isvekhotq = smallPackages.Where(s => s.IsTemp != true && s.Status >= (int)StatusSmallPackage.DaVeKhoVN).ToList();
                                        decimal che = sp_support_isvekhotq.Count + sp_main_isvekhotq.Count;
                                        if (che >= sp_main.Count)
                                        {
                                            if (mainOrder.Status <= (int)StatusOrderContants.DaVeKhoVN)
                                            {
                                                if (mainOrder.DateVN == null) mainOrder.DateVN = currentDate;
                                                mainOrder.Status = (int)StatusOrderContants.DaVeKhoVN;

                                                historyOrderChanges.Add(new HistoryOrderChange()
                                                {
                                                    MainOrderId = mainOrder.Id,
                                                    UID = user.Id,
                                                    HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng Id là: {1}, là \"Đã về kho VN\"", user.UserName, mainOrder.Id),
                                                    Type = (int)TypeHistoryOrderChange.MaVanDon
                                                });
                                            }
                                        }
                                    }
                                    //Detach
                                    mainOrderList.Add(mainOrder);
                                    mainOrderUpdateds.Add(mainOrderList.LastOrDefault());
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
                                    await sendNotificationService.SendNotification(notificationSettingVN, notiTemplateUserVN, mainOrder.Id.ToString(), String.Format(Detail_MainOrder_Admin, mainOrder.Id), String.Format(Detail_MainOrder, mainOrder.Id), mainOrder.UID, string.Empty, string.Empty);
                                }
                                else
                                {
                                    if (transportationOrder == null || transportationOrder.Id == 0) break;

                                    transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoVN;

                                    smallPackages = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.TransportationOrderId == transportationOrder.Id).ToListAsync();
                                    if (!smallPackages.Any())
                                        throw new KeyNotFoundException("Không tìm thấy smallpackage");

                                    int index = smallPackages.FindIndex(e => e.Id == item.Id);
                                    if (index != -1)
                                    {
                                        smallPackages[index] = item;
                                    }
                                    else throw new AppException("Đã có lỗi xảy ra");
                                    transportationOrder.SmallPackages = smallPackages;
                                    //Tính tiền
                                    transportationOrder = await transportationOrderService.PriceAdjustment(transportationOrder);

                                    if (!smallPackages.Where(e => e.Status < (int)StatusSmallPackage.DaVeKhoVN && e.CurrentPlaceId == transportationOrder.WareHouseId).Any())
                                    {
                                        var sp_main = smallPackages.Where(s => s.IsTemp != true).ToList();
                                        var sp_support_isvekhotq = smallPackages.Where(s => s.IsTemp == true && s.Status >= (int)StatusSmallPackage.DaVeKhoVN).ToList();
                                        var sp_main_isvekhotq = smallPackages.Where(s => s.IsTemp != true && s.Status >= (int)StatusSmallPackage.DaVeKhoVN).ToList();
                                        decimal che = sp_support_isvekhotq.Count + sp_main_isvekhotq.Count;
                                        if (che >= sp_main.Count)
                                            transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoVN;
                                    }
                                    //Detach
                                    if (!transportationOrderList.Select(e => e.Id).Contains(transportationOrder.Id))
                                    {
                                        unitOfWork.Repository<TransportationOrder>().Update(transportationOrder);
                                        //Cập nhật hoa hồng ký gửi
                                        var staffInCome = await unitOfWork.Repository<StaffIncome>()
                                                        .GetQueryable()
                                                        .FirstOrDefaultAsync(x => x.TransportationOrderId == transportationOrder.Id && x.Deleted == false);
                                        if (staffInCome != null)
                                        {
                                            staffInCome.TotalPriceReceive = transportationOrder.DeliveryPrice * staffInCome.PercentReceive / 100;
                                            staffInCome.OrderTotalPrice = transportationOrder.TotalPriceVND;
                                            await unitOfWork.Repository<StaffIncome>().UpdateFieldsSaveAsync(staffInCome, new Expression<Func<StaffIncome, object>>[]
                                            {
                                                s => s.TotalPriceReceive,
                                                s => s.OrderTotalPrice
                                            });
                                        }
                                    }
                                    transportationOrderList.Add(transportationOrder);
                                    await sendNotificationService.SendNotification(notificationSettingVN, notiTemplateUserVN, transportationOrder.Id.ToString(), String.Format(Detail_Transportorder_Admin, transportationOrder.Id), String.Format(Detail_Transportorder), transportationOrder.UID, string.Empty, string.Empty);
                                }

                                //Kiêm tra nếu 1 mã vận đơn trong bao lớn đã về VN thì đổi trạng thái đã về VN
                                if (item.BigPackageId != null && item.BigPackageId > 0)
                                {
                                    var bigPackage = await unitOfWork.CatalogueRepository<BigPackage>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.BigPackageId);
                                    bigPackage.Status = (int)StatusBigPackage.DaNhanHang;
                                    unitOfWork.Repository<BigPackage>().Update(bigPackage);
                                    await unitOfWork.SaveAsync();
                                    unitOfWork.Repository<BigPackage>().Detach(bigPackage);
                                }
                                break;
                            default:
                                break;
                        }
                        //item.DonGia = Math.Round(item.DonGia.Value, 0);
                        //item.TotalPrice = Math.Round(item.TotalPrice.Value, 0);
                        unitOfWork.Repository<SmallPackage>().Update(item);
                        await unitOfWork.SaveAsync();
                    }
                    foreach (var mainOrderUpdated in mainOrderUpdateds)
                    {
                        unitOfWork.Repository<MainOrder>().Update(mainOrderUpdated);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<MainOrder>().Detach(mainOrderUpdated);
                    }
                    await dbContextTransaction.CommitAsync();

                    //Detach(tại sau khi Update có GetById thằng MainOrder, mà getby thì có cập nhật => Bug)
                    foreach (var mainOrderDetach in mainOrderList)
                    {
                        unitOfWork.Repository<MainOrder>().Detach(mainOrderDetach);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task<bool> UpdateIsLost(SmallPackage item)
        {
            var bigPackage = await unitOfWork.CatalogueRepository<BigPackage>().GetQueryable().Where(e => e.Id == item.BigPackageId).FirstOrDefaultAsync();
            if (bigPackage != null)
            {
                var smallPackageNotBigPackage = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(e => !e.Deleted
                    && e.BigPackageId == bigPackage.Id && e.OrderTransactionCode.Equals("")
                    && e.Status >= (int)StatusSmallPackage.DaVeKhoVN).ToListAsync();

                if (smallPackageNotBigPackage.Any())
                {
                    bigPackage.Status = (int)StatusBigPackage.DaNhanHang;
                    unitOfWork.CatalogueRepository<BigPackage>().Update(bigPackage);
                }
            }

            item.IsLost = true;
            item.BigPackageId = 0;

            unitOfWork.Repository<SmallPackage>().Update(item);
            await unitOfWork.SaveAsync();
            return true;
        }

        public Task<AppDomainImportResult> ImportTemplateFile(Stream stream, string createdBy)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Import file danh mục
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        public virtual async Task<AppDomainImportResult> ImportTemplateFile(Stream stream, int? bigPackageId, string createdBy)
        {
            AppDomainImportResult appDomainImportResult = new AppDomainImportResult();
            DateTime currentDate = DateTime.Now;
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                int totalSuccess = 0;
                List<string> failedResult = new List<string>();

                var ws = package.Workbook.Worksheets.FirstOrDefault();

                if (ws.Columns.Range.Columns != 2 || !((string)(ws.Cells[1, 1]).Value).Equals("MÃ VẬN ĐƠN") || !((string)(ws.Cells[1, 2]).Value).Equals("CÂN NẶNG"))
                    throw new AppException("Tập tin không đúng định dạng");

                if (ws == null) throw new Exception("Sheet name không tồn tại");
                var catalogueMappers = new ExcelMapper(stream) { HeaderRow = false, MinRowNumber = 1 }.Fetch<SmallPackageMapper>().ToList();
                if (catalogueMappers == null || !catalogueMappers.Any()) throw new Exception("Sheet không có dữ liệu");

                int duplicateCount = 0;
                HashSet<string> checkSet = new HashSet<string>();
                int updateCount = 0;
                foreach (var catalogueMapper in catalogueMappers)
                {
                    string orderTransactionCode = catalogueMapper.OrderTransactionCode;

                    var smallPackage = await Queryable.Where(e => !e.Deleted && e.OrderTransactionCode.Equals(orderTransactionCode)).FirstOrDefaultAsync();
                    if (smallPackage != null)
                    {
                        smallPackage.Weight = catalogueMapper.Weight;
                        smallPackage.BigPackageId = bigPackageId;
                        smallPackage.Status = (int)StatusSmallPackage.DaVeKhoTQ;
                        smallPackage.DateInTQWarehouse = currentDate;
                        if (!checkSet.Add(catalogueMapper.OrderTransactionCode))
                        {
                            duplicateCount++;
                        }
                        else
                        {
                            updateCount++;
                        }
                    }
                    //Tạo mã vận đơn mới
                    if (smallPackage == null)
                    {
                        if (!checkSet.Add(catalogueMapper.OrderTransactionCode))
                        {
                            duplicateCount++;
                        }
                        smallPackage = new SmallPackage();
                        smallPackage.OrderTransactionCode = orderTransactionCode;
                        smallPackage.IsTemp = true;
                        smallPackage.Weight = catalogueMapper.Weight;
                        smallPackage.BigPackageId = bigPackageId;
                        smallPackage.Status = (int)StatusSmallPackage.DaVeKhoTQ;
                        smallPackage.DateInTQWarehouse = currentDate;

                        await unitOfWork.Repository<SmallPackage>().CreateAsync(smallPackage);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<SmallPackage>().Detach(smallPackage);

                        totalSuccess++;
                        continue;
                    }

                    //Đơn mua hộ
                    if (smallPackage.MainOrderId != 0)
                    {
                        var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.MainOrderId).FirstOrDefaultAsync();
                        if (mainOrder == null)
                            failedResult.Add(smallPackage.OrderTransactionCode);
                        else
                        {
                            unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                            mainOrder.Status = (int)StatusOrderContants.DaVeKhoTQ;
                            mainOrder.DateTQ = currentDate;
                            await unitOfWork.Repository<MainOrder>().UpdateFieldsSaveAsync(mainOrder, new Expression<Func<MainOrder, object>>[]
                            {
                                x =>x.Status,
                                x =>x.DateTQ
                            });
                            unitOfWork.Repository<MainOrder>().Detach(mainOrder);
                            totalSuccess++;
                        }
                    }
                    //Đơn vận chuyển hộ
                    else if (smallPackage.TransportationOrderId != 0)
                    {
                        var transportationOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();
                        if (transportationOrder == null)
                            failedResult.Add(smallPackage.OrderTransactionCode);
                        else
                        {
                            unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                            transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoTQ;
                            await unitOfWork.Repository<TransportationOrder>().UpdateFieldsSaveAsync(transportationOrder, new Expression<Func<TransportationOrder, object>>[]
                            {
                                x =>x.Status
                            });
                            unitOfWork.Repository<TransportationOrder>().Detach(transportationOrder);
                            totalSuccess++;
                        }
                    }
                    //Kiện trôi nổi
                    else if (smallPackage.IsTemp.HasValue && smallPackage.IsTemp.Value)
                    {
                        unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                        totalSuccess++;
                    }
                    else
                    {
                        failedResult.Add(smallPackage.OrderTransactionCode);
                    }
                    await unitOfWork.SaveAsync();
                    unitOfWork.Repository<SmallPackage>().Detach(smallPackage);
                }

                appDomainImportResult.Data = new
                {
                    TotalSuccess = totalSuccess,
                    TotalFailed = failedResult.Count,
                    FailedResult = failedResult,
                    TotalUpdate = updateCount,
                    TotalDuplicate = duplicateCount
                };
                appDomainImportResult.Success = true;
                return appDomainImportResult;
            }
        }

        public async Task<SmallPackage> GetByOrderTransactionCode(string code)
        {
            return await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.OrderTransactionCode == code && !x.Deleted).FirstOrDefaultAsync();
        }

        public async Task<List<SmallPackage>> GetAllByMainOrderId(int mainOrderId)
        {
            return await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.MainOrderId == mainOrderId).ToListAsync();
        }

        public async Task<List<SmallPackage>> GetInVietNamByMainOrderId(int mainOrderId)
        {
            return await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.MainOrderId == mainOrderId && x.Status == (int)StatusSmallPackage.DaVeKhoVN).ToListAsync();
        }

        public async Task<List<SmallPackage>> GetAllByTransportationOrderId(int transportationOrderId)
        {
            return await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.TransportationOrderId == transportationOrderId).ToListAsync();
        }
        protected async Task<List<SmallPackage>> BarCodeFor12(string BarCode, int UID, int Status)
        {
            //1: Chưa yêu cầu, 2: Đã yêu cầu
            List<SmallPackage> getPackages = new List<SmallPackage>();
            var user = await userService.GetByIdAsync(UID);
            if (user == null)
                throw new KeyNotFoundException("User không tìm thấy");

            var smallPackage = await this.GetSingleAsync(x => x.OrderTransactionCode.Equals(BarCode));
            if (smallPackage == null)
                throw new KeyNotFoundException("Không tìm thấy kiện");

            if (Status == 2) //Đã yêu cầu
            {
                var rOS = await unitOfWork.Repository<RequestOutStock>().GetQueryable().Where(e => !e.Deleted && e.SmallPackageId == smallPackage.Id).FirstOrDefaultAsync();

                if (rOS == null)
                    throw new AppException("Kiện này khách chưa yêu cầu");

                var eRT = await unitOfWork.Repository<ExportRequestTurn>().GetQueryable().Where(e => !e.Deleted && e.Id == rOS.ExportRequestTurnId).FirstOrDefaultAsync();
                if (eRT == null)
                    throw new AppException("Không đúng yêu cầu");
                if (eRT.Status != 2)
                    throw new AppException("Kiện chưa thanh toán");
            }

            if (smallPackage.Status > 0)
            {
                if (smallPackage.TransportationOrderId > 0)
                {
                    var trans = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();
                    if (trans == null)
                        throw new KeyNotFoundException("TransportationOrder không tồn tại");
                    if (user.Id != trans.UID)
                        throw new KeyNotFoundException("User không tồn tại");

                    smallPackage.UID = user.Id;
                    smallPackage.UserName = user.UserName;
                    smallPackage.Phone = user.Phone;

                    smallPackage.OrderType = 2; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định
                }
                else
                {
                    smallPackage.OrderType = 3; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định
                }

                getPackages.Add(smallPackage);
            }

            return getPackages;
        }

        protected async Task<List<SmallPackage>> BarCodeFor3(string BarCode, int UID)
        {
            //3: Xuất kho
            List<SmallPackage> getPackages = new List<SmallPackage>();
            var user = await userService.GetByIdAsync(UID);
            if (user == null)
                throw new KeyNotFoundException("User không tìm thấy");
            var smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                            && (x.OrderTransactionCode.Equals(BarCode)) && x.UID == UID
            );
            foreach (var smallPackage in smallPackages)
            {
                if (smallPackage.Status > 0)
                {
                    if (smallPackage.MainOrderId > 0)
                    {
                        var mainOrder = await mainOrderService.GetByIdAsync(smallPackage.MainOrderId ?? 0);
                        if (mainOrder == null)
                            throw new KeyNotFoundException("MainOrder không tồn tại");

                        if (user.Id != mainOrder.UID)
                            throw new KeyNotFoundException("User không tồn tại");

                        smallPackage.UID = user.Id;
                        smallPackage.UserName = user.UserName;
                        smallPackage.Phone = user.Phone;

                        smallPackage.IsCheckProduct = mainOrder.IsCheckProduct;
                        smallPackage.IsPackged = mainOrder.IsPacked;
                        smallPackage.IsInsurance = mainOrder.IsInsurance;

                        smallPackage.OrderType = 1; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định
                    }
                    else if (smallPackage.TransportationOrderId > 0)
                    {
                        var trans = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();
                        if (trans == null)
                            throw new KeyNotFoundException("TransportationOrder không tồn tại");

                        if (user.Id != trans.UID)
                            throw new KeyNotFoundException("User không tồn tại");

                        smallPackage.UID = user.Id;
                        smallPackage.UserName = user.UserName;
                        smallPackage.Phone = user.Phone;

                        smallPackage.IsCheckProduct = trans.IsCheckProduct;
                        smallPackage.IsPackged = trans.IsPacked;
                        smallPackage.IsInsurance = trans.IsInsurance;

                        smallPackage.OrderType = 2; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định
                    }
                    //else
                    //{
                    //    smallPackage.OrderType = 3; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định
                    //}
                    getPackages.Add(smallPackage);
                }
            }
            return getPackages;
        }

        protected async Task<List<SmallPackage>> UserNameFor12(int UID, int Status)
        {
            //1: Chưa yêu cầu, 2: Đã yêu cầu
            List<SmallPackage> getPackages = new List<SmallPackage>();
            var user = await userService.GetByIdAsync(UID);
            if (user == null)
                throw new KeyNotFoundException("User không tìm thấy");

            IList<TransportationOrder> transportationOrders = new List<TransportationOrder>();

            if (Status == 1) //Chưa yêu cầu
                transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id && e.Status == (int)StatusGeneralTransportationOrder.VeKhoVN).ToListAsync();
            else //Đã yêu cầu
                transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id && e.Status == (int)StatusGeneralTransportationOrder.DaThanhToan).ToListAsync();

            if (!transportationOrders.Any())
                throw new KeyNotFoundException("TransportationOrder không tìm thấy");

            foreach (var transportationOrder in transportationOrders)
            {
                var smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                    && (x.TransportationOrderId == transportationOrder.Id)
                );

                if (!smallPackages.Any())
                    continue;

                foreach (var smallPackage in smallPackages)
                {
                    if (Status == 2) //Đã yêu cầu
                    {
                        var rOS = await unitOfWork.Repository<RequestOutStock>().GetQueryable().Where(e => !e.Deleted && e.SmallPackageId == smallPackage.Id).FirstOrDefaultAsync();

                        if (rOS == null)
                            throw new KeyNotFoundException("Kiện này khách chưa yêu cầu");

                        var eRT = await unitOfWork.Repository<ExportRequestTurn>().GetQueryable().Where(e => !e.Deleted && e.Id == rOS.ExportRequestTurnId).FirstOrDefaultAsync();
                        if (eRT == null)
                            throw new AppException("Không đúng yêu cầu");
                        if (eRT.Status != 2)
                            throw new AppException("Kiện chưa thanh toán");
                    }

                    if (smallPackage.Status > 0)
                    {
                        if (smallPackage.TransportationOrderId > 0)
                        {
                            var trans = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();
                            if (trans == null)
                                throw new KeyNotFoundException("TransportationOrder không tồn tại");

                            if (user.Id != trans.UID)
                                throw new KeyNotFoundException("User không tồn tại");

                            smallPackage.UID = user.Id;
                            smallPackage.UserName = user.UserName;
                            smallPackage.Phone = user.Phone;

                            smallPackage.OrderType = 2; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định
                        }
                        else
                        {
                            smallPackage.OrderType = 3; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định
                        }
                    }

                    getPackages.Add(smallPackage);
                }
            }

            return getPackages;
        }

        protected async Task<List<SmallPackage>> UserNameFor3(int OrderID, int UID, int OrderType)
        {
            //3: Xuất kho
            List<SmallPackage> getPackages = new List<SmallPackage>();
            var user = await userService.GetByIdAsync(UID);
            if (user == null)
                throw new KeyNotFoundException("User không tìm thấy");

            IList<MainOrder> mainOrders = new List<MainOrder>();
            IList<SmallPackage> smallPackages = new List<SmallPackage>();
            IList<TransportationOrder> transportationOrders = new List<TransportationOrder>();

            switch (OrderType)
            {
                case 1:
                    if (OrderID > 0)
                        mainOrders = await mainOrderService.GetAsync(x => !x.Deleted && x.Active
                            && (x.UID == user.Id && x.Id == OrderID)
                        ); //getById
                    else
                        mainOrders = await mainOrderService.GetAsync(x => !x.Deleted && x.Active
                            && (x.UID == user.Id)
                        );

                    if (!mainOrders.Any())
                        throw new KeyNotFoundException("MainOrder không tìm thấy");

                    foreach (var mainOrder in mainOrders)
                    {
                        if (OrderID > 0)
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.MainOrderId == mainOrder.Id)
                            );
                        else
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.MainOrderId == mainOrder.Id && x.Status == 3)
                            );

                        foreach (var smallPackage in smallPackages)
                        {
                            smallPackage.UID = user.Id;
                            smallPackage.UserName = user.UserName;
                            smallPackage.Phone = user.Phone;

                            smallPackage.OrderType = 1; //1: Xuất kho, 2: Ký gửi đã yêu cầu, chưa yêu cầu, 3: Chưa xác định

                            getPackages.Add(smallPackage);
                        }
                    }

                    break;
                case 2:
                    if (OrderID > 0)
                        transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id && e.Id == OrderID).ToListAsync();
                    else
                        transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id).ToListAsync();

                    foreach (var transportationOrder in transportationOrders)
                    {
                        if (OrderID > 0)
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.TransportationOrderId == transportationOrder.Id)
                            );
                        else
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.TransportationOrderId == transportationOrder.Id && x.Status == 3)
                            );
                        foreach (var smallPackage in smallPackages)
                        {
                            smallPackage.UID = user.Id;
                            smallPackage.UserName = user.UserName;
                            smallPackage.Phone = user.Phone;

                            smallPackage.OrderType = 2; //1: Xuất kho, 2: Ký gửi chưa yêu cầu, đã yêu cầu, 3: Chưa xác định

                            getPackages.Add(smallPackage);
                        }
                    }

                    break;
                default:
                    if (OrderID > 0)
                    {
                        mainOrders = await mainOrderService.GetAsync(x => !x.Deleted && x.Active
                            && (x.UID == user.Id && x.Id == OrderID)
                        ); //getById

                        if (mainOrders.Any()) //Có tồn tại
                        {
                            foreach (var mainOrder in mainOrders)
                            {
                                smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                    && (x.MainOrderId == mainOrder.Id)
                                );
                                foreach (var smallPackage in smallPackages)
                                {
                                    smallPackage.UID = user.Id;
                                    smallPackage.UserName = user.UserName;
                                    smallPackage.Phone = user.Phone;

                                    smallPackage.IsCheckProduct = mainOrder.IsCheckProduct;
                                    smallPackage.IsPackged = mainOrder.IsPacked;
                                    smallPackage.IsInsurance = mainOrder.IsInsurance;

                                    smallPackage.OrderType = 1; //1: Xuất kho, 2: Ký gửi chưa yêu cầu, đã yêu cầu, 3: Chưa xác định

                                    getPackages.Add(smallPackage);
                                }
                            }
                        }
                        else
                        {
                            transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id && e.Id == OrderID).ToListAsync();

                            foreach (var transportationOrder in transportationOrders)
                            {
                                smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                    && (x.TransportationOrderId == transportationOrder.Id)
                                );
                                foreach (var smallPackage in smallPackages)
                                {
                                    smallPackage.UID = user.Id;
                                    smallPackage.UserName = user.UserName;
                                    smallPackage.Phone = user.Phone;

                                    smallPackage.OrderType = 2; //1: Xuất kho, 2: Ký gửi chưa yêu cầu, đã yêu cầu, 3: Chưa xác định

                                    getPackages.Add(smallPackage);
                                }
                            }
                        }
                    }
                    else
                    {
                        smallPackages = await this.GetAsync(x => !x.Deleted && x.Active

                            && (x.UID == user.Id && x.Status == 3)
                        );

                        foreach (var smallPackage in smallPackages)
                        {
                            if (smallPackage.MainOrderId > 0)
                            {
                                smallPackage.UID = user.Id;
                                smallPackage.UserName = user.UserName;
                                smallPackage.Phone = user.Phone;

                                mainOrders = await mainOrderService.GetAsync(x => !x.Deleted && x.Active
                                    && (x.Id == smallPackage.MainOrderId)
                                ); //getById
                                foreach (var mainOrder in mainOrders)
                                {
                                    smallPackage.IsCheckProduct = mainOrder.IsCheckProduct;
                                    smallPackage.IsPackged = mainOrder.IsPacked;
                                    smallPackage.IsInsurance = mainOrder.IsInsurance;
                                }

                                smallPackage.OrderType = (int)TypeOrder.DonHangMuaHo;

                                getPackages.Add(smallPackage);
                            }
                            if (smallPackage.TransportationOrderId > 0)
                            {
                                smallPackage.UID = user.Id;
                                smallPackage.UserName = user.UserName;
                                smallPackage.Phone = user.Phone;

                                transportationOrders = await transportationOrderService.GetAsync(x => !x.Deleted && x.Active
                                    && (x.Id == smallPackage.TransportationOrderId)
                                );
                                foreach (var transOrder in transportationOrders)
                                {
                                    smallPackage.IsCheckProduct = transOrder.IsCheckProduct;
                                    smallPackage.IsPackged = transOrder.IsPacked;
                                    smallPackage.IsInsurance = transOrder.IsInsurance;
                                }
                                smallPackage.OrderType = (int)TypeOrder.DonKyGui;

                                getPackages.Add(smallPackage);
                            }
                        }
                    }
                    break;
            }
            return getPackages;
        }


    }
}
