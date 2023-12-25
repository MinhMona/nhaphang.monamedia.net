using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Service.Services
{
    public class OrderTempService : DomainService<OrderTemp, OrderTempSearch>, IOrderTempService
    {
        protected readonly IAppDbContext Context;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IOrderShopTempService orderShopTempService;
        public OrderTempService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            orderShopTempService = serviceProvider.GetRequiredService<IOrderShopTempService>();
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var exists = Queryable
                        .AsNoTracking()
                        .FirstOrDefault(e => e.Id == id);
                    if (exists != null)
                    {
                        exists.Deleted = true;
                        unitOfWork.Repository<OrderTemp>().Update(exists);
                        await unitOfWork.SaveAsync();

                        //Nếu bị delete hết sản phẩm thì delete luôn cái shop
                        var orderShopTemp = await orderShopTempService.GetByIdAsync(exists.OrderShopTempId ?? 0);
                        if (!orderShopTemp.OrderTemps.Any()) //Không còn sản phẩm nào
                            await orderShopTempService.DeleteAsync(orderShopTemp.Id);
                        else
                        {
                            orderShopTemp = await orderShopTempService.UpdatePrice(orderShopTemp);
                            await orderShopTempService.UpdateAsync(orderShopTemp);
                        }
                    }
                    else
                        throw new Exception(id + " not exists");
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

        protected override string GetStoreProcName()
        {
            return "OrderTemp_GetPagingData";
        }

        public override async Task<bool> UpdateAsync(OrderTemp item)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    if (item.Quantity == 0)
                    {  //Nếu như bị xuống mức 0 thì xóa nó luôn
                        await this.DeleteAsync(item.Id);
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<OrderTemp>().Detach(item);
                    }
                    else
                    {
                        await UpdateAsync(new List<OrderTemp> { item });
                        await unitOfWork.SaveAsync();
                        unitOfWork.Repository<OrderTemp>().Detach(item);
                        //Cập nhật lại số tiền
                        var orderShopTemp = await orderShopTempService.GetByIdAsync(item.OrderShopTempId ?? 0);
                        orderShopTemp = await orderShopTempService.UpdatePrice(orderShopTemp);
                        await orderShopTempService.UpdateAsync(orderShopTemp);

                    }

                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
            return true;
        }

        public override async Task<OrderTemp> GetByIdAsync(int id)
        {
            var item = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (item == null)
                return null;
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == item.UID && !e.Deleted).FirstOrDefaultAsync();

            var configurations = await configurationsService.GetSingleAsync();
            decimal currency = Convert.ToDecimal(configurations.Currency);

            if (user != null && user.Currency > 0)
                item.Currency = user.Currency;
            else
                item.Currency = currency;

            return item;
        }
    }
}
