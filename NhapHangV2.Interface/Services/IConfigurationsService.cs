using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Interface.Services
{
    public interface IConfigurationsService : IDomainService<Configurations, BaseSearch>
    {
        Task<Configurations> GetSingleAsync();
        Task<decimal> GetCurrentPayHelp(decimal price);
        Task<decimal> GetCurrency(int? UID);
        Task<object> GetWarehouse();
    }
}
