using AutoMapper;
using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;

namespace NhapHangV2.Service.Services
{
    public class HistoryServicesService : DomainService<HistoryServices, BaseSearch>, IHistoryServicesService
    {
        public HistoryServicesService(IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
        protected override string GetStoreProcName()
        {
            return "HistoryOrderChange_GetPagingData";
        }
    }
}
