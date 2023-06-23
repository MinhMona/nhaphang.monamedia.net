using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Request.Catalogue;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers.Catalogue
{
    [Route("api/menu")]
    [ApiController]
    [Description("Menu (Trang chủ)")]
    public class MenuController : ControllerBase
    {
        protected IMapper mapper;
        protected IWebHostEnvironment env;

        protected readonly IMenuService menuService;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IPageService pageService;
        protected readonly IPageTypeService pageTypeService;

        public MenuController(IServiceProvider serviceProvider, ILogger<MenuController> logger, IWebHostEnvironment env, IMapper mapper)
        {
            this.mapper = mapper;
            this.env = env;

            menuService = serviceProvider.GetRequiredService<IMenuService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            pageService = serviceProvider.GetRequiredService<IPageService>();
            pageTypeService = serviceProvider.GetService<IPageTypeService>();
        }

        /// <summary>
        /// Lấy thông tin cấu hình theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<AppDomainResult> GetById(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            var item = await menuService.GetByIdAsync(id);
            if (item != null)
            {
                var itemModel = mapper.Map<MenuModel>(item);
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
        public async Task<AppDomainResult> AddItem([FromBody] MenuRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = false;
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(itemModel.Code))
                    itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Name).ToLower().Replace(" ", "-");
                var item = mapper.Map<Menu>(itemModel);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await menuService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new KeyNotFoundException(messageUserCheck);
                    success = await menuService.CreateAsync(item);
                    if (success)
                    {
                        if(itemModel.PageTypeId != null)
                        {
                            await pageTypeService.UpdateMenuId(itemModel.PageTypeId?? 0, item.Id);
                        }
                        if(itemModel.PageId != null)
                        {
                            await pageService.UpdateMenuId(itemModel.PageId ?? 0, item.Id);
                        }
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    }
                    else
                        throw new Exception("Lỗi trong quá trình xử lý");
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
        public async Task<AppDomainResult> UpdateItem([FromBody] MenuRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = false;
            if (ModelState.IsValid)
            {
                itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Name).ToLower().Replace(" ", "-");
                var item = mapper.Map<Menu>(itemModel);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await menuService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new KeyNotFoundException(messageUserCheck);
                    success = await menuService.UpdateAsync(item);
                    if (success)
                    {
                        if (itemModel.PageTypeId != null)
                        {
                            await pageTypeService.UpdateMenuId(itemModel.PageTypeId ?? 0, item.Id);
                        }
                        if (itemModel.PageId != null)
                        {
                            await pageService.UpdateMenuId(itemModel.PageId ?? 0, item.Id);
                        }
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    }
                    else
                        throw new Exception("Lỗi trong quá trình xử lý");

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

            bool success = await menuService.DeleteAsync(id);
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
                PagedList<Menu> pagedData = await menuService.GetPagedListData(baseSearch);
                PagedList<MenuModel> pagedDataModel = mapper.Map<PagedList<MenuModel>>(pagedData);
                pagedDataModel = await menuService.GetSubMenu(pagedDataModel);
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


        /// <summary>
        /// Lấy thông tin theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("config")]
        public async Task<AppDomainResult> GetConfigById(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            var item = await configurationsService.GetByIdAsync(id);
            if (item != null)
            {
                //var itemModel = mapper.Map<MenuModel>(item);
                appDomainResult = new AppDomainResult()
                {
                    Success = true,
                    Data = item,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
                throw new KeyNotFoundException("Item không tồn tại");

            return appDomainResult;
        }
    }
}
