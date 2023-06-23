using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.Configurations;
using NhapHangV2.Service.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.Service.Services.Catalogue
{
    public class PageTypeService : DomainService<PageType, CatalogueSearch>, IPageTypeService
    {
        protected readonly IAppDbContext Context;
        public PageTypeService(IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext context) : base(unitOfWork, mapper)
        {
            this.Context = context;
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
            var page = await unitOfWork.Repository<Entities.Page>().GetQueryable()
                .Where(e => e.PageTypeId == item.Id && !e.Deleted && e.Active)
                .Select(x => new Entities.Page() { IMG = x.IMG, Code = x.Code, Title = x.Title, Summary = x.Summary, Id = x.Id, IsHidden = x.IsHidden, Active = x.Active, Created = x.Created, CreatedBy = x.CreatedBy })
                .OrderByDescending(o => o.Id).ToListAsync();
            if (page.Any())
                item.Pages = page;
            return item;
        }


        public async Task<bool> UpdateMenuId(int id, int menuId)
        {
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    unitOfWork.Repository<PageType>().ExecuteNonQuery(string.Format("update PageType set MenuId = {0} where id = {1}", menuId, id));
                    unitOfWork.Repository<Page>().ExecuteNonQuery(string.Format("update Page set MenuId = {0} where PageTypeId = {1}", menuId, id));
                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
