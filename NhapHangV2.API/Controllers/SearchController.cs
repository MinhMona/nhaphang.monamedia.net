using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Request;
using NhapHangV2.Utilities;
using Polly.Caching;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web;

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

        [HttpGet("get-full-link")]
        public string GetFullLink(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest();
            IRestResponse response = client.Execute(request);
            if (response.Content.Contains("wireless1688://ma.m.1688.com/plugin?url="))
            {
                var urlRed = HttpUtility.UrlDecode(response.Content.Replace("wireless1688://ma.m.1688.com/plugin?url=", ""));
                return urlRed;

            }
            return response.Content;
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
