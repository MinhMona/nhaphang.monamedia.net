using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Service.Services.Catalogue
{
    public class PageTypeService : DomainService<PageType, CatalogueSearch>, IPageTypeService
    {
        public PageTypeService(IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        protected override string GetStoreProcName()
        {
            return "PageType_GetPagingData";
        }

        public override async Task<PageType> GetByIdAsync(int id)
        {
            var item = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (item == null)
                return null;
            var page = await unitOfWork.Repository<Entities.Page>().GetQueryable().Where(e => e.PageTypeId == item.Id && !e.Deleted).OrderByDescending(o => o.Id).ToListAsync();
            if (page.Any())
                item.Pages = page;
            return item;
        }

        public async Task<PageType> GetByCodeAsync(string code)
        {
            var item = await Queryable.Where(e => e.Code == code && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (item == null)
                return null;
            var page = await unitOfWork.Repository<Entities.Page>().GetQueryable().Select(x => new Entities.Page() { IMG = x.IMG, Code = x.Code, Title = x.Title, Summary = x.Summary, Id = x.Id, IsHidden = x.IsHidden, Active = x.Active }).Where(e => e.PageTypeId == item.Id && !e.Deleted).OrderByDescending(o => o.Id).ToListAsync();
            if (page.Any())
                item.Pages = page;
            return item;
        }
    }
}
