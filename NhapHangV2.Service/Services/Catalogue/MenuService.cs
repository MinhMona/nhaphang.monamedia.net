using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Service.Services.Catalogue
{
    public class MenuService : CatalogueService<Menu, CatalogueSearch>, IMenuService
    {
        public MenuService(IAppUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        public async Task<PagedList<MenuModel>> GetSubMenu(PagedList<MenuModel> dataList)
        {

            for(int i=0; i<dataList.Items.Count;)
            {
                if (dataList.Items[i].Parent == null || Convert.ToInt32(dataList.Items[i].Parent) == 0)
                {
                    var subMenus = await unitOfWork.Repository<Menu>().GetQueryable().Where(e => e.Parent == dataList.Items[i].Id && !e.Deleted).Select(e => new MenuModel()
                    {
                        Active = e.Active,
                        Code = e.Code,
                        Created = e.Created,
                        CreatedBy = e.CreatedBy,
                        Deleted = e.Deleted,
                        Description = e.Description,
                        Id = e.Id,
                        Link = e.Link,
                        Name = e.Name,
                        Position = e.Position,
                        Parent = e.Parent,
                        RowNumber = e.RowNumber,
                        Type = e.Type,
                        Updated = e.Updated,
                        UpdatedBy = e.UpdatedBy,
                        PageId = e.PageId,
                        PageTypeId = e.PageTypeId,
                    }).ToListAsync();
                    dataList.Items[i].Children = subMenus;
                    i++;
                }
                else
                {
                    dataList.Items.RemoveAt(i);
                }
            }
            return dataList;
        }

        public async Task<IList<MenuModel>> GetListMenu(IList<MenuModel> menuList)
        {
            for (int i = 0; i < menuList.Count;)
            {
                if ((menuList[i].Parent == null || Convert.ToInt32(menuList[i].Parent) == 0) && !menuList[i].Deleted)
                {
                    var subMenus = await unitOfWork.Repository<Menu>().GetQueryable().Where(e => e.Parent == menuList[i].Id && !e.Deleted).Select(e => new MenuModel()
                    {
                        Active = e.Active,
                        Code = e.Code,
                        Created = e.Created,
                        CreatedBy = e.CreatedBy,
                        Deleted = e.Deleted,
                        Description = e.Description,
                        Id = e.Id,
                        Link = e.Link,
                        Name = e.Name,
                        Position = e.Position,
                        Parent = e.Parent,
                        RowNumber = e.RowNumber,
                        Type = e.Type,
                        Updated = e.Updated,
                        UpdatedBy = e.UpdatedBy
                    }).ToListAsync();
                    menuList[i].Children = subMenus;
                    i++;
                }
                else
                {
                    menuList.RemoveAt(i);
                }
            }
            return menuList;
        }
    }
}
