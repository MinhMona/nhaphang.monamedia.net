using NhapHangV2.Entities.DomainEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.Search
{
    public class MainOrderSearch : BaseSearch
    {
        public bool? IsShipRequest { get; set; }
        /// <summary>
        /// Nếu là trang quản lý thì bỏ trống
        /// </summary>
        public int? UID { get; set; }

        /// <summary>
        /// UserGroupID
        /// </summary>
        public int? RoleID { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Tìm kiếm theo
        /// 1. Id
        /// 2. Mã vận đơn
        /// 3. Website
        /// 5. UserName
        /// </summary>
        public int? TypeSearch { get; set; }

        /// <summary>
        /// Loại đơn hàng
        /// 1: Đơn mua hộ
        /// 2: Đơn vận chuyển hộ
        /// 3: Không xác định
        /// </summary>
        public int? OrderType { get; set; }

        /// <summary>
        /// Từ ngày
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Đến ngày
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Giá từ
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? FromPrice { get; set; }

        /// <summary>
        /// Giá đến
        /// </summary>
        [Column(TypeName = "decimal(18,0)")] 
        public decimal? ToPrice { get; set; }

        /// <summary>
        /// Đơn không có mã vận đơn
        /// </summary>
        public bool? IsNotMainOrderCode { get; set; }

        /// <summary>
        /// Search theo mã đơn hàng
        /// </summary>
        public string MainOrderCode { get; set; }

        /// <summary>
        /// Search theo mã vận đơn
        /// </summary>
        public string OrderTransactionCode { get; set; }

        /// <summary>
        /// Id Saler
        /// </summary>
        public int? SalerId { get; set; }

        /// <summary>
        /// Id DatHang
        /// </summary>
        public int? DatHangId { get; set; }
    }
}
