using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Catalogue
{
    public class PageType : AppDomainCatalogue
    {
        /// <summary>
        /// Title của chuyên mục
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// OG Url
        /// </summary>
        public string OGUrl { get; set; } = string.Empty;

        /// <summary>
        /// OG Title
        /// </summary>
        public string OGTitle { get; set; } = string.Empty;

        /// <summary>
        /// OG Description
        /// </summary>
        public string OGDescription { get; set; } = string.Empty;

        /// <summary>
        /// OG Image
        /// </summary>
        public string OGImage { get; set; } = string.Empty;

        /// <summary>
        /// Meta Title
        /// </summary>
        public string MetaTitle { get; set; } = string.Empty;

        /// <summary>
        /// Meta Description
        /// </summary>
        public string MetaDescription { get; set; } = string.Empty;

        /// <summary>
        /// Meta Keyword
        /// </summary>
        public string MetaKeyword { get; set; } = string.Empty;

        /// <summary>
        /// OG Facebook Title 
        /// </summary>
        public string OGFacebookTitle { get; set; } = string.Empty;

        /// <summary>
        /// OG Facebook Description
        /// </summary>
        public string OGFacebookDescription { get; set; } = string.Empty;

        /// <summary>
        /// OG Facebook IMG
        /// </summary>
        public string OGFacebookIMG { get; set; } = string.Empty;

        /// <summary>
        /// OG Twitter Title
        /// </summary>
        public string OGTwitterTitle { get; set; } = string.Empty;

        /// <summary>
        /// OG Twitter Description
        /// </summary>
        public string OGTwitterDescription { get; set; } = string.Empty;

        /// <summary>
        /// OG Twitter IMG
        /// </summary>
        public string OGTwitterIMG { get; set; } = string.Empty;

        /// <summary>
        /// SideBar
        /// </summary>
        public bool? SideBar { get; set; } = false;

        /// <summary>
        /// menu id
        /// </summary>
        public int? MenuId { get; set; } = 0;

        /// <summary>
        /// Danh sách bài viết
        /// </summary>
        [NotMapped]
        public List<Page> Pages { get; set; } = new List<Page>();

        /// <summary>
        /// Danh sách bài viết store
        /// </summary>
        [NotMapped]
        public string PagesJson { get; set; }
    }
}
