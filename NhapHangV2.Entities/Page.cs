
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class Page : DomainEntities.AppDomain
    {
        /// <summary>
        /// Id chuyên mục bài viết
        /// </summary>
        public int? PageTypeId { get; set; } = 0;

        /// <summary>
        /// Tiêu đề
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả ngắn
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung
        /// </summary>
        public string PageContent { get; set; } = string.Empty;

        /// <summary>
        /// Cờ ẩn
        /// </summary>
        public bool? IsHidden { get; set; } = false;

        /// <summary>
        /// Cờ Sidebar
        /// </summary>
        public bool? SideBar { get; set; } = false;

        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        public string IMG { get; set; } = string.Empty;

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
        /// OG Facebook Tile
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

        public int? MenuId { get; set; } = 0;
    }
}
