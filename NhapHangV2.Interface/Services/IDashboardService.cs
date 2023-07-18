using NhapHangV2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Interface.Services
{
    public interface IDashboardService
    {
        Task<List<Dashboard_GetTotalInWeek>> GetTotalInWeek();

        Task<List<Dashboard_GetItemInWeek>> GetItemInWeek();

        Task<List<Dashboard_GetPercentOrder>> GetPercentOrder();
        Task<List<Dashboard_GetPercentOrder>> GetPercentPayHelp();
        Task<List<Dashboard_GetPercentOrder>> GetPercentTransOrder();
    }
}
