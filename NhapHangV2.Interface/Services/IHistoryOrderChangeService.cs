using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Interface.Services.DomainServices;

namespace NhapHangV2.Interface.Services
{
    public interface IHistoryOrderChangeService : IDomainService<HistoryOrderChange, HistoryOrderChangeSearch>
    {
    }
}
