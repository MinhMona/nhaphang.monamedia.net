using Microsoft.EntityFrameworkCore;
using NhapHangV2.Entities;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Service.Services
{
    public class DevLoginService: IDevLoginService
    {
        protected IAppUnitOfWork unitOfWork;

        public DevLoginService(IAppUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<bool> CheckIsDev(string id)
        {
            var dev= await unitOfWork.Repository<DevLogin>().GetQueryable().Where(x => x.TelegramId == id).FirstOrDefaultAsync();
            return dev != null;
        }
    }
}
