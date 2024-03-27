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
    public interface ITransportationOrderService : IDomainService<TransportationOrder, TransportationOrderSearch>
    {
        Task<TransportationOrderBilling> GetBillingInfo(List<int> listID, bool isUpdated);
        Task<bool> UpdateAsync(IList<TransportationOrder> item, int status, int typePayment);
        Task<TransportationOrder> PriceAdjustment(TransportationOrder item);
        Task<bool> UpdateTransportationOrder(List<int> listId, int userId);
        Task<AmountStatistic> GetTotalOrderPriceByUID(int UID);
        List<TransportationsInfor> GetTransportationsInfor(TransportationOrderSearch transportationOrderSearch);
        TransportationsAmount GetTransportationsAmount(int UID);
        Task<bool> UpdateStaffAsync(int transportationOrderID, int salerID);
        string GetStatusName(int? status);
        Task<bool> UpdateCurrency(int id, decimal currency);

    }
}
