using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Models.Catalogue;
using NhapHangV2.Request;
using NhapHangV2.Request.Catalogue;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.API.Controllers.Catalogue
{
    [Route("api/big-package")]
    [ApiController]
    [Description("Quản lý bao hàng")]
    [Authorize]
    public class BigPackageController : BaseController<BigPackage, BigPackageModel, BigPackageRequest, BigPackageSearch>
    {
        private IConfiguration configuration;
        protected readonly ISmallPackageService smallPackageService;

        public BigPackageController(IServiceProvider serviceProvider, ILogger<BaseController<BigPackage, BigPackageModel, BigPackageRequest, BigPackageSearch>> logger, IWebHostEnvironment env, IConfiguration configuration) : base(serviceProvider, logger, env)
        {
            this.configuration = configuration;
            this.domainService = serviceProvider.GetRequiredService<IBigPackageService>();
            smallPackageService = serviceProvider.GetRequiredService<ISmallPackageService>();

        }

        /// <summary>
        /// Thêm bao lớn
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpPost]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public override async Task<AppDomainResult> AddItem([FromBody] BigPackageRequest itemModel)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            var checkItem = await this.domainService.GetByIdAsync(itemModel.Id);
            if (checkItem != null)
                throw new AppException("Bao lớn đã tồn tại");
            var item = mapper.Map<BigPackage>(itemModel);
            bool success = await this.domainService.CreateAsync(item);
            if (!success)
                throw new AppException("Lỗi trong quá trình xử lý");
            return new AppDomainResult() { ResultCode = (int)HttpStatusCode.OK, ResultMessage = "Thêm bao lớn thành công", Success = success };
        }

        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public override async Task<AppDomainResult> UpdateItem([FromBody] BigPackageRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            bool updateSmallPackageResult = false;
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            var item = await this.domainService.GetByIdAsync(itemModel.Id);
            if (item == null)
                throw new KeyNotFoundException("Bao lớn không tồn tại");
            switch (itemModel.Status ?? 0)
            {
                case (int)StatusBigPackage.DangChuyenVe:
                    updateSmallPackageResult = await ChangeSmallPackgeStatus(itemModel, (int)StatusSmallPackage.DaVeKhoTQ);
                    if (!updateSmallPackageResult)
                        throw new AppException("Cập nhật trạng thái mã vận đơn thất bại");
                    break;
                case (int)StatusBigPackage.DaNhanHang:
                    updateSmallPackageResult = await ChangeSmallPackgeStatus(itemModel, (int)StatusSmallPackage.DaVeKhoVN);
                    if (!updateSmallPackageResult)
                        throw new AppException("Cập nhật trạng thái mã vận đơn thất bại");
                    break;
                case (int)StatusBigPackage.Huy:
                default:
                    updateSmallPackageResult = await ChangeSmallPackgeStatus(itemModel, (int)StatusSmallPackage.DaHuy);
                    if (!updateSmallPackageResult)
                        throw new AppException("Cập nhật trạng thái mã vận đơn thất bại");
                    break;
            }

            mapper.Map(itemModel, item);
            success = await this.domainService.UpdateAsync(item);
            if (success)
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;

            return appDomainResult;
        }
        #region Excel

        /// <summary>
        /// Xuất Excel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("export-excel")]
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] int id)
        {
            string fileResultPath = string.Empty;
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            var item = await this.domainService.GetByIdAsync(id);
            var itemModel = mapper.Map<BigPackageModel>(item);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("BigPackageTemplate.xlsx");
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(itemModel, id);
            if (itemModel.SmallPackages == null || !itemModel.SmallPackages.Any())
                itemModel.SmallPackages.Add(new Models.SmallPackageModel());
            byte[] fileByteReport = excelUtility.Export(itemModel.SmallPackages);
            // Xuất biểu đồ nếu có
            //fileByteReport = await this.ExportChart(fileByteReport, itemModel.SmallPackages);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "BigPackage");
            string filePath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.EXCEL_FOLDER_NAME, fileName);

            string folderUploadPath = string.Empty;
            var folderUpload = configuration.GetValue<string>("MySettings:FolderUpload");
            folderUploadPath = Path.Combine(folderUpload, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.EXCEL_FOLDER_NAME);
            string fileUploadPath = Path.Combine(folderUploadPath, Path.GetFileName(filePath));

            FileUtilities.CreateDirectory(folderUploadPath);
            FileUtilities.SaveToPath(fileUploadPath, fileByteReport);

            var currentLinkSite = $"{Extensions.HttpContext.Current.Request.Scheme}://{Extensions.HttpContext.Current.Request.Host}/{CoreContants.EXCEL_FOLDER_NAME}/";
            fileResultPath = Path.Combine(currentLinkSite, Path.GetFileName(filePath));

            // 5. TRẢ ĐƯỜNG DẪN FILE CHO CLIENT DOWN VỀ
            return new AppDomainResult()
            {
                Data = fileResultPath,
                ResultCode = (int)HttpStatusCode.OK,
                Success = true,
            };
        }

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<Models.SmallPackageModel> listData)
        //{
        //    return excelData;
        //}

        /// <summary>
        /// Lấy đường dẫn file template
        /// </summary>
        /// <param name="fileTemplateName"></param>
        /// <returns></returns>
        protected virtual string GetTemplateFilePath(string fileTemplateName)
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string path = System.IO.Path.Combine(currentDirectory, CoreContants.TEMPLATE_FOLDER_NAME, fileTemplateName);
            if (!System.IO.File.Exists(path))
                throw new AppException("File template không tồn tại!");
            return path;
        }

        /// <summary>
        /// Lấy thông số parameter truyền vào
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(BigPackageModel itemModel, int id)
        {
            return await Task.Run(() =>
            {
                IDictionary<string, object> dictionaries = new Dictionary<string, object>();
                return dictionaries;
            });
        }

        #endregion

        private async Task<bool> ChangeSmallPackgeStatus(BigPackageRequest itemModel, int status)
        {
            bool success = false;
            if (itemModel.SmallPackages.Count > 0)
            {
                foreach (var item in itemModel.SmallPackages)
                {
                    var smallPackage = await smallPackageService.GetByIdAsync(item.Id);
                    if (smallPackage != null)
                    {
                        smallPackage.BigPackageId = null;
                        success = await smallPackageService.UpdateAsync(smallPackage);
                    }
                }
            }
            else
                success = true;
            return success;
        }
    }
}
