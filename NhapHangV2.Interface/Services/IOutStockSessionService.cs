using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NhapHangV2.Entities.Search;

namespace NhapHangV2.Interface.Services
{
    public interface IOutStockSessionService : IDomainService<OutStockSession, OutStockSessionSearch>
    {
        Task<bool> Export(int id);

        Task<bool> UpdateStatus(int id, int status, bool isPaymentWallet);

        Task<bool> DeleteNotPayment(OutStockSession item);

        Task<bool> UpdateOutStockStatus(int id, int status);
    }
}
