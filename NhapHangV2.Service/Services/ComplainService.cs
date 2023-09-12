using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class ComplainService : DomainService<Complain, ComplainSearch>, IComplainService
    {
        protected readonly IAppDbContext Context;
        protected readonly IUserService userService;
        protected readonly IMainOrderService mainOrderService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly ISendNotificationService sendNotificationService;


        public ComplainService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            userService = serviceProvider.GetRequiredService<IUserService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
        }


        public override async Task<bool> CreateAsync(Complain item)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    string orderType = "";
                    if (item.MainOrderId > 0)
                    {
                        var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.MainOrderId);
                        mainOrder.Status = (int)StatusOrderContants.KhieuNai;
                        mainOrder.IsComplain = true;
                        if (mainOrder.ComplainDate == null)
                            mainOrder.ComplainDate = DateTime.Now;
                        await unitOfWork.Repository<MainOrder>().UpdateFieldsSaveAsync(mainOrder, new Expression<Func<MainOrder, object>>[]
                        {
                            x=>x.Status,
                            x=>x.IsComplain,
                            x=>x.ComplainDate,
                            x=>x.Updated,
                            x=>x.UpdatedBy,
                        });
                        if (mainOrder.OrderType != 1)
                            orderType = $"mua hộ khác {item.MainOrderId}";
                        else
                            orderType = $"mua hộ {item.MainOrderId}";
                    }
                    else if (item.TransportationOrderId > 0)
                    {
                        var transOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.TransportationOrderId);
                        transOrder.Status = (int)StatusGeneralTransportationOrder.DaKhieuNai;
                        transOrder.ComplainDate = DateTime.Now;
                        await unitOfWork.Repository<TransportationOrder>().UpdateFieldsSaveAsync(transOrder, new Expression<Func<TransportationOrder, object>>[]
                        {
                            x=>x.Status,
                            x=>x.ComplainDate,
                            x=>x.Updated,
                            x=>x.UpdatedBy,
                        });
                        orderType = $"ký gửi {item.TransportationOrderId}";
                    }
                    await unitOfWork.Repository<Complain>().CreateAsync(item);
                    await unitOfWork.SaveAsync();

                    var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiKhieuNai);
                    sendNotificationService.SendNotification(notificationSetting,
                        new List<string>() { item.Id.ToString(), orderType },
                        new UserNotification());

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

        public override async Task<Complain> GetByIdAsync(int id)
        {
            var complain = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (complain == null)
                return null;
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == complain.UID).FirstOrDefaultAsync();
            if (user != null)
                complain.UserName = user.UserName;

            var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(x => x.Id == complain.MainOrderId).FirstOrDefaultAsync();
            if (mainOrder != null)
                complain.CurrentCNYVN = mainOrder.CurrentCNYVN;
            return complain;
        }

        protected override string GetStoreProcName()
        {
            return "Complain_GetPagingData";
        }

        public async Task<bool> UpdateStatus(int id, decimal amount, int status)
        {
            DateTime currentDate = DateTime.Now;
            string userName = LoginContext.Instance.CurrentUser.UserName;

            var item = await this.GetByIdAsync(id);
            if (item == null)
                throw new KeyNotFoundException("Item không tồn tại");
            var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.MainOrderId);
            var transOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.TransportationOrderId);
            if (mainOrder == null && transOrder == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng");
            }
            var users = await userService.GetByIdAsync(item.UID ?? 0);
            item.Updated = currentDate;
            item.UpdatedBy = userName;
            item.Amount = amount;
            string newStatus = GetStatusName(status);
            string oldStatus = GetStatusName(item.Status);
            switch (status)
            {
                case (int)StatusComplain.DaHuy:
                    mainOrder.IsComplain = false;
                    mainOrder.Status = (int)StatusOrderContants.HoanThanh;
                    unitOfWork.Repository<MainOrder>().UpdateFieldsSave(mainOrder, new Expression<Func<MainOrder, object>>[]
                    {
                            e => e.IsComplain,
                            e => e.Status
                    });
                    break;
                case (int)StatusComplain.HoanThanh:
                    decimal? wallet = users.Wallet + amount;
                    //Cập nhật cho account
                    users.Wallet = wallet;
                    users.Updated = currentDate;
                    users.UpdatedBy = users.UserName;
                    unitOfWork.Repository<Users>().Update(users);
                    //Lịch sử ví tiền
                    int tradeType = (int)HistoryPayWalletContents.HoanTienKhieuNaiMuaHo;
                    string content = string.Format("{0} đã được hoàn tiền khiếu nại của đơn mua hộ: {1} vào tài khoản.", userName, item.MainOrderId);
                    if (mainOrder != null)
                    {
                        mainOrder.IsComplain = false;
                        mainOrder.Status = (int)StatusOrderContants.HoanThanh;
                        unitOfWork.Repository<MainOrder>().UpdateFieldsSave(mainOrder, new Expression<Func<MainOrder, object>>[]
                        {
                            e => e.IsComplain,
                            e => e.Status
                        });
                    }
                    else if (transOrder != null)
                    {
                        transOrder.Status = (int)StatusGeneralTransportationOrder.DaHoanThanh;
                        unitOfWork.Repository<TransportationOrder>().UpdateFieldsSave(transOrder, new Expression<Func<TransportationOrder, object>>[]
                        {
                            e => e.Status
                        });
                        tradeType = (int)HistoryPayWalletContents.HoanTienKhieuNaiKyGui;
                        content = string.Format("{0} đã được hoàn tiền khiếu nại của đơn ký gửi: {1} vào tài khoản.", userName, item.TransportationOrderId);

                    }
                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                    {
                        UID = users.Id,
                        MainOrderId = item.MainOrderId,
                        MoneyLeft = wallet,
                        Amount = amount,
                        Type = (int)DauCongVaTru.Cong,
                        TradeType = tradeType,
                        Content = content,
                        Deleted = false,
                        Active = true,
                        Created = currentDate,
                        CreatedBy = userName
                    });
                    break;
                default:
                    break;
            }
            item.Status = status;
            unitOfWork.Repository<Complain>().UpdateFieldsSave(item, new Expression<Func<Complain, object>>[]
            {
                e => e.Updated,
                e => e.UpdatedBy,
                e => e.Status,
                e => e.Amount
            });

            var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiKhieuNai);
            sendNotificationService.SendNotification(notificationSetting,
                new List<string>() { item.Id.ToString(), userName, oldStatus, newStatus },
                new UserNotification() { UserId = item.UID });

            await unitOfWork.SaveAsync();
            return true;
        }

        public string GetStatusName(int? status)
        {
            switch (status)
            {
                case (int)StatusComplain.DaHuy:
                    return "Đã hủy";
                case (int)StatusComplain.MoiTao:
                    return "Mới tạo";
                case (int)StatusComplain.DaXacNhan:
                    return "Đã xác nhận";
                case (int)StatusComplain.DangXuLy:
                    return "Đang xử lý";
                case (int)StatusComplain.HoanThanh:
                    return "Hoàn thành";
                default:
                    return "Không xác định";
            }
        }
    }
}
