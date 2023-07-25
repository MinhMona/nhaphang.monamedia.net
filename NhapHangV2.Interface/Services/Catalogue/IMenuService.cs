using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services.DomainServices;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Interface.Services.Catalogue
{
    public interface IMenuService : ICatalogueService<Menu, CatalogueSearch>
    {
        Task<PagedList<MenuModel>> GetSubMenu(PagedList<MenuModel> dataList);
        Task<IList<MenuModel>> GetListMenu(IList<MenuModel> menuList);
    }
}
