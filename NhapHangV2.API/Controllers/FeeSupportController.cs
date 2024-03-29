﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models;
using NhapHangV2.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers
{
    [Route("api/fee-support")]
    [ApiController]
    [Description("Quản lý phụ phí")]
    [Authorize]
    public class FeeSupportController : BaseController<FeeSupport, FeeSupportModel, FeeSupportRequest, BaseSearch>
    {
        public FeeSupportController(IServiceProvider serviceProvider, ILogger<BaseController<FeeSupport, FeeSupportModel, FeeSupportRequest, BaseSearch>> logger, IWebHostEnvironment env) : base(serviceProvider, logger, env)
        {
            this.domainService = serviceProvider.GetRequiredService<IFeeSupportService>();
        }
    }
}
