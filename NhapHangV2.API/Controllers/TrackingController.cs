using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.Entities;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers
{
    [Route("api/tracking")]
    [ApiController]
    [Description("Tracking theo mã vận đơn (Website)")]
    public class TrackingController : ControllerBase
    {
        protected IMapper mapper;
        protected IWebHostEnvironment env;

        protected readonly ISmallPackageService smallPackageService;

        public TrackingController(IServiceProvider serviceProvider, ILogger<TrackingController> logger, IWebHostEnvironment env, IMapper mapper)
        {
            this.mapper = mapper;
            this.env = env;

            smallPackageService = serviceProvider.GetRequiredService<ISmallPackageService>();
        }

        /// <summary>
        /// Tracking theo mã vận đơn
        /// </summary>
        /// <param name="transactionCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<AppDomainResult> Tracking(string transactionCode) //Giống tracking của SmallpackageController
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            transactionCode = transactionCode.Trim();
            if (string.IsNullOrEmpty(transactionCode))
                throw new KeyNotFoundException("Code không tồn tại");

            IList<SmallPackage> items = new List<SmallPackage>();

            items = await smallPackageService.GetAsync(x => !x.Deleted
                    && (x.OrderTransactionCode == transactionCode));

            if (items.Any())
            {
                items = await smallPackageService.CheckBarCode(items.ToList(), false);

                var itemModel = mapper.Map<List<SmallPackageModel>>(items);
                appDomainResult = new AppDomainResult()
                {
                    Success = true,
                    Data = itemModel,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
                throw new KeyNotFoundException("Item không tồn tại");
            return appDomainResult;
        }
    }
}
