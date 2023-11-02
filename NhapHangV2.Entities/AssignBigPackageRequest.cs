using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities
{
    public class AssignBigPackageRequest
    {
        public int? BigPackageId { get; set; }
        public List<int> SmallPackageIds { get; set; } = new List<int>();
    }
}
