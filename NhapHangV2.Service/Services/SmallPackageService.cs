using AutoMapper;
using Ganss.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.Configuration;
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
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;
using SmallPackageToolRequest = NhapHangV2.Entities.SmallPackageToolRequest;
using Users = NhapHangV2.Entities.Users;

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
        public async Task AssignBigPackage(AssignBigPackageRequest request)
        {
            var bigPackage = await unitOfWork.Repository<BigPackage>().GetQueryable().FirstOrDefaultAsync(x => x.Id == request.BigPackageId);
            if (bigPackage == null || bigPackage?.Status != (int?)StatusBigPackage.MoiTao)
            {
                throw new AppException("Không tìm thấy bao lớn");
            }
            for (int i = 0; i < request.SmallPackageIds.Count; i++)
            {
                var smallPackage = await unitOfWork.Repository<SmallPackage>().GetQueryable().FirstOrDefaultAsync(x => x.Id == request.SmallPackageIds[i]);
                if (smallPackage?.Status != (int)StatusSmallPackage.MoiTao || smallPackage?.Status != (int)StatusSmallPackage.VeKhoTQ)
                {
                    smallPackage.Status = (int)StatusSmallPackage.VeKhoTQ;
                    smallPackage.BigPackageId = bigPackage.Id;
                    await unitOfWork.Repository<SmallPackage>().UpdateFieldsSaveAsync(smallPackage, new Expression<Func<SmallPackage, object>>[]
                    {
                        x => x.Status,
                        x => x.BigPackageId,
                        x => x.Updated,
                        x => x.UpdatedBy
                    });
                }
            }
        }
        public void CreateSmallPackageTool(List<SmallPackageToolRequest> requests)
        {
            try
            {
                var currentUser = LoginContext.Instance.CurrentUser;
                var currentDate = DateTime.Now;
                string sql = "";
                int i = 0;
                foreach (var request in requests)
                {
                    sql += $"DECLARE @UID{i} INT = (SELECT UID FROM MainOrder WHERE Id = {request.MainOrderId}) " +
                        $"DECLARE @MVD{i} INT = (SELECT COUNT(Id) FROM SmallPackage WHERE OrderTransactionCode = '{request.OrdertransactionCode}') " +
                        $"IF(@UID{i} > 0 AND @MVD{i} < 1) " +
                        $"BEGIN " +
                        $"INSERT INTO SmallPackage(MainOrderId, UID, OrderTransactionCode, FeeShip, " +
                        $"Weight,Status,FloatingStatus,FloatingUserName,FloatingUserPhone,IsTemp,IsLost,IsHelpMoving, " +
                        $"TotalPrice,StaffTQWarehouse,StaffVNWarehouse,StaffVNOutWarehouse,CurrentPlaceId, " +
                        $"MainOrderCodeId,DonGia,PriceWeight,Created,CreatedBy,Deleted,Active,IsPayment,PriceVolume,VolumePayment) " +
                        $"VALUES ({request.MainOrderId}, @UID{i}, N'{request.OrdertransactionCode}', 0, " +
                        $"0,{(int)StatusSmallPackage.MoiTao},0,'','',0,0,0, " +
                        $"0,'','','',0, " +
                        $"{request.MainOrderCodeId}, 0,0,'{currentDate}',N'{currentUser.UserName}',0,1,0,0,0) " +
                        $"INSERT INTO HistoryOrderChange(MainOrderId, UID, HistoryContent,Type,Created,CreatedBy,Deleted,Active) " +
                        $"VALUES ({request.MainOrderId}, {currentUser.UserId}, N'{currentUser.UserName} đã thêm mã vận đơn {request.OrdertransactionCode} vào đơn hàng #{request.MainOrderId} bằng công cụ',11, '{currentDate}', N'{currentUser.UserName}',0,1) " +
                        $"END ";
                    i++;
                }
                unitOfWork.Repository<SmallPackage>().ExecuteNonQuery(sql);
            }
            catch
            (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
                    case (int)StatusSmallPackage.MoiTao:
                        statusName = "Mới tạo";
                        break;
                    case (int)StatusSmallPackage.VeKhoTQ:
                        statusName = "Đã về kho TQ";
                        break;
                    case (int)StatusSmallPackage.XuatKhoTQ:
                        statusName = "Xuất kho TQ";
                        break;
                    case (int)StatusSmallPackage.VeKhoVN:
                        statusName = "Đã về kho VN";
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
            var config = await unitOfWork.Repository<Entities.Configurations>().GetQueryable().FirstOrDefaultAsync(x => x.Id == 1);

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
                        var oldItem = await unitOfWork.Repository<SmallPackage>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.Id);
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
                                        var assignUser = await unitOfWork.Repository<Users>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.AssignUID);
                                        if (assignUser == null)
                                            break;
                                        decimal? currencyTransportation = assignUser.Currency > 0 ? assignUser.Currency : config.AgentCurrency;
                                        transportationOrder.UID = item.AssignUID;
                                        transportationOrder.SmallPackageId = item.Id;
                                        transportationOrder.WareHouseFromId = item.WareHouseFromId;
                                        transportationOrder.WareHouseId = item.WareHouseId;
                                        transportationOrder.ShippingTypeId = item.ShippingTypeId;
                                        transportationOrder.OrderTransactionCode = item.OrderTransactionCode;
                                        transportationOrder.Currency = currencyTransportation;
                                        switch (item.Status)
                                        {
                                            case (int)StatusSmallPackage.VeKhoTQ:
                                                if (transportationOrder.Status < (int)StatusGeneralTransportationOrder.VeKhoTQ)
                                                    transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoTQ;
                                                break;
                                            case (int)StatusSmallPackage.XuatKhoTQ:
                                                if (transportationOrder.Status < (int)StatusGeneralTransportationOrder.DangVeVN)
                                                    transportationOrder.Status = (int)StatusGeneralTransportationOrder.DangVeVN;
                                                break;
                                            case (int)StatusSmallPackage.VeKhoVN:
                                                if (transportationOrder.Status < (int)StatusGeneralTransportationOrder.VeKhoVN)
                                                    transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoVN;
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
                                        historyOrderChanges.Add(new HistoryOrderChange()
                                        {
                                            TransportationOrderId = transportationOrder.Id,
                                            UID = user.Id,
                                            HistoryContent = $"{user.UserName} đã thêm mã vận đơn của đơn hàng ID là: {transportationOrder.Id}, Mã vận đơn {item.OrderTransactionCode}.",
                                            Type = (int?)TypeHistoryOrderChange.MaVanDon
                                        });
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
                        var warehouseFrom = await unitOfWork.CatalogueRepository<WarehouseFrom>().GetQueryable().Where(e => !e.Deleted && user.WarehouseFrom == e.Id).FirstOrDefaultAsync();
                        if (warehouseFrom != null)
                        {
                            item.CurrentPlaceId = warehouseFrom.Id;
                        }

                        switch (item.Status)
                        {
                            case (int)StatusSmallPackage.VeKhoTQ: //Kiểm hàng kho TQ
                                if (item.DateInTQWarehouse == null)
                                    item.DateInTQWarehouse = item.DateScanTQ = currentDate;
                                item.StaffTQWarehouse = user.UserName;
                                item.IsPayment = false;

                                //Đơn mua hộ
                                if (mainOrder != null && mainOrder.Id != 0)
                                {
                                    mainOrder.OldStatus = mainOrder.Status;
                                    if (mainOrder.DateTQ == null)
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
                                    if (mainOrder.Status < (int)StatusOrderContants.VeTQ)
                                    {
                                        mainOrder.Status = (int)StatusOrderContants.VeTQ;
                                        historyOrderChanges.Add(new HistoryOrderChange()
                                        {
                                            MainOrderId = mainOrder.Id,
                                            UID = user.Id,
                                            HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng Id là: {1}, là \"Đã về kho TQ\"", user.UserName, mainOrder.Id),
                                            Type = (int)TypeHistoryOrderChange.MaVanDon
                                        });
                                    }

                                    mainOrderList.Add(mainOrder);
                                    mainOrderUpdateds.Add(mainOrderList.LastOrDefault());
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
                                }
                                //Đơn ký gửi
                                else
                                {
                                    if (transportationOrder == null || transportationOrder.Id == 0) break;
                                    if (transportationOrder.Status == (int)StatusGeneralTransportationOrder.ChoDuyet)
                                        throw new AppException("Đơn ký gửi chưa được duyệt");
                                    var transportationOrderOld = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transportationOrder.Id);
                                    if (transportationOrder.Status < (int)StatusGeneralTransportationOrder.VeKhoTQ)
                                        transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoTQ;
                                    if (transportationOrder.TQDate == null)
                                        transportationOrder.TQDate = DateTime.Now;
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

                                    historyOrderChanges.AddRange(CreateHistoryTransOrderScanWareHouse(user, transportationOrder, transportationOrderOld, item, oldItem));
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
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
                                        if (transportationOrder.Status != transportationOrderOld.Status)
                                        {
                                            var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiKyGui);
                                            sendNotificationService.SendNotification(notificationSetting,
                                                new List<string>() { transportationOrder.Id.ToString(), user.UserName, transportationOrderService.GetStatusName(transportationOrderOld.Status), transportationOrderService.GetStatusName(transportationOrder.Status) },
                                                new UserNotification() { UserId = transportationOrder.UID, SaleId = transportationOrder.SalerID });
                                        }
                                    }
                                    transportationOrderList.Add(transportationOrder);
                                }
                                break;
                            case (int)StatusSmallPackage.XuatKhoTQ: //Kiểm hàng kho TQ
                                if (item.DateOutTQ == null)
                                    item.DateOutTQ = currentDate;
                                item.StaffOutTQ = user.UserName;
                                item.IsPayment = false;

                                //Thông báo cho user đơn hàng đã đến kho TQ
                                var notificationSettingXuatTQ = await notificationSettingService.GetByIdAsync(8);
                                var notiTemplateUserXuatTQ = await notificationTemplateService.GetByIdAsync(20);
                                //Đơn mua hộ
                                if (mainOrder != null && mainOrder.Id != 0)
                                {
                                    mainOrder.OldStatus = mainOrder.Status;
                                    if (mainOrder.DateComingVN == null)
                                        mainOrder.DateComingVN = currentDate;

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
                                        HistoryContent = String.Format("{0} đã đổi trạng thái của mã vận đơn: {1} của đơn hàng ID: {2} là \"Xuất kho TQ\"", user.UserName, item.OrderTransactionCode, mainOrder.Id),
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
                                    if (mainOrder.Status < (int)StatusOrderContants.DangVeVN)
                                    {
                                        mainOrder.Status = (int)StatusOrderContants.DangVeVN;
                                        historyOrderChanges.Add(new HistoryOrderChange()
                                        {
                                            MainOrderId = mainOrder.Id,
                                            UID = user.Id,
                                            HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng Id là: {1}, là \"Đang về kho VN\"", user.UserName, mainOrder.Id),
                                            Type = (int)TypeHistoryOrderChange.MaVanDon
                                        });
                                    }


                                    mainOrderList.Add(mainOrder);
                                    mainOrderUpdateds.Add(mainOrderList.LastOrDefault());
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
                                }
                                //Đơn ký gửi
                                else
                                {
                                    if (transportationOrder == null || transportationOrder.Id == 0) break;
                                    if (transportationOrder.Status == (int)StatusGeneralTransportationOrder.ChoDuyet)
                                        throw new AppException("Đơn ký gửi chưa được duyệt");
                                    var transportationOrderOld = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transportationOrder.Id);
                                    if (transportationOrder.Status < (int)StatusGeneralTransportationOrder.DangVeVN)
                                        transportationOrder.Status = (int)StatusGeneralTransportationOrder.DangVeVN;
                                    if (transportationOrder.ComingVNDate == null)
                                        transportationOrder.ComingVNDate = DateTime.Now;
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
                                    historyOrderChanges.AddRange(CreateHistoryTransOrderScanWareHouse(user, transportationOrder, transportationOrderOld, item, oldItem));
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
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
                                        if (transportationOrder.Status != transportationOrderOld.Status)
                                        {
                                            var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiKyGui);
                                            sendNotificationService.SendNotification(notificationSetting,
                                                new List<string>() { transportationOrder.Id.ToString(), user.UserName, transportationOrderService.GetStatusName(transportationOrderOld.Status), transportationOrderService.GetStatusName(transportationOrder.Status) },
                                                new UserNotification() { UserId = transportationOrder.UID, SaleId = transportationOrder.SalerID });
                                        }
                                    }
                                    transportationOrderList.Add(transportationOrder);
                                }
                                break;
                            case (int)StatusSmallPackage.VeKhoVN: //Kiểm hàng kho VN
                                if (item.DateInLasteWareHouse == null)
                                    item.DateInLasteWareHouse = item.DateScanVN = currentDate;
                                item.StaffVNWarehouse = user.UserName;
                                item.IsPayment = false;
                                var warehouse = await unitOfWork.CatalogueRepository<Warehouse>().GetQueryable().Where(e => !e.Deleted && user.WarehouseTo == e.Id).FirstOrDefaultAsync();
                                if (warehouse != null)
                                {
                                    item.CurrentPlaceId = warehouse.Id;
                                }

                                //Thông báo cho user đơn hàng đã đến kho VN

                                if (mainOrder != null && mainOrder.Id != 0)
                                {
                                    mainOrder.OldStatus = mainOrder.Status;
                                    if (mainOrder.DateVN == null)
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
                                    mainOrder.Status = (int)StatusOrderContants.VeVN;
                                    historyOrderChanges.Add(new HistoryOrderChange()
                                    {
                                        MainOrderId = mainOrder.Id,
                                        UID = user.Id,
                                        HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng Id là: {1}, là \"Đã về kho VN\"", user.UserName, mainOrder.Id),
                                        Type = (int)TypeHistoryOrderChange.MaVanDon
                                    });

                                    //Detach
                                    mainOrderList.Add(mainOrder);
                                    mainOrderUpdateds.Add(mainOrderList.LastOrDefault());
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);

                                }
                                else
                                {
                                    if (transportationOrder == null || transportationOrder.Id == 0) break;
                                    var transportationOrderOld = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x=>x.Id == transportationOrder.Id);
                                    if (transportationOrder.Status < (int)StatusGeneralTransportationOrder.VeKhoVN)
                                        transportationOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoVN;
                                    if (transportationOrder.VNDate == null)
                                        transportationOrder.VNDate = DateTime.Now;
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
                                    historyOrderChanges.AddRange(CreateHistoryTransOrderScanWareHouse(user, transportationOrder, transportationOrderOld, item, oldItem));
                                    if (historyOrderChanges.Any())
                                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
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
                                        if (transportationOrder.Status != transportationOrderOld.Status)
                                        {
                                            var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiKyGui);
                                            sendNotificationService.SendNotification(notificationSetting,
                                                new List<string>() { transportationOrder.Id.ToString(), user.UserName, transportationOrderService.GetStatusName(transportationOrderOld.Status), transportationOrderService.GetStatusName(transportationOrder.Status) },
                                                new UserNotification() { UserId = transportationOrder.UID, SaleId = transportationOrder.SalerID });
                                        }
                                    }
                                    transportationOrderList.Add(transportationOrder);
                                }

                                //Kiêm tra nếu 1 mã vận đơn trong bao lớn đã về VN thì đổi trạng thái đã về VN
                                if (item.BigPackageId != null && item.BigPackageId > 0)
                                {
                                    var bigPackage = await unitOfWork.CatalogueRepository<BigPackage>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.BigPackageId);
                                    bigPackage.Status = (int)StatusBigPackage.TrongKhoVN;
                                    unitOfWork.Repository<BigPackage>().Update(bigPackage);
                                    await unitOfWork.SaveAsync();
                                    unitOfWork.Repository<BigPackage>().Detach(bigPackage);
                                }
                                break;
                            default:
                                break;
                        }
