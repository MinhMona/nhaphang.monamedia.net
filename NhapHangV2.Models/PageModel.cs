using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Models
{
    public class PageModel : AppDomainModel
    {
        /// <summary>
        /// Id chuyên mục bài viết
        /// </summary>
        public int? PageTypeId { get; set; }

        /// <summary>
        /// Tiêu đề
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Mô tả ngắn
        /// </summary>
        public string Summary { get; set; } 

        /// <summary>
        /// Nội dung
        /// </summary>
        public string PageContent { get; set; } 

        /// <summary>
        /// Cờ ẩn
        /// </summary>
        public bool? IsHidden { get; set; }

        /// <summary>
        /// Cờ Sidebar
        /// </summary>
        public bool? SideBar { get; set; }

        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        public string IMG { get; set; } 

        /// <summary>
        /// OG Url
        /// </summary>
        public string OGUrl { get; set; } 

        /// <summary>
        /// OG Title
        /// </summary>
        public string OGTitle { get; set; } 

        /// <summary>
        /// OG Description
        /// </summary>
        public string OGDescription { get; set; } 

        /// <summary>
        /// OG Image
        /// </summary>
        public string OGImage { get; set; } 

        /// <summary>
        /// Meta Title
        /// </summary>
        public string MetaTitle { get; set; } 

        /// <summary>
        /// Meta Description
        /// </summary>
        public string MetaDescription { get; set; } 

        /// <summary>
        /// Meta Keyword
        /// </summary>
        public string MetaKeyword { get; set; } 

        /// <summary>
        /// OG Facebook Tile
        /// </summary>
        public string OGFacebookTitle { get; set; } 

        /// <summary>
        /// OG Facebook Description
        /// </summary>
        public string OGFacebookDescription { get; set; } 

        /// <summary>
        /// OG Facebook IMG
        /// </summary>
        public string OGFacebookIMG { get; set; } 

        /// <summary>
        /// OG Twitter Title
        /// </summary>
        public string OGTwitterTitle { get; set; } 

        /// <summary>
        /// OG Twitter Description
        /// </summary>
        public string OGTwitterDescription { get; set; } 

        /// <summary>
        /// OG Twitter IMG
        /// </summary>
        public string OGTwitterIMG { get; set; }

        public int? MenuId { get; set; }
    }
}
