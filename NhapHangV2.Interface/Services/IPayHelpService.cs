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
    public interface IPayHelpService : IDomainService<PayHelp, PayHelpSearch>
    {
        Task<bool> UpdateStatus(PayHelp model, int status, int statusOld, decimal? totalPriceVNDOld);
        Task<bool> UpdateCurrency(int id, decimal? currency);
        Task<AmountStatistic> GetTotalOrderPriceByUID(int UID);
        Task<bool> UpdateStaff(PayHelp payHelp, int oldSalerId);
        List<CountStatusData> CountStatus(PayHelpSearch payHelpSearch);
    }
}
