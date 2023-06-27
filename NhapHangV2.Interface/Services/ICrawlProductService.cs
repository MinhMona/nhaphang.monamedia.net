using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Interface.Services
{
    public interface ICrawlProductService
    {
        Task<string> CrawlProduct(long id, string web);
    }
}
