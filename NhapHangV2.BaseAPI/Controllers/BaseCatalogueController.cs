﻿using NhapHangV2.Extensions;
using NhapHangV2.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Models.DomainModels;
using NhapHangV2.Interface.Services.DomainServices;
using NhapHangV2.Request.DomainRequests;

namespace NhapHangV2.BaseAPI.Controllers
{
    //Menu, CustomerBenefits, PageType, Service, Step không có kế thừa 
    [ApiController]
    public abstract class BaseCatalogueController<E, T, R, DomainSearch> : BaseController<E, T, R, DomainSearch> where E : AppDomainCatalogue where T : AppDomainCatalogueModel where R : AppDomainCatalogueRequest where DomainSearch : BaseSearch, new()
    {
        protected ICatalogueService<E, DomainSearch> catalogueService;

        public BaseCatalogueController(IServiceProvider serviceProvider, ILogger<BaseController<E, T, R, DomainSearch>> logger, IWebHostEnvironment env) : base(serviceProvider, logger, env)
        {
        }

        /// <summary>
        /// Lấy thông tin theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public override async Task<AppDomainResult> GetById(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            var item = await this.catalogueService.GetByIdAsync(id);
            if (item != null)
            {
                var itemModel = mapper.Map<T>(item);
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
        public override async Task<AppDomainResult> AddItem([FromBody] R itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = false;
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(itemModel.Code))
                    itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Name).ToLower().Replace(" ", "-");
                var item = mapper.Map<E>(itemModel);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await this.catalogueService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new KeyNotFoundException(messageUserCheck);
                    success = await this.catalogueService.CreateAsync(item);
                    if (success)
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
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
        public override async Task<AppDomainResult> UpdateItem([FromBody] R itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = false;
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(itemModel.Code))
                    itemModel.Code = AppUtilities.RemoveUnicode(itemModel.Name).ToLower().Replace(" ", "-");
                var item = mapper.Map<E>(itemModel);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await this.catalogueService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new KeyNotFoundException(messageUserCheck);
                    success = await this.catalogueService.UpdateAsync(item);
                    if (success)
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
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
        public override async Task<AppDomainResult> DeleteItem(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = await this.catalogueService.DeleteAsync(id);
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
        [AppAuthorize(new int[] { CoreContants.ViewAll })]
        public override async Task<AppDomainResult> Get([FromQuery] DomainSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                PagedList<E> pagedData = await this.catalogueService.GetPagedListData(baseSearch);
                PagedList<T> pagedDataModel = mapper.Map<PagedList<T>>(pagedData);
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

        ///// <summary>
        ///// Down load template file import
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("download-template-import")]
        //[AppAuthorize(new int[] { CoreContants.Download })]
        //public virtual async Task<ActionResult> DownloadTemplateImport(string fileName)
        //{
        //    var currentDirectory = System.IO.Directory.GetCurrentDirectory();
        //    string path = System.IO.Path.Combine(currentDirectory, CoreContants.TEMPLATE_FOLDER_NAME, CoreContants.CATALOGUE_TEMPLATE_NAME);
        //    if (!System.IO.File.Exists(path))
        //        throw new AppException("File template không tồn tại!");
        //    var file = await System.IO.File.ReadAllBytesAsync(path);
        //    return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TemplateImport.xlsx");
        //}

        ///// <summary>
        ///// Tải về file kết quả sau khi import
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //[HttpGet("download-import-result-file/{fileName}")]
        //[AppAuthorize(new int[] { CoreContants.Download })]
        //public virtual async Task<ActionResult> DownloadImportFileResult(string fileName)
        //{
        //    if (string.IsNullOrEmpty(fileName))
        //        throw new AppException("File name không tồn tại!");
        //    if (env == null)
        //        throw new AppException("IHostingEnvironment is null => inject to constructor");
        //    var webRoot = env.ContentRootPath;
        //    string path = Path.Combine(webRoot, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.TEMP_FOLDER_NAME, fileName);
        //    var file = await System.IO.File.ReadAllBytesAsync(path);
        //    // Xóa file thư mục temp
        //    if (System.IO.File.Exists(path))
        //        System.IO.File.Delete(path);
        //    return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("KetQuaCD-{0:dd-MM-yyyy_HH_mm_ss}{1}", DateTime.UtcNow.AddHours(7), Path.GetExtension(fileName)));
        //}

        ///// <summary>
        ///// Import file danh mục
        ///// </summary>
        ///// <param name="file"></param>
        ///// <returns></returns>
        //[HttpPost("import-template-file")]
        //[AppAuthorize(new int[] { CoreContants.Import })]
        //public virtual async Task<AppDomainImportResult> ImportTemplateFile(IFormFile file)
        //{
        //    AppDomainImportResult appDomainImportResult = new AppDomainImportResult();
        //    var fileStream = file.OpenReadStream();
        //    appDomainImportResult = await this.catalogueService.ImportTemplateFile(fileStream, LoginContext.Instance.CurrentUser.UserName);
        //    if (appDomainImportResult.ResultFile != null)
        //    {
        //        var webRoot = env.ContentRootPath;
        //        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        //        string path = Path.Combine(webRoot, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.TEMP_FOLDER_NAME, fileName);
        //        FileUtilities.CreateDirectory(Path.Combine(webRoot, CoreContants.UPLOAD_FOLDER_NAME));
        //        FileUtilities.SaveToPath(path, appDomainImportResult.ResultFile);
        //        appDomainImportResult.ResultFile = null;
        //        appDomainImportResult.DownloadFileName = fileName;
        //    }
        //    return appDomainImportResult;
        //}
    }
}
