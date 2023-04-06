using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services
{
    public class MainOrderCodeService : DomainService<MainOrderCode, MainOrderCodeSearch>, IMainOrderCodeService
    {
        public MainOrderCodeService(IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        protected override string GetStoreProcName()
        {
            return "MainOrderCode_GetPagingData";
        }

        public override async Task<bool> CreateAsync(MainOrderCode item)
        {
            int userId = LoginContext.Instance.CurrentUser.UserId;
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == userId).FirstOrDefaultAsync();

            var exists = await this.GetSingleAsync(x => !x.Deleted && x.Code.Equals(item.Code));
            if (exists != null)
                throw new AppException("Mã đơn hàng đã tồn tại");
            var mainOrder = await unitOfWork.Repository<MainOrder>().GetQueryable().Where(x => x.Id == item.MainOrderID).FirstOrDefaultAsync();
            mainOrder.Status = (int?)StatusOrderContants.DaMuaHang;
            await unitOfWork.Repository<MainOrderCode>().CreateAsync(item);
            unitOfWork.Repository<MainOrder>().Update(mainOrder);
            await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
            {
                MainOrderId = item.MainOrderID,
                UID = userId, //Lấy thằng đăng nhập
                HistoryContent = string.Format("{0} đã thêm mới mã đơn hàng của đơn hàng ID là: {1}. Mã đơn hàng: {2}", user.UserName, item.MainOrderID, item.Code),
                Type = (int)TypeHistoryOrderChange.MaDonHang,
            });

            await unitOfWork.SaveAsync();
            return true;
        }

        public override async Task<bool> UpdateAsync(MainOrderCode item)
        {
            var exists = await Queryable
                 .AsNoTracking()
                 .Where(e => e.Id == item.Id && !e.Deleted)
                 .FirstOrDefaultAsync();

            if (exists != null)
            {
                var currentCreated = exists.Created;
                var currentCreatedByInfo = exists.CreatedBy;
                exists = mapper.Map<MainOrderCode>(item);
                exists.Created = currentCreated;
                exists.CreatedBy = currentCreatedByInfo;
                unitOfWork.Repository<MainOrderCode>().Update(exists);


                int userId = LoginContext.Instance.CurrentUser.UserId;
                var user = unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == userId).FirstOrDefault();

                await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                {
                    MainOrderId = item.MainOrderID,
                    UID = userId, //Lấy thằng đăng nhập
                    HistoryContent = string.Format("{0} đã đổi mã đơn hàng của đơn hàng ID là: {1}. Mã đơn hàng mới: {2}", user.UserName, item.MainOrderID, item.Code),
                    Type = (int)TypeHistoryOrderChange.MaDonHang,
                });

            }

            await unitOfWork.SaveAsync();
            return true;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var exists = Queryable
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == id);
            if (exists != null)
            {
                exists.Deleted = true;
                unitOfWork.Repository<MainOrderCode>().Update(exists);

                int userId = LoginContext.Instance.CurrentUser.UserId;
                var user = unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == userId).FirstOrDefault();

                var smallPackages = await unitOfWork.Repository<SmallPackage>().GetQueryable().Where(x => !x.Deleted && x.MainOrderCodeId == exists.Id).ToListAsync();
                if (smallPackages.Any())
                {
                    foreach (var smallPackage in smallPackages)
                    {
                        smallPackage.MainOrderCodeId = 0;
                        unitOfWork.Repository<SmallPackage>().Update(smallPackage);
                    }
                }

                await unitOfWork.Repository<HistoryOrderChange>().CreateAsync(new HistoryOrderChange()
                {
                    MainOrderId = exists.MainOrderID,
                    UID = userId, //Lấy thằng đăng nhập
                    HistoryContent = string.Format("{0} đã xóa mã đơn hàng của đơn hàng ID là: {1}. Mã đơn hàng: {2}", user.UserName, exists.MainOrderID, exists.Code),
                    Type = (int)TypeHistoryOrderChange.MaDonHang,
                });


                await unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new Exception(id + " not exists");
            }
        }
    }
}
