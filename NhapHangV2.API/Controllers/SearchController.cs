using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Request;
using NhapHangV2.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers
{
    [Route("api/search")]
    [ApiController]
    [Description("Tìm kiếm sản phẩm")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService searchService;
        private readonly ICrawlProductService crawlProductService;
        public SearchController(ISearchService searchService, ICrawlProductService crawlProductService)
        {
            this.searchService = searchService;
            this.crawlProductService = crawlProductService;
        }

        [HttpPost]
        public AppDomainResult SearchContent(SearchRequest searchRequest)
        {
            return searchService.SearchContent(searchRequest.Site, searchRequest.Content);
        }

        [HttpGet]
        public async Task<AppDomainResult> CrawlProduct([FromQuery] long id, [FromQuery] string web)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            var result = await crawlProductService.CrawlProduct(id, web);
            if (!string.IsNullOrEmpty(result))
            {
                appDomainResult = new AppDomainResult()
                {
                    Success = true,
                    Data = result,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
            {
                throw new KeyNotFoundException("Item không tồn tại");
            }
            return appDomainResult;
        }
    }
}
