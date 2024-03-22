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
using System.Linq.Expressions;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class ShipRequestService : DomainService<ShipRequest, ShipRequestSearch>, IShipRequestService
    {
        protected readonly IAppDbContext Context;
        protected readonly IMainOrderService mainOrderService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly ISendNotificationService sendNotificationService;


        public ShipRequestService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
        }

        public override async Task<bool> CreateAsync(ShipRequest item)
        {
            var currentDate = DateTime.Now;
            var currentUser = LoginContext.Instance.CurrentUser;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    item.Status = (int)RequestShipStatus.UnAccept;
                    string mainOrderIdString = "";
                    string transOrderIdString = "";
                    foreach (var mainOrderId in item.MainOrderIds)
                    {
                        var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == mainOrderId && x.UID == item.UID && x.IsShipRequest != true);
                        if (mainOrder == null)
                        {
                            continue;
                        }
                        mainOrderIdString += $"{mainOrderId} | ";
                        mainOrder.IsShipRequest = true;
                        if (mainOrder.DateShipRequest == null)
                        {
                            mainOrder.DateShipRequest = currentDate;
                        }
                        await unitOfWork.Repository<MainOrder>().UpdateFieldsSaveAsync(mainOrder, new Expression<Func<MainOrder, object>>[]
                        {
                            x => x.IsShipRequest,
                            x => x.DateShipRequest,
                            x => x.Updated,
                            x => x.UpdatedBy
                        });
                    }

                    foreach (var transOrderId in item.TransportationIds)
                    {
                        var transOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transOrderId && x.UID == item.UID && x.IsShipRequest != true);
                        if (transOrder == null)
                        {
                            continue;
                        }
                        transOrderIdString += $"{transOrderId} | ";
                        transOrder.IsShipRequest = true;
                        if (transOrder.DateShipRequest == null)
                        {
                            transOrder.DateShipRequest = currentDate;
                        }

                        await unitOfWork.Repository<TransportationOrder>().UpdateFieldsSaveAsync(transOrder, new Expression<Func<TransportationOrder, object>>[]
                        {
                            x => x.IsShipRequest,
                            x => x.DateShipRequest,
                            x => x.Updated,
                            x => x.UpdatedBy
                        });
                    }

                    if (string.IsNullOrEmpty(mainOrderIdString) && string.IsNullOrEmpty(transOrderIdString))
                    {
                        throw new AppException("Đơn hàng đã được yêu cầu");
                    }
                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                    item.MainOrderId = mainOrderIdString;
                    item.TransportrationOrderId = transOrderIdString;
                }
                catch (AppException ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new AppException(ex.Message);
                }
            }
            var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.YeuCauGiaoMoi);
            sendNotificationService.SendNotification(notificationSetting,
                new List<string>() { currentUser.UserName },
                new UserNotification());
            return await base.CreateAsync(item);
        }

        public override async Task<bool> UpdateAsync(ShipRequest item)
        {
            var currentUser = LoginContext.Instance.CurrentUser;
            var oldItem = await unitOfWork.Repository<ShipRequest>().GetQueryable().FirstOrDefaultAsync(x => x.Id == item.Id);
            if (oldItem == null)
            {
                return true;
            }

            var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.TrangThaiYeuCauGiao);
            sendNotificationService.SendNotification(notificationSetting,
                new List<string>() { item.Id.ToString(), currentUser.UserName, GetStatusName(oldItem.Status), GetStatusName(item.Status) },
                new UserNotification() { UserId = item.UID });

            await unitOfWork.Repository<ShipRequest>().UpdateFieldsSaveAsync(item, new Expression<Func<ShipRequest, object>>[]
            {
                x => x.Status,
                x => x.Updated,
                x => x.UpdatedBy
            });
            if (item.Status == (int)RequestShipStatus.Accept || item.Status == (int)RequestShipStatus.Cancel)
            {
                if (!string.IsNullOrEmpty(item.MainOrderId))
                {
                    string[] mainOrderIds = item.MainOrderId.Split('|');
                    foreach (string orderId in mainOrderIds)
                    {
                        try
                        {
                            int mainOrderId = int.Parse(orderId);
                            var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == mainOrderId && x.UID == item.UID && x.IsShipRequest == true);
                            if (mainOrder == null)
                            {
                                continue;
                            }
                            mainOrder.IsShipRequest = false;
                            await unitOfWork.Repository<MainOrder>().UpdateFieldsSaveAsync(mainOrder, new Expression<Func<MainOrder, object>>[]
                            {
                                x => x.IsShipRequest,
                                x => x.DateShipRequest,
                                x => x.Updated,
                                x => x.UpdatedBy
                            });
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(item.TransportrationOrderId))
                {
                    string[] tranOrderIds = item.TransportrationOrderId.Split('|');
                    foreach (string orderId in tranOrderIds)
                    {
                        try
                        {
                            int transOrderId = int.Parse(orderId);
                            var transOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == transOrderId && x.UID == item.UID && x.IsShipRequest == true);
                            if (transOrder == null)
                            {
                                continue;
                            }
                            transOrder.IsShipRequest = false;
                            await unitOfWork.Repository<TransportationOrder>().UpdateFieldsSaveAsync(transOrder, new Expression<Func<TransportationOrder, object>>[]
                            {
                                x => x.IsShipRequest,
                                x => x.DateShipRequest,
                                x => x.Updated,
                                x => x.UpdatedBy
                            });
                        }

                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            await unitOfWork.SaveAsync();
            return true;
        }
        protected override string GetStoreProcName()
        {
            return "ShipRequest_GetPagingData";
        }
        public string GetStatusName(int? status)
        {
            switch (status)
            {
                case (int)RequestShipStatus.Cancel:
                    return "Đã hủy";
                case (int)RequestShipStatus.UnAccept:
                    return "Chờ duyệt";
                case (int)RequestShipStatus.Accept:
                    return "Đã duyệt";
                default:
                    return "Không xác định";
            }
        }
    }

}
