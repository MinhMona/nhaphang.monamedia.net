using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Request.Catalogue;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers.Catalogue
{
    [Route("api/customer-benefits")]
    [ApiController]
    [Description("Danh sách quyền lợi khách hàng (Trang chủ)")]
    public class CustomerBenefitsController : ControllerBase
    {
        protected IMapper mapper;
        protected IWebHostEnvironment env;
        protected IConfiguration configuration;

        protected readonly ICustomerBenefitsService customerBenefitsService;
        public CustomerBenefitsController(IServiceProvider serviceProvider, ILogger<CustomerBenefitsController> logger, IWebHostEnvironment env, IMapper mapper, IConfiguration configuration)
        {
            this.mapper = mapper;
            this.env = env;
            this.configuration = configuration;

            customerBenefitsService = serviceProvider.GetRequiredService<ICustomerBenefitsService>();
        }

        /// <summary>
        /// Lấy thông tin theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<AppDomainResult> GetById(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            var item = await customerBenefitsService.GetByIdAsync(id);
            if (item != null)
            {
                var itemModel = mapper.Map<CustomerBenefitsModel>(item);
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

        /// <summary>
        /// Thêm mới item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public async Task<AppDomainResult> AddItem([FromBody] CustomerBenefitsRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = false;
            if (ModelState.IsValid)
            {
                itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Name).ToLower().Replace(" ", "-");
                var item = mapper.Map<CustomerBenefits>(itemModel);
                item.Created = DateTime.UtcNow.AddHours(7);
                item.CreatedBy = LoginContext.Instance.CurrentUser.UserName;
                item.Active = true;
                if (item != null)
                {
                    //// Kiểm tra item có tồn tại chưa?
                    //var messageUserCheck = await this.catalogueService.GetExistItemMessage(item);
                    //if (!string.IsNullOrEmpty(messageUserCheck))
                    //    throw new KeyNotFoundException(messageUserCheck);
                    success = await customerBenefitsService.CreateAsync(item);
                    if (success)
                    {
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    }
                    

                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateItem([FromBody] CustomerBenefitsRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = false;
            if (ModelState.IsValid)
            {
                itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Name).ToLower().Replace(" ", "-");
                var item = mapper.Map<CustomerBenefits>(itemModel);
                item.Updated = DateTime.UtcNow.AddHours(7);
                item.UpdatedBy = LoginContext.Instance.CurrentUser.UserName;
                
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    //var messageUserCheck = await this.catalogueService.GetExistItemMessage(item);
                    //if (!string.IsNullOrEmpty(messageUserCheck))
                    //    throw new KeyNotFoundException(messageUserCheck);
                    success = await customerBenefitsService.UpdateAsync(item);
                    if (success)
                    {
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    }
                    
                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Xóa item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [AppAuthorize(new int[] { CoreContants.Delete })]
        public async Task<AppDomainResult> DeleteItem(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = await customerBenefitsService.DeleteAsync(id);
            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                appDomainResult.Success = success;
            }
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            return appDomainResult;
        }

        /// <summary>
        /// Lấy danh sách item phân trang
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<AppDomainResult> Get([FromQuery] CatalogueSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                PagedList<CustomerBenefits> pagedData = await customerBenefitsService.GetPagedListData(baseSearch);
                PagedList<CustomerBenefitsModel> pagedDataModel = mapper.Map<PagedList<CustomerBenefitsModel>>(pagedData);
                appDomainResult = new AppDomainResult
                {
                    Data = pagedDataModel,
                    Success = true,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }
    }
}