;
                        unitOfWork.Repository<SmallPackage>().Update(item);
                        await unitOfWork.SaveAsync();
                    }
                    foreach (var mainOrderUpdated in mainOrderUpdateds)
                    {
                        unitOfWork.Repository<MainOrder>().Update(mainOrderUpdated);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<MainOrder>().Detach(mainOrderUpdated);
                        if (mainOrderUpdated.Status != mainOrderUpdated.OldStatus)
                        {
                            var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiMuaHo);
                            sendNotificationService.SendNotification(notificationSetting,
                                new List<string>() { mainOrderUpdated.Id.ToString(), user.UserName, mainOrderService.GetStatusName(mainOrderUpdated.OldStatus), mainOrderService.GetStatusName(mainOrderUpdated.Status) },
                                new UserNotification() { UserId = mainOrderUpdated.UID, SaleId = mainOrderUpdated.SalerId, OrdererId = mainOrderUpdated.DatHangId });
                        }

                    }
                    await dbContextTransaction.CommitAsync();
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
                    && e.Status >= (int)StatusSmallPackage.VeKhoVN).ToListAsync();

                if (smallPackageNotBigPackage.Any())
                {
                    bigPackage.Status = (int)StatusBigPackage.TrongKhoVN;
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
        public virtual async Task<AppDomainImportResult> ImportTemplateFile(Stream stream, int? bigPackageId, string createdBy, int? type)
        {
            AppDomainImportResult appDomainImportResult = new AppDomainImportResult();
            DateTime currentDate = DateTime.Now;
            var currentUser = LoginContext.Instance.CurrentUser;
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
                var historyOrderChanges = new List<HistoryOrderChange>();
                var mainOrders = new List<MainOrder>();
                var transOrders = new List<TransportationOrder>();
                foreach (var catalogueMapper in catalogueMappers)
                {
                    string orderTransactionCode = catalogueMapper.OrderTransactionCode;
                    decimal weightOld = 0;
                    var smallPackage = await Queryable.Where(e => !e.Deleted && e.OrderTransactionCode.Equals(orderTransactionCode)).FirstOrDefaultAsync();
                    if (smallPackage != null)
                    {
                        weightOld = smallPackage.Weight ?? 0;
                        smallPackage.Weight = catalogueMapper.Weight;
                        smallPackage.BigPackageId = bigPackageId;
                        if (type == 2)
                        {
                            smallPackage.Status = (int)StatusSmallPackage.XuatKhoTQ;
                        }
                        else
                        {
                            smallPackage.Status = (int)StatusSmallPackage.VeKhoTQ;
                        }
                        smallPackage.DateInTQWarehouse = currentDate;
                        smallPackage.StaffTQWarehouse = currentUser.UserName;
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
                    else if (smallPackage == null)
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
                        if (type == 2)
                        {
                            smallPackage.Status = (int)StatusSmallPackage.XuatKhoTQ;
                        }
                        else
                        {
                            smallPackage.Status = (int)StatusSmallPackage.VeKhoTQ;
                        }
                        smallPackage.DateInTQWarehouse = currentDate;
                        smallPackage.StaffTQWarehouse = currentUser.UserName;
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
                            historyOrderChanges.Add(new HistoryOrderChange()
                            {
                                MainOrderId = mainOrder.Id,
                                UID = currentUser.UserId,
                                HistoryContent = String.Format("{0} đã đổi trạng thái của mã vận đơn: {1} của đơn hàng ID: {2} là \"Đã về kho TQ\"", currentUser.UserName, smallPackage.OrderTransactionCode, mainOrder.Id),
                                Type = (int)TypeHistoryOrderChange.MaVanDon
                            });
                            if (smallPackage.Weight != weightOld)
                            {
                                historyOrderChanges.Add(new HistoryOrderChange()
                                {
                                    MainOrderId = mainOrder.Id,
                                    UID = currentUser.UserId,
                                    HistoryContent = String.Format("{0} đã đổi cân nặng của mã vận đơn: {1} của đơn hàng ID: {2}, từ: {3}, sang: {4}", currentUser.UserName, smallPackage.OrderTransactionCode, mainOrder.Id, weightOld, smallPackage.Weight),
                                    Type = (int)TypeHistoryOrderChange.MaVanDon
                                });
                            }
                            if (!mainOrders.Contains(mainOrder))
                            {
                                mainOrders.Add(mainOrder);
                            }
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
                            if (!transOrders.Contains(transportationOrder))
                            {
                                transOrders.Add(transportationOrder);
                            }
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
                //Tính tiền đơn mua hộ
                if (mainOrders.Count > 0)
                {
                    foreach (var mainOrder in mainOrders)
                    {
                        historyOrderChanges.Add(new HistoryOrderChange()
                        {
                            MainOrderId = mainOrder.Id,
                            UID = currentUser?.UserId,
                            HistoryContent = String.Format("{0} đã đổi trạng thái của đơn hàng Id là: {1}, là \"Đã về kho TQ\"", currentUser.UserName, mainOrder.Id),
                            Type = (int)TypeHistoryOrderChange.MaVanDon
                        });
                        if (type == 2)
                        {
                            mainOrder.Status = (int)StatusOrderContants.DangVeVN;
                            if (mainOrder.DateComingVN == null)
                                mainOrder.DateComingVN = currentDate;
                        }
                        else
                        {
                            mainOrder.Status = (int)StatusOrderContants.VeTQ;
                            if (mainOrder.DateTQ == null)
                                mainOrder.DateTQ = currentDate;
                        }
                        var smallPackageNews = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.MainOrderId == mainOrder.Id).ToListAsync();
                        mainOrder.SmallPackages = smallPackageNews;
                        var mainOrderNew = await mainOrderService.PriceAdjustment(mainOrder);
                        unitOfWork.Repository<MainOrder>().Update(mainOrderNew);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<MainOrder>().Detach(mainOrderNew);
                    }
                    if (historyOrderChanges.Any())
                        await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(historyOrderChanges);
                }
                //Tính tiền đơn ký gửi
                if (transOrders.Count > 0)
                {
                    foreach (var transOrder in transOrders)
                    {
                        if (type == 2)
                        {
                            if (transOrder.ComingVNDate == null)
                                transOrder.ComingVNDate = currentDate;
                            transOrder.Status = (int)StatusGeneralTransportationOrder.DangVeVN;
                        }
                        else
                        {
                            if (transOrder.TQDate == null)
                                transOrder.TQDate = currentDate;
                            transOrder.Status = (int)StatusGeneralTransportationOrder.VeKhoTQ;
                        }
                        var smallPackageNews = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.TransportationOrderId == transOrder.Id).ToListAsync();
                        transOrder.SmallPackages = smallPackageNews;
                        var transOrderNew = await transportationOrderService.PriceAdjustment(transOrder);
                        unitOfWork.Repository<TransportationOrder>().Update(transOrderNew);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<TransportationOrder>().Detach(transOrderNew);
                    }
                    //Lịch sử ký gửi
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
            return await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.MainOrderId == mainOrderId && x.Status == (int)StatusSmallPackage.VeKhoVN).ToListAsync();
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
                            && (x.UID == user.Id && x.Id == OrderID) && x.Status >= (int)StatusOrderContants.VeVN
                        ); //getById
                    else
                        mainOrders = await mainOrderService.GetAsync(x => !x.Deleted && x.Active
                            && (x.UID == user.Id) && x.Status >= (int)StatusOrderContants.VeVN
                        );

                    if (!mainOrders.Any())
                        throw new KeyNotFoundException("MainOrder không tìm thấy");

                    foreach (var mainOrder in mainOrders)
                    {
                        if (OrderID > 0)
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.MainOrderId == mainOrder.Id && x.Status == (int)StatusSmallPackage.VeKhoVN)
                            );
                        else
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.MainOrderId == mainOrder.Id && x.Status == (int)StatusSmallPackage.VeKhoVN)
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
                        transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id && e.Id == OrderID && e.Status >= (int)StatusGeneralTransportationOrder.VeKhoVN).ToListAsync();
                    else
                        transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id && e.Status >= (int)StatusGeneralTransportationOrder.VeKhoVN).ToListAsync();

                    foreach (var transportationOrder in transportationOrders)
                    {
                        if (OrderID > 0)
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.TransportationOrderId == transportationOrder.Id && x.Status == (int)StatusSmallPackage.VeKhoVN)
                            );
                        else
                            smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                && (x.TransportationOrderId == transportationOrder.Id && x.Status == (int)StatusSmallPackage.VeKhoVN)
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
                            && (x.UID == user.Id && x.Id == OrderID) && x.Status >= (int)StatusOrderContants.VeVN
                        ); //getById

                        if (mainOrders.Any()) //Có tồn tại
                        {
                            foreach (var mainOrder in mainOrders)
                            {
                                smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                    && (x.MainOrderId == mainOrder.Id && x.Status == (int)StatusSmallPackage.VeKhoVN)
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
                            transportationOrders = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.UID == user.Id && e.Id == OrderID && e.Status >= (int)StatusGeneralTransportationOrder.VeKhoVN).ToListAsync();

                            foreach (var transportationOrder in transportationOrders)
                            {
                                smallPackages = await this.GetAsync(x => !x.Deleted && x.Active
                                    && (x.TransportationOrderId == transportationOrder.Id) && x.Status == (int)StatusSmallPackage.VeKhoVN
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

                            && (x.UID == user.Id && x.Status == (int)StatusSmallPackage.VeKhoVN)
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

        public string GetStatusName(int? status)
        {
            switch (status)
            {
                case (int)StatusSmallPackage.DaHuy:
                    return "Đã hủy";
                case (int)StatusSmallPackage.MoiTao:
                    return "Mới tạo";
                case (int)StatusSmallPackage.VeKhoTQ:
                    return "Đã về kho TQ";
                case (int)StatusSmallPackage.XuatKhoTQ:
                    return "Xuất kho TQ";
                case (int)StatusSmallPackage.VeKhoVN:
                    return "Đã về kho VN";
                case (int)StatusSmallPackage.DaGiao:
                    return "Đã giao";
                default:
                    return string.Empty;
            }
        }

        public List<HistoryOrderChange> CreateHistoryTransOrderScanWareHouse(Users user, TransportationOrder transportationOrder, TransportationOrder transportationOrderOld, SmallPackage item, SmallPackage oldItem)
        {
            var historyOrderChanges = new List<HistoryOrderChange>();
            if (oldItem.Status != item.Status)
            {
                historyOrderChanges.Add(new HistoryOrderChange()
                {
                    TransportationOrderId = transportationOrder.Id,
                    UID = user.Id,
                    HistoryContent = $"{user.UserName} đã đổi trạng thái của mã vận đơn: {item.OrderTransactionCode} của đơn hàng ID: {transportationOrder.Id} từ {GetStatusName(oldItem.Status)} sang {GetStatusName(item.Status)}",
                    Type = (int)TypeHistoryOrderChange.MaVanDon
                });
            }
            if (item.Weight != oldItem.Weight)
            {
                historyOrderChanges.Add(new HistoryOrderChange()
                {
                    TransportationOrderId = transportationOrder.Id,
                    UID = user.Id,
                    HistoryContent = $"{user.UserName} đã đổi cân nặng của mã vận đơn: {item.OrderTransactionCode} của đơn hàng ID: {transportationOrder.Id}, " +
                    $"từ: {oldItem.Weight}, sang: {item.PayableWeight}",
                    Type = (int)TypeHistoryOrderChange.MaVanDon
                });
            }
            if (item.VolumePayment != oldItem.VolumePayment)
            {
                historyOrderChanges.Add(new HistoryOrderChange()
                {
                    TransportationOrderId = transportationOrder.Id,
                    UID = user.Id,
                    HistoryContent = $"{user.UserName} đã đổi số khối của mã vận đơn: {item.OrderTransactionCode} của đơn hàng ID: {transportationOrder.Id}, " +
                    $"từ: {oldItem.VolumePayment}, sang: {item.VolumePayment}",
                    Type = (int)TypeHistoryOrderChange.MaVanDon
                });
            }
            if (item.Status != oldItem.Status)
            {
                historyOrderChanges.Add(new HistoryOrderChange()
                {
                    TransportationOrderId = transportationOrder.Id,
                    UID = user.Id,
                    HistoryContent = $"{user.UserName} đã đổi trạng thái của đơn hàng ID: {transportationOrder.Id} từ {transportationOrderService.GetStatusName(transportationOrderOld.Status)} sang {transportationOrderService.GetStatusName(transportationOrder.Status)}",
                    Type = (int)TypeHistoryOrderChange.MaVanDon
                });
            }
            return historyOrderChanges;
        }
    }
}
