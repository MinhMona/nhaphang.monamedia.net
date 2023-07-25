using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Models.Catalogue
{
    public class MenuModel : AppDomainCatalogueModel
    {
        /// <summary>
        /// Link menu
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// 1. Ngoài, 2. Trong
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// Id menu Cha
        /// </summary>
        public int? Parent { get; set; }

        /// <summary>
        /// Vị trí
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// danh sach menu con
        /// </summary>
        public IList<MenuModel> Children { get; set; }

        /// <summary>
        /// Id chuyên mục
        /// </summary>
        public int? PageTypeId { get; set; }
        /// <summary>
        /// id bài viết
        /// </summary>
        public int? PageId { get; set; }
    }
}
