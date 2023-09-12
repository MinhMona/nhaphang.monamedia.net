using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Request;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class ExportRequestTurnService : DomainService<ExportRequestTurn, ExportRequestTurnSearch>, IExportRequestTurnService
    {
        protected readonly IAppDbContext Context;
        protected readonly IUserService userService;
        protected readonly ITransportationOrderService transportationOrderService;
        public ExportRequestTurnService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            userService = serviceProvider.GetRequiredService<IUserService>();
            transportationOrderService = serviceProvider.GetRequiredService<ITransportationOrderService>();
        }

        protected override string GetStoreProcName()
        {
            return "ExportRequestTurn_GetPagingData";
        }

        //public async Task<bool> Payment(ExportRequestTurn entity)
        //{
        //    DateTime currentDate = DateTime.Now;
        //    string userName = LoginContext.Instance.CurrentUser.UserName;
        //    var users = await userService.GetByIdAsync(entity.UID ?? 0); //Vì admin có thể thanh toán cho tài khoản -> lấy id theo đơn
        //    var wallet = users.Wallet;

        //    entity.Updated = currentDate;
        //    entity.UpdatedBy = userName;

        //    if (wallet < entity.TotalPriceVND)
        //        throw new AppException("Không đủ tiền trong tài khoản! Vui lòng nạp thêm tiền");

        //    using (var dbContextTransaction = Context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            users.Wallet -= entity.TotalPriceVND;
        //            users.Updated = currentDate;
        //            users.UpdatedBy = users.UserName;
        //            unitOfWork.Repository<Users>().Update(users);

        //            //Lịch sử của ví
        //            await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
        //            {
        //                UID = users.Id,
        //                MainOrderId = 0,
        //                Amount = entity.TotalPriceVND,
        //                Content = string.Format("{0} đã thanh toán đơn hàng vận chuyển hộ.", users.UserName),
        //                MoneyLeft = users.Wallet,
        //                Type = (int?)DauCongVaTru.Tru,
        //                TradeType = (int?)HistoryPayWalletContents.ThanhToanVanChuyenHo,
        //                Deleted = false,
        //                Active = true,
        //                Created = currentDate,
        //                CreatedBy = users.UserName
        //            });

        //            await unitOfWork.SaveAsync();
        //            await dbContextTransaction.CommitAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            await dbContextTransaction.RollbackAsync();
        //            throw new Exception(ex.Message);
        //        }
        //    }

        //    return true;
        //}

        public async Task<bool> UpdateStatus(int id, int status, bool isPaymentWallet)
        {
            DateTime currentDate = DateTime.Now;
            string userName = LoginContext.Instance.CurrentUser.UserName;

            var item = await this.GetByIdAsync(id);
            if (item == null)
                throw new KeyNotFoundException("Item không tồn tại");
            var users = await userService.GetByIdAsync(item.UID ?? 0);

            item.Updated = currentDate;
            item.UpdatedBy = userName;
            item.Status = status;

            switch (status)
            {
                case (int)ExportRequestTurnStatus.DaThanhToan:

                    if (item.TotalPriceVND <= 0)
                        break;

                    var wallet = users.Wallet;

                    if (isPaymentWallet) 
                    {
                        item.Type = 1; //Thanh toán bằng ví

                        if (wallet < item.TotalPriceVND)
                            throw new AppException("Không đủ tiền trong tài khoản! Vui lòng nạp thêm tiền");
                    }
                    else
                    {
                        item.Type = 2; //Thanh toán trực tiếp (nạp tiền vô rồi trừ :))))

                        users.Wallet += item.TotalPriceVND;

                        await unitOfWork.Repository<AdminSendUserWallet>().CreateAsync(new AdminSendUserWallet()
                        {
                            UID = item.UID,
                            Amount = item.TotalPriceVND,
                            Status = (int)WalletStatus.DaDuyet,
                            BankId = 100, //Đặt đại
                            TradeContent = string.Format("{0} đã được nạp tiền vào tài khoản", users.UserName),
                            Deleted = false,
                            Active = true,
                            Created = currentDate,
                            CreatedBy = userName
                        });

                        await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                        {
                            UID = users.Id,
                            MainOrderId = 0,
                            Amount = item.TotalPriceVND,
                            Content = string.Format("{0} đã được nạp tiền vào tài khoản.", users.UserName),
                            MoneyLeft = users.Wallet,
                            Type = (int?)DauCongVaTru.Cong,
                            TradeType = (int?)HistoryPayWalletContents.NapTien,
                            Deleted = false,
                            Active = true,
                            Created = currentDate,
                            CreatedBy = users.UserName
                        });
                    }


                    users.Wallet -= item.TotalPriceVND;
                    users.Updated = currentDate;
                    users.UpdatedBy = users.UserName;
                    unitOfWork.Repository<Users>().Update(users);
                    //await unitOfWork.Repository<Users>().UpdateFieldsSaveAsync(users, new Expression<Func<Users, object>>[]
                    //{
                    //        e => e.Updated,
                    //        e => e.UpdatedBy,
                    //        e => e.Wallet
                    //});

                    //Lịch sử của ví
                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                    {
                        UID = users.Id,
                        MainOrderId = 0,
                        Amount = item.TotalPriceVND,
                        Content = string.Format("{0} đã thanh toán đơn hàng vận chuyển hộ.", users.UserName),
                        MoneyLeft = users.Wallet,
                        Type = (int?)DauCongVaTru.Tru,
                        TradeType = (int?)HistoryPayWalletContents.ThanhToanKyGui,
                        Deleted = false,
                        Active = true,
                        Created = currentDate,
                        CreatedBy = users.UserName
                    });

                    unitOfWork.Repository<ExportRequestTurn>().Update(item);

                    //await unitOfWork.Repository<ExportRequestTurn>().UpdateFieldsSaveAsync(item, new Expression<Func<ExportRequestTurn, object>>[]
                    //{
                    //        e => e.Updated,
                    //        e => e.UpdatedBy,
                    //        e => e.Status,
                    //        e => e.Type
                    //});

                    break;
                case (int)ExportRequestTurnStatus.Huy: //Là xóa luôn :)))

                    var req = await unitOfWork.Repository<RequestOutStock>().GetQueryable().Where(e => !e.Deleted && e.Active && (e.ExportRequestTurnId == item.Id)).ToListAsync();

                    if (req.Any())
                    {
                        foreach (var jtem in req)
                        {
                            var smallPackage = unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => x.Id == jtem.SmallPackageId).FirstOrDefault();
                            if (smallPackage != null)
                            {
                                smallPackage.Updated = currentDate;
                                smallPackage.UpdatedBy = users.UserName;
                                smallPackage.Status = (int)StatusSmallPackage.VeKhoTQ;
                                unitOfWork.Repository<SmallPackage>().Update(smallPackage);

                                //await unitOfWork.Repository<SmallPackage>().UpdateFieldsSaveAsync(smallPackage, new Expression<Func<SmallPackage, object>>[]
                                //{
                                //    e => e.Updated,
                                //    e => e.UpdatedBy,
                                //    e => e.Status
                                //});

                            }

                            var trans = await transportationOrderService.GetByIdAsync(smallPackage.TransportationOrderId ?? 0);
                            if (trans != null)
                            {
                                trans.Updated = currentDate;
                                trans.UpdatedBy = users.UserName;
                                trans.Status = (int)StatusGeneralTransportationOrder.VeKhoVN;
                                unitOfWork.Repository<TransportationOrder>().Update(trans);

                                //await unitOfWork.Repository<TransportationOrder>().UpdateFieldsSaveAsync(trans, new Expression<Func<TransportationOrder, object>>[]
                                //{
                                //    e => e.Updated,
                                //    e => e.UpdatedBy,
                                //    e => e.Status
                                //});
                            }

                            unitOfWork.Repository<RequestOutStock>().Delete(jtem); //Xóa luôn nè
                        }
                    }

                    unitOfWork.Repository<ExportRequestTurn>().Delete(item); //Xóa luôn nè
                    break;
                default:
                    break;
            }
            await unitOfWork.SaveAsync();
            return true;
        }

        public override async Task<ExportRequestTurn> GetByIdAsync(int id)
        {
            var exportRequestTurn = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (exportRequestTurn == null) return null;

            var requestOutStocks = await unitOfWork.Repository<RequestOutStock>().GetQueryable().Where(e => !e.Deleted && e.ExportRequestTurnId == exportRequestTurn.Id).OrderByDescending(o => o.Id).ToListAsync();
            if (!requestOutStocks.Any()) return null;

            var user = await userService.GetByIdAsync(exportRequestTurn.UID ?? 0);
            if (user != null) exportRequestTurn.UserName = user.UserName;

            foreach (var requestOutStock in requestOutStocks)
            {
                var smallPackage = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(e => !e.Deleted && e.Id == requestOutStock.SmallPackageId).FirstOrDefaultAsync();
                if (smallPackage == null) continue;
                exportRequestTurn.SmallPackages.Add(smallPackage);
                var transportationOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();
                if (transportationOrder == null) continue;
                exportRequestTurn.TransportationOrders.Add(transportationOrder);
            }

            return exportRequestTurn;
        }

        public override async Task<bool> CreateAsync(ExportRequestTurn item)
        {
            //Giống với Thanh toán (TransportationOrder/UpdateAsync)
            DateTime currentDate = DateTime.Now;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);

                    if (!user.IsAdmin && (user.UserGroupId != (int)PermissionTypes.Admin
                        || user.UserGroupId != (int)PermissionTypes.Manager
                        || user.UserGroupId != (int)PermissionTypes.VietNamWarehouseManager))
                        throw new InvalidCastException("Không có quyền gán đơn");

                    if (item.SmallPackageIds == null || item.SmallPackageIds.Count == 0)
                        throw new KeyNotFoundException("Không tìm thấy các kiện");

                    var oldMainOrders = new List<int>();
                    var oldTranss = new List<int>();

                    foreach (var id in item.SmallPackageIds)
                    {
                        var smallPackage = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(e => !e.Deleted && e.Id == id).FirstOrDefaultAsync();
                        if (smallPackage == null)
                            throw new KeyNotFoundException("Có kiện không tồn tại hoặc đã bị xóa");

                        var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.MainOrderId).FirstOrDefaultAsync();
                        var trans = await unitOfWork.Repository<TransportationOrder>().GetQueryable().Where(e => !e.Deleted && e.Id == smallPackage.TransportationOrderId).FirstOrDefaultAsync();

                        if (mainOrder != null && trans != null)
                            throw new KeyNotFoundException("Đơn có vấn đề");

                        //Tính phí
                        var info = await transportationOrderService.GetBillingInfo(new List<int>() { trans.Id }, true);

                        if (mainOrder != null)
                        {

                        }
                        else if (trans != null)
                        {
                            if (!oldTranss.Contains(trans.Id)) //Tránh tình trạng bị lỗi bị cập nhật 2 lần trên cùng 1 phiên (BeginTransaction)
                            {
                                var userTrans = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Id == trans.UID).FirstOrDefaultAsync();
                                if (userTrans == null)
                                    throw new KeyNotFoundException("Không tìm thấy Users");

                                item.UID = userTrans.Id;
                                item.TotalPriceVND = info.TotalPriceVND;
                                item.TotalPriceCNY = info.TotalPriceCNY;
                                item.TotalWeight = info.TotalWeight;
                                item.TotalPackage = item.SmallPackageIds.Count();
                                //Thanh toán bằng ví: Type = 2, Status = 2
                                //Thanh toán tại kho: Type = 0, Status 1
                                item.Status = 1;
                                item.Type = 0; //0: Chưa thanh toán, 1: Thanh toán bằng ví, 2: Thanh toán trực tiếp (ở đây là 2 0)
                                item.OutStockDate = currentDate; //Ngày xuất kho

                                await unitOfWork.Repository<ExportRequestTurn>().CreateAsync(item);
                                await unitOfWork.SaveAsync();

                                if (smallPackage == null && smallPackage.Status != 3) //Cái này chưa hiểu
                                    continue;
                                var check = await unitOfWork.Repository<RequestOutStock>().GetQueryable().Where(e => !e.Deleted && e.SmallPackageId == smallPackage.Id).FirstOrDefaultAsync();
                                if (check != null) //Có tồn tại rồi thì thôi
                                    continue;

                                smallPackage.DateOutWarehouse = currentDate;

                                await unitOfWork.Repository<RequestOutStock>().CreateAsync(new RequestOutStock
                                {
                                    SmallPackageId = smallPackage.Id,
                                    Status = 1,
                                    ExportRequestTurnId = item.Id
                                });

                                trans.TotalPriceVND = info.TotalPriceVND;
                                trans.Status = (int)StatusGeneralTransportationOrder.DaThanhToan;
                                trans.ExportRequestNote = item.Note;
                                trans.DateExportRequest = currentDate;
                                trans.ShippingTypeVN = item.ShippingTypeInVNId;

                                unitOfWork.Repository<TransportationOrder>().Update(trans);
                                oldTranss.Add(trans.Id);

                                unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                            }
                        }
                        else
                            throw new AppException("Kiện trôi nổi không thể xuất kho");
                    }

                    await unitOfWork.SaveAsync();
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

        public async Task<bool> Export(int id, List<int> smallPackageIds, bool isRequest)
        {
            DateTime currentDate = DateTime.Now;

            var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
            if (!user.IsAdmin && (user.UserGroupId != (int)PermissionTypes.Admin
                || user.UserGroupId != (int)PermissionTypes.Manager
                || user.UserGroupId != (int)PermissionTypes.VietNamWarehouseManager))
                throw new InvalidCastException("Không có quyền xuất kho");

            if (isRequest) //Xuất kho đã yêu cầu
            {
                var requestOutStocks = await unitOfWork.Repository<RequestOutStock>().GetQueryable().Where(e => !e.Deleted && e.Active && (smallPackageIds.Contains(e.SmallPackageId??0))).ToListAsync();
                if (!requestOutStocks.Any()) throw new KeyNotFoundException("Không tìm thấy item");
                foreach (var requestOutStock in requestOutStocks)
                {
                    var exportRequestTurn = await this.GetByIdAsync(requestOutStock.ExportRequestTurnId ?? 0);
                    if (exportRequestTurn == null) continue;

                    var smallPackage = exportRequestTurn.SmallPackages.Where(e => smallPackageIds.Contains(e.Id)).FirstOrDefault();
                    if (smallPackage == null) continue;

                    var tran = exportRequestTurn.TransportationOrders.Where(e => smallPackageIds.Contains(e.SmallPackageId ?? 0)).FirstOrDefault();
                    if (tran == null) continue;

                    //Cập nhật
                    smallPackage.Status = (int)StatusSmallPackage.DaGiao;
                    tran.Status = (int)StatusGeneralTransportationOrder.DaHoanThanh;
                    smallPackage.DateOutWarehouse = tran.DateExport = currentDate;

                    var checkUpdateReq = exportRequestTurn.SmallPackages.Where(e => e.Status < (int)StatusSmallPackage.DaGiao).ToList();

                    if (!checkUpdateReq.Any()) requestOutStock.Status = 2; //Đã xuất

                    unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                    unitOfWork.Repository<TransportationOrder>().Update(tran);
                    unitOfWork.Repository<RequestOutStock>().Update(requestOutStock);

                    await unitOfWork.SaveAsync(); //Thôi lưu lại luôn đi
                }
            }
            else //Xuất kho chỉ với 1 đơn
            {
                var item = await this.GetByIdAsync(id);
                if (item == null)
                    throw new KeyNotFoundException("Không tìm thấy item");

                foreach (var smallPackage in item.SmallPackages)
                {
                    var req = await unitOfWork.Repository<RequestOutStock>().GetQueryable().Where(e => !e.Deleted && e.SmallPackageId == smallPackage.Id).FirstOrDefaultAsync();
                    if (req == null)
                        throw new KeyNotFoundException("Không tìm thấy chi tiết đơn xuất kho");
                    var tran = item.TransportationOrders.Where(e => e.SmallPackageId == smallPackage.Id).FirstOrDefault();

                    //Cập nhật
                    smallPackage.Status = (int)StatusSmallPackage.DaGiao;
                    tran.Status = (int)StatusGeneralTransportationOrder.DaHoanThanh;
                    smallPackage.DateOutWarehouse = tran.DateExport = currentDate;

                    req.Status = 2; //Đã xuất

                    unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                    unitOfWork.Repository<TransportationOrder>().Update(tran);
                    unitOfWork.Repository<RequestOutStock>().Update(req);

                    await unitOfWork.SaveAsync(); //Thôi lưu lại đi chớ mệt quá
                }
            }

            return true;
        }
    }
}
