using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Models;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Request;
using NhapHangV2.Service.Services;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers
{
    [Route("api/page")]
    [ApiController]
    [Description("Danh sách bài viết")]
    public class PageController : ControllerBase
    {
        protected IMapper mapper;
        protected readonly IConfiguration configuration;
        protected readonly IPageService pageService;
        protected readonly IPageTypeService pageTypeService;
        protected readonly IWebHostEnvironment env;
        public PageController(IServiceProvider serviceProvider, ILogger<PageController> logger, IWebHostEnvironment env, IConfiguration configuration, IPageTypeService pageTypeService, IMapper mapper)
        {
            this.mapper = mapper;
            this.configuration = configuration;
            this.pageTypeService = pageTypeService;
            this.env = env;
            pageService = serviceProvider.GetRequiredService<IPageService>();
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

            var item = await pageService.GetByIdAsync(id);
            if (item != null)
            {
                var itemModel = mapper.Map<PageModel>(item);
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
        /// Lấy thông tin theo code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("get-by-code")]
        public async Task<AppDomainResult> GetByCode(string code)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (string.IsNullOrEmpty(code))
            {
                throw new KeyNotFoundException("code không tồn tại");
            }
            var item = await pageService.GetByCodeAsync(code);
            if (item != null)
            {
                var itemModel = mapper.Map<PageModel>(item);
                appDomainResult = new AppDomainResult()
                {
                    Success = true,
                    Data = itemModel,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
            {
                throw new KeyNotFoundException("Item không tồn tại");
            }
            return appDomainResult;
        }

        /// <summary>
        /// Thêm mới item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public async Task<AppDomainResult> AddItem([FromBody] PageRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                int imgIndex = -1;
                do
                {
                    imgIndex = itemModel.PageContent.IndexOf("src=", imgIndex + 1);
                    if (imgIndex < 0)
                    {
                        break;
                    }
                    imgIndex += 5;
                    int imgStartIndex = imgIndex;
                    imgIndex = itemModel.PageContent.IndexOf(',', imgIndex);
                    imgIndex++;
                    int imgEndIndex = itemModel.PageContent.IndexOf('"', imgIndex);
                    try
                    {
                        string content = itemModel.PageContent.Substring(imgIndex, imgEndIndex - imgIndex);
                        string fileUploadPath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.UPLOAD_FOLDER_NAME);
                        string path = Path.Combine(fileUploadPath, Guid.NewGuid().ToString() + ".PNG");
                        FileUtilities.SaveToPath(path, Convert.FromBase64String(content));
                        var currentLinkSite = $"{Extensions.HttpContext.Current.Request.Scheme}://{Extensions.HttpContext.Current.Request.Host}/{CoreContants.UPLOAD_FOLDER_NAME}/{CoreContants.UPLOAD_FOLDER_NAME}";
                        var fileUrl = Path.Combine(currentLinkSite, Path.GetFileName(path));
                        content = itemModel.PageContent.Substring(imgStartIndex, imgEndIndex - imgStartIndex);
                        itemModel.PageContent = itemModel.PageContent.Replace(content, fileUrl);
                    }
                    catch
                    {
                        imgIndex++;
                    }
                } while (true);
                itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Title).ToLower().Replace(" ", "-");
                var pageType = await pageTypeService.GetByIdAsync(Convert.ToInt32(itemModel.PageTypeId));
                itemModel.Code = pageType.Code + "/" + itemModel.Code;
                var item = mapper.Map<Page>(itemModel);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var existCode = await pageService.GetAsync(x => !x.Deleted && x.Id != item.Id && x.Code == item.Code);
                    if (existCode.Count > 0)
                        throw new KeyNotFoundException("Mã đã tồn tại!");

                    success = await pageService.CreateAsync(item);
                    if (success)
                    {
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                        appDomainResult.Data = item;
                    }

                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
            {
                throw new AppException(ModelState.GetErrorMessage());
            }
            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateItem([FromBody] PageRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                int imgIndex = -1;
                do
                {
                    imgIndex = itemModel.PageContent.IndexOf("src=", imgIndex + 1);
                    if (imgIndex < 0)
                    {
                        break;
                    }
                    imgIndex += 5;
                    int imgStartIndex = imgIndex;
                    imgIndex = itemModel.PageContent.IndexOf(',', imgIndex);
                    imgIndex++;
                    int imgEndIndex = itemModel.PageContent.IndexOf('"', imgIndex);
                    try
                    {
                        string content = itemModel.PageContent.Substring(imgIndex, imgEndIndex - imgIndex);
                        string fileUploadPath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.UPLOAD_FOLDER_NAME);
                        string path = Path.Combine(fileUploadPath, Guid.NewGuid().ToString() + ".PNG");
                        FileUtilities.SaveToPath(path, Convert.FromBase64String(content));
                        var currentLinkSite = $"{Extensions.HttpContext.Current.Request.Scheme}://{Extensions.HttpContext.Current.Request.Host}/{CoreContants.UPLOAD_FOLDER_NAME}/{CoreContants.UPLOAD_FOLDER_NAME}";
                        var fileUrl = Path.Combine(currentLinkSite, Path.GetFileName(path));
                        content = itemModel.PageContent.Substring(imgStartIndex, imgEndIndex - imgStartIndex);
                        itemModel.PageContent = itemModel.PageContent.Replace(content, fileUrl);
                    }
                    catch
                    {
                        imgIndex++;
                    }
                } while (true);
                itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Title).ToLower().Replace(" ", "-");
                var pageType = await pageTypeService.GetByIdAsync(Convert.ToInt32(itemModel.PageTypeId));
                itemModel.Code = pageType.Code + "/" + itemModel.Code;
                var page = pageService.GetById(itemModel.Id);
                if (!page.Code.Equals(itemModel.Code))
                {
                    // Kiểm tra item có tồn tại chưa?
                    var existCode = await pageService.GetAsync(x => !x.Deleted && x.Code == itemModel.Code);
                    if (existCode.Count > 0)
                        throw new KeyNotFoundException("Mã đã tồn tại!");
                }
                var item = mapper.Map<Page>(itemModel);
                if (item != null)
                {

                    success = await pageService.UpdateAsync(item);
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

            bool success = await pageService.DeleteAsync(id);
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
        public async Task<AppDomainResult> Get([FromQuery] PageSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                PagedList<Page> pagedData = await pageService.GetPagedListData(baseSearch);
                PagedList<PageModel> pagedDataModel = mapper.Map<PagedList<PageModel>>(pagedData);
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
