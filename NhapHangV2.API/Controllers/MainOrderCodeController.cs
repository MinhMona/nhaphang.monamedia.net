using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Entities.SQLEntities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models;
using NhapHangV2.Request;
using NhapHangV2.Service.Services;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers
{
    [Route("api/main-order-code")]
    [Description("Quản lý mã đơn hàng")]
    [ApiController]
    [Authorize]
    public class MainOrderCodeController : BaseController<MainOrderCode, MainOrderCodeModel, MainOrderCodeRequest, MainOrderCodeSearch>
    {
        protected readonly IMainOrderCodeService mainOrderCodeService;
        public MainOrderCodeController(IServiceProvider serviceProvider, ILogger<BaseController<MainOrderCode, MainOrderCodeModel, MainOrderCodeRequest, MainOrderCodeSearch>> logger, IWebHostEnvironment env) : base(serviceProvider, logger, env)
        {
            this.domainService = this.serviceProvider.GetRequiredService<IMainOrderCodeService>();
            mainOrderCodeService = serviceProvider.GetRequiredService<IMainOrderCodeService>();
        }
    }
}
