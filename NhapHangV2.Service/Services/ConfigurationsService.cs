using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Service.Services
{
    public class ConfigurationsService : DomainService<Entities.Configurations, BaseSearch>, IConfigurationsService
    {
        protected readonly IUserService userService;
        protected readonly IPriceChangeService priceChangeService;
        protected readonly IOrderShopTempService orderShopTempService;
        public ConfigurationsService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            userService = serviceProvider.GetRequiredService<IUserService>();
            priceChangeService = serviceProvider.GetRequiredService<IPriceChangeService>();
            orderShopTempService = serviceProvider.GetRequiredService<IOrderShopTempService>();
        }

        public async Task<Entities.Configurations> GetSingleAsync()
        {
            return await unitOfWork.Repository<Entities.Configurations>()
                .GetQueryable()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetCurrentPayHelp(decimal price)
        {
            decimal pc = 0;

            string userName = LoginContext.Instance.CurrentUser.UserName;
            var users = await userService.GetSingleAsync(x => x.UserName == userName);
            var configurations = await this.GetSingleAsync();

            decimal pc_config = configurations == null ? 0 : configurations.PricePayHelpDefault ?? 0;
            var priceChange = await priceChangeService.GetSingleAsync(f => f.PriceFromCNY < price && f.PriceToCNY >= price);

            if (priceChange != null)
            {
                switch (users.LevelId)
                {
                    case 1:
                        pc = pc_config + priceChange.Vip0 ?? 0;
                        break;
                    case 2:
                        pc = pc_config + priceChange.Vip1 ?? 0;
                        break;
                    case 3:
                        pc = pc_config + priceChange.Vip2 ?? 0;
                        break;
                    case 4:
                        pc = pc_config + priceChange.Vip3 ?? 0;
                        break;
                    case 5:
                        pc = pc_config + priceChange.Vip4 ?? 0;
                        break;
                    case 11:
                        pc = pc_config + priceChange.Vip5 ?? 0;
                        break;
                    case 12:
                        pc = pc_config + priceChange.Vip6 ?? 0;
                        break;
                    case 13:
                        pc = pc_config + priceChange.Vip7 ?? 0;
                        break;
                    case 14:
                        pc = pc_config + priceChange.Vip8 ?? 0;
                        break;
                    default:
                        pc = pc_config + priceChange.Vip8 ?? 0;
                        break;
                }
            }

            return pc;
        }

        public override async Task<bool> UpdateAsync(Entities.Configurations item)
        {
            var exists = await Queryable
             .AsNoTracking()
             .Where(e => e.Id == item.Id && !e.Deleted)
             .FirstOrDefaultAsync();

            if (exists != null)
            {
                var currentCreated = exists.Created;
                var currentCreatedByInfo = exists.CreatedBy;
                exists = mapper.Map<Entities.Configurations>(item);
                exists.Created = currentCreated;
                exists.CreatedBy = currentCreatedByInfo;
                unitOfWork.Repository<Entities.Configurations>().Update(exists);
                await unitOfWork.SaveAsync(); //Phải save lại để get từ thằng OrderShopTemp thì mới có data thay đổi mới được

                //Cập nhật lại tiền ở giỏ hàng khi thay đổi tỷ giá
                //var orderShopTempIds = await unitOfWork.Repository<OrderShopTemp>().GetQueryable().Where(e => !e.Deleted && e.Active).Select(s => s.Id).ToListAsync();
                //if (orderShopTempIds.Any())
                //{
                //    foreach (var orderShopTempId in orderShopTempIds)
                //    {
                //        var orderShopTemp = await orderShopTempService.GetByIdAsync(orderShopTempId);

                //        //Cập nhật tiền
                //        orderShopTemp = await orderShopTempService.UpdatePrice(orderShopTemp);

                //        unitOfWork.Repository<OrderShopTemp>().Update(orderShopTemp);
                //        await this.unitOfWork.SaveAsync(); //Save thử xem sao
                //    }
                //}

            }

            //await unitOfWork.SaveAsync();
            return true;
        }

        public async Task<decimal> GetCurrency(int? UID)
        {
            decimal configCurrency = 0;
            var config = await this.GetSingleAsync();
            if (config != null)
                configCurrency = config.Currency ?? 0;
            decimal userCurrency = 0;
            var user = await userService.GetByIdAsync(UID ?? 0);
            if (user != null)
                userCurrency = user.Currency ?? 0;
            return userCurrency > 0 ? userCurrency : configCurrency;
        }

        public async Task<object> GetWarehouse()
        {
            var warehouse = await unitOfWork.Repository<Warehouse>().GetQueryable().Where(x => !x.Deleted && x.Active).OrderByDescending(x => x.Id).ToListAsync();
            var warehouseFrom = await unitOfWork.Repository<WarehouseFrom>().GetQueryable().Where(x => !x.Deleted && x.Active).OrderByDescending(x => x.Id).ToListAsync();
            var shippingType = await unitOfWork.Repository<ShippingTypeToWareHouse>().GetQueryable().Where(x => !x.Deleted && x.Active).OrderByDescending(x => x.Id).ToListAsync();

            return new
            {
                Warehouse = warehouse,
                WarehouseFrom = warehouseFrom,
                ShippingType = shippingType,
            };
        }
    }
}
