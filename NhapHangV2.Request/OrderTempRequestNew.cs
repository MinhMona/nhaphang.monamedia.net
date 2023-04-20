using NhapHangV2.Request.DomainRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Request
{
    public class OrderTempRequestNew : AppDomainRequest
    {

        /// <summary>
        /// Tên sản phẩm (Gốc)
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Giá gốc (tệ)
        /// </summary>
        public decimal? PriceOrigin { get; set; }

        /// <summary>
        /// Giá khuyến mãi (tệ)
        /// </summary>
        public decimal? PricePromotion { get; set; }

        /// <summary>
        /// Thông số (Gốc)
        /// </summary>
        public string? Properties { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// Hàng trong kho
        /// </summary>
        public int? Stock { get; set; }

        /// <summary>
        /// Ảnh sản phẩm
        /// </summary>
        public string? Image { get; set; }


    }
}
