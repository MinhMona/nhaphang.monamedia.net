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
    public class OrderShopTempRequest : AppDomainRequest
    {
        ///// <summary>
        ///// Id của shop
        ///// </summary>
        //[StringLength(500)]
        //public string? shop_id { get; set; }

        ///// <summary>
        ///// Tên shop
        ///// </summary>
        //[StringLength(500)]
        //public string? shop_name { get; set; }

        ///// <summary>
        ///// Website
        ///// </summary>
        //[StringLength(10)]
        //public string? site { get; set; }

        ///// <summary>
        ///// List sản phẩm
        ///// </summary>
        //public List<OrderTempRequest> OrderTemps { get; set; }

        public string? title_origin { get; set; }

        public decimal? price_origin { get; set; }

        public decimal? price_promotion { get; set; }

        public string? seller_id { get; set; }

        public string? shop_id { get; set; }

        public string? shop_name { get; set; }

        public string? wangwang { get; set; }

        public int? quantity { get; set; }

        public int? stock { get; set; }

        public string? item_id { get; set; }

        public string? brand { get; set; }

        public string? property { get; set; }

        public string? link_origin { get; set; }

        public string? image_model { get; set; }

        public string? site { get; set; }

        public string? image_origin { get; set; }

        public int? MinimumQuantity { get; set; }



        //public string? location_sale { get; set; }

        //public string? property_translated { get; set; }

        //public string? weight { get; set; }

        //public string? category_name { get; set; }

        //public string? category_id { get; set; }

        //public string? title_translated { get; set; }



        //public string? error { get; set; }

        //public string? tool { get; set; }

        //public string? version { get; set; }

        //public bool? is_translate { get; set; }

        public string? pricestep { get; set; }

    }
}
