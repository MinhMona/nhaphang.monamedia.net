using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Interface.Services.DomainServices;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Interface.Services
{
    public interface IOrderShopTempService : IDomainService<OrderShopTemp, OrderShopTempSearch>
    {
        Task<OrderShopTemp> UpdatePrice(OrderShopTemp item);
        Task<PagedList<OrderShopTemp>> DeleteOrderShopTempAfterDays(PagedList<OrderShopTemp> orderShopTemps);
        Task<OrderShopTemp> CreateWithMainOrderId(int mainOrderId);
        Task<bool> CreateAddSameAsync(OrderShopTemp item);
        bool DeleteSelectedId(List<int> ids);
    }
}
