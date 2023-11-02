using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class StaffIncomeService : DomainService<StaffIncome, StaffIncomeSearch>, IStaffIncomeService
    {
        protected IAppDbContext Context;
        protected readonly IUserService userService;
        protected readonly IUserInGroupService userInGroupService;
        protected readonly IMainOrderService mainOrderService;
        protected readonly IHistoryPayWalletService historyPayWalletService;
        public StaffIncomeService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            userService = serviceProvider.GetRequiredService<IUserService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            historyPayWalletService = serviceProvider.GetRequiredService<IHistoryPayWalletService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
        }

        protected override string GetStoreProcName()
        {
            return "StaffIncome_2_GetPagingData";
        }

        /// <summary>
        /// Thanh toán 
        /// </summary>
        /// <param name="listId">List Id staffIncome</param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<bool> UpdateStatus(List<StaffIncome> staffIncomes)
        {
            DateTime currentDate = DateTime.Now;
            string userName = LoginContext.Instance.CurrentUser.UserName;

            if (!staffIncomes.Any())
                throw new KeyNotFoundException("List Item không tồn tại");

            var userIds = new List<int>();

            foreach (var staffIncome in staffIncomes)
            {
                var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == (staffIncome.MainOrderId ?? 0));
                var transOrder = await unitOfWork.Repository<TransportationOrder>().GetQueryable().FirstOrDefaultAsync(x => x.Id == (staffIncome.TransportationOrderId ?? 0));
                var payHelp = await unitOfWork.Repository<PayHelp>().GetQueryable().FirstOrDefaultAsync(x => x.Id == (staffIncome.PayHelpOrderId ?? 0));
                if (mainOrder == null && transOrder == null && payHelp == null)
                    continue;
                var userInGroups = await userInGroupService.GetAsync(x => !x.Deleted &&
                    (x.UserId == staffIncome.UID)
                );
                if (!userInGroups.Any())
                    continue;
                foreach (var userInGroup in userInGroups)
                {
                    int? idUser = 0;
                    if (userInGroup.UserGroupId == (int)PermissionTypes.Orderer
                        || userInGroup.UserGroupId == (int)PermissionTypes.Admin
                        || userInGroup.UserGroupId == (int)PermissionTypes.Manager)
                        if (staffIncome.MainOrderId > 0)
                            idUser = mainOrder.DatHangId;
                    if (userInGroup.UserGroupId == (int)PermissionTypes.Saler
                        || userInGroup.UserGroupId == (int)PermissionTypes.Admin
                        || userInGroup.UserGroupId == (int)PermissionTypes.Manager)
                    {
                        if (staffIncome.MainOrderId > 0)
                            idUser = mainOrder.SalerId;
                        if (staffIncome.TransportationOrderId > 0)
                            idUser = transOrder.SalerID;
                        if (staffIncome.PayHelpOrderId > 0)
                            idUser = payHelp.SalerID;
                    }
                    var user = await userService.GetByIdAsync(idUser ?? 0);
                    if (user == null)
                        continue;

                    decimal? wallet = user.Wallet + staffIncome.TotalPriceReceive;

                    //Cập nhật hoa hồng cho ví tiền
                    user.Updated = currentDate;
                    user.UpdatedBy = userName;
                    user.Wallet = wallet;

                    //Detach

                    unitOfWork.Repository<Users>().Update(user);
                    await unitOfWork.SaveAsync();
                    unitOfWork.Repository<Users>().Detach(user);
                    //Lịch sử ví tiền VNĐ
                    int tradeType = 0;
                    string content = "";
                    if (staffIncome.MainOrderId > 0)
                    {
                        content = string.Format("{0} đã nhận được hoa hồng của đơn mua hộ: {1}.", user.UserName, staffIncome.MainOrderId);
                        tradeType = (int)HistoryPayWalletContents.HoaHongMuaHo;
                    }
                    if (staffIncome.TransportationOrderId > 0) {
                        content = string.Format("{0} đã nhận được hoa hồng của đơn ký gửi: {1}.", user.UserName, staffIncome.TransportationOrderId);
                        tradeType = (int)HistoryPayWalletContents.HoaHongKyGui;
                    }
                    if (staffIncome.PayHelpOrderId > 0) {
                        content = string.Format("{0} đã nhận được hoa hồng của đơn thanh toán hộ: {1}.", user.UserName, staffIncome.PayHelpOrderId);
                        tradeType = (int)HistoryPayWalletContents.HoaHongThanhToanHo;
                    }
                    await unitOfWork.Repository<HistoryPayWallet>().CreateAsync(new HistoryPayWallet
                    {
                        UID = user.Id,
                        MoneyLeft = wallet,
                        Amount = staffIncome.TotalPriceReceive,
                        Type = (int)DauCongVaTru.Cong,
                        TradeType = tradeType,
                        Content = content,
                        Deleted = false,
                        Active = true,
                        Created = currentDate,
                        CreatedBy = userName
                    });


                    //Thay đổi trạng thái thanh toán
                    staffIncome.Updated = currentDate;
                    staffIncome.UpdatedBy = userName;
                    staffIncome.Status = (int)StatusStaffIncome.Paid;

                    unitOfWork.Repository<StaffIncome>().Update(staffIncome);
                    if (mainOrder != null)
                        unitOfWork.Repository<MainOrder>().Detach(mainOrder);
                    if (transOrder != null)
                        unitOfWork.Repository<TransportationOrder>().Detach(transOrder);
                    if (payHelp != null)
                        unitOfWork.Repository<PayHelp>().Detach(payHelp);
                    await unitOfWork.SaveAsync();
                }
            }

            return true;
        }
    }
}
