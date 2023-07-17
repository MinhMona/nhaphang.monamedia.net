using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Interface.Services.DomainServices;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Interface.Services
{
    public interface ISmallPackageService : IDomainService<SmallPackage, SmallPackageSearch>
    {
        Task<List<SmallPackage>> ExportForBarCode(int UID, string BarCode, int Type);

        Task<List<SmallPackage>> ExportForUserName(int UID, int OrderID, int StatusType, int OrderType);

        Task<bool> UpdateIsLost(SmallPackage item);

        Task<List<SmallPackage>> CheckBarCode(List<SmallPackage> items, bool isAssign);

        Task<AppDomainImportResult> ImportTemplateFile(Stream stream, int? bigPackageId, string createdBy);

        Task<SmallPackage> GetByOrderTransactionCode(string code);

        Task<List<SmallPackage>> GetAllByMainOrderId(int mainOrderId);
        Task<List<SmallPackage>> GetAllByTransportationOrderId(int transportationOrderId);
        Task<List<SmallPackage>> GetInVietNamByMainOrderId(int mainOrderId);
        void CreateSmallPackageTool(List<SmallPackageToolRequest> requests);
    }
}
