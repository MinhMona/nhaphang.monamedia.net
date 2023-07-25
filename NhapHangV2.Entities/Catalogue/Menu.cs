
using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Catalogue
{
    public class Menu : AppDomainCatalogue
    {
        /// <summary>
        /// Link menu
        /// </summary>
        public string Link { get; set; } = string.Empty;

        /// <summary>
        /// 1. Ngoài, 2. Trong
        /// </summary>
        public int? Type { get; set; } = 0;

        /// <summary>
        /// Id menu Cha
        /// </summary>
        public int? Parent { get; set; } = 0;

        /// <summary>
        /// Vị trí
        /// </summary>
        public int? Position { get; set; } = 0;
        /// <summary>
        /// Id chuyên mục
        /// </summary>
        public int? PageTypeId { get; set; } = 0;
        /// <summary>
        /// id bài viết
        /// </summary>
        public int? PageId { get; set; } = 0;
    }
}
