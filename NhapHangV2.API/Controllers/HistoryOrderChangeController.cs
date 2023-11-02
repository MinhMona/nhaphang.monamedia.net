using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models;
using NhapHangV2.Request;
using System;

namespace NhapHangV2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryOrderChangeController : BaseController<HistoryOrderChange, HistoryOrderChangeModel, HistoryOrderChangeRequest, HistoryOrderChangeSearch>
    {
        public HistoryOrderChangeController(IServiceProvider serviceProvider, ILogger<ControllerBase> logger, IWebHostEnvironment env) : base(serviceProvider, logger, env)
        {
            this.domainService = this.serviceProvider.GetRequiredService<IHistoryOrderChangeService>();
        }
    }
}
