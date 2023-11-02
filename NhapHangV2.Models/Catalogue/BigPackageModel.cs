using NhapHangV2.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Models.Catalogue
{
    public class BigPackageModel : AppDomainCatalogueModel
    {
        /// <summary>
        /// Cân nặng (kg)
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Khối (m3)
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Tên trạng thái
        /// </summary>
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case 0:
                        return "Đã hủy";
                    case 1:
                        return "Bao hàng ở Trung Quốc";
                    case 2:
                        return "Trên đường về VN";
                    case 3:
                        return "Trong kho VN";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Tổng kiện
        /// </summary>
        public int? Total { get; set; }

        public List<SmallPackageModel> SmallPackages { get; set; }
    }
}
