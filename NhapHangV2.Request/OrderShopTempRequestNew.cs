using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class OrderShopTempRequestNew : AppDomainRequest
    {
        /// <summary>
        /// Id của shop
        /// </summary>
        [StringLength(500)]
        public string? ShopId { get; set; }

        /// <summary>
        /// Tên shop
        /// </summary>
        [StringLength(500)]
        public string? ShopName { get; set; }

        /// <summary>
        /// Website
        /// </summary>
        [StringLength(10)]
        public string? Site { get; set; }

        /// <summary>
        /// Extension (Công cụ)
        /// </summary>
        public string? Tool { get; set; }

        /// <summary>
        /// Extension (Phiên bản)
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Bước nhảy
        /// </summary>
        public string? StepPrice { get; set; }

        /// <summary>
        /// Số lượng sản phẩm tối thiểu
        /// </summary>
        public int? MinimumQuantity { get; set; }

        /// <summary>
        /// Link của sản phẩm
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        /// Id của sản phẩm trên website
        /// </summary>
        public string? ItemID { get; set; }

        /// <summary>
        /// Id của sản phẩm trên website
        /// </summary>
        public string? Wangwang { get; set; }

        /// <summary>
        /// Id của sản phẩm trên website
        /// </summary>
        public string? SalerId { get; set; }

        /// <summary>
        /// List sản phẩm
        /// </summary>
        public List<OrderTempRequestNew>? Orders { get; set; }

    }
}
