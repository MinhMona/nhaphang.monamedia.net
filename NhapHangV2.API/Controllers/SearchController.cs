using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Request;
using NhapHangV2.Utilities;
using Polly.Caching;
using System;
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
        private readonly IMemoryCache memoryCache;
        public SearchController(ISearchService searchService, ICrawlProductService crawlProductService, IMemoryCache memoryCache)
        {
            this.searchService = searchService;
            this.crawlProductService = crawlProductService;
            this.memoryCache = memoryCache;
        }

        [HttpPost]
        public AppDomainResult SearchContent(SearchRequest searchRequest)
        {
            return searchService.SearchContent(searchRequest.Site, searchRequest.Content);
        }

        [HttpGet]
        public async Task<AppDomainResult> CrawlProduct([FromQuery] long id, [FromQuery] string web, [FromQuery] bool isDev)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            string result = string.Empty;

            result = await crawlProductService.CrawlProduct(id, web);
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
