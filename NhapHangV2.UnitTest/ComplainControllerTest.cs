using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.API.Controllers;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities.Search;
using NhapHangV2.Entities;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.DomainServices;
using NhapHangV2.Models;
using NhapHangV2.Request;
using FluentAssertions;
using System.Net;
using static NhapHangV2.Utilities.CoreContants;
using NhapHangV2.Utilities;

namespace NhapHangV2.UnitTest
{
    public class ComplainControllerTest
    {
        protected readonly ILogger<BaseController<Complain, ComplainModel, ComplainRequest, ComplainSearch>> logger;
        protected readonly IServiceProvider serviceProvider;
        protected IWebHostEnvironment env;
        private readonly IConfiguration configuration;
        private readonly IComplainService complainService;

        public ComplainControllerTest()
        {
            logger = A.Fake<ILogger<BaseController<Complain, ComplainModel, ComplainRequest, ComplainSearch>>>();
            serviceProvider = A.Fake<IServiceProvider>();
            env = A.Fake<IWebHostEnvironment>();
            configuration = A.Fake<IConfiguration>();
            complainService = A.Fake<IComplainService>();
        }
        [Fact]
        public void AddItem()
        {
            //Arrange
            var controller = new ComplainController(serviceProvider, logger, env,configuration);
            ComplainRequest request = new ComplainRequest()
            {
                Amount = 10000,
                ComplainText = "Khiếu nại unit test",
                MainOrderId = 1395,
                UID = 22
            };
            //Act
            var result = controller.AddItem(request);
            //Assert
            result.Should().NotBeNull();
            result.Result.ResultCode = (int)HttpStatusCode.OK;
        }

        [Fact]
        public async void CreateAsync()
        {
            //Arrange
            Complain request = new Complain()
            {
                Amount = 10000,
                ComplainText = "Khiếu nại unit test 33",
                MainOrderId = 1395,
                UID = 22,
                Status = (int)StatusComplain.ChuaDuyet,
                Active = true,
                Deleted = false
            };
            //Act
            var result = await complainService.CreateAsyncNew(request);
            //Assert
            result.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}