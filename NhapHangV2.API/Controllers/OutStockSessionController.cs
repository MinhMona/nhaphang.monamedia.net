using NhapHangV2.Entities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using NhapHangV2.Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using NhapHangV2.Request;
using NhapHangV2.Entities.Search;
using NhapHangV2.Entities.Auth;
using NhapHangV2.BaseAPI.Controllers;

namespace NhapHangV2.API.Controllers
{
    [Route("api/out-stock-session")]
    [ApiController]
    [Description("Xuất kho")]
    [Authorize]
    public class OutStockSessionController : BaseController<OutStockSession, OutStockSessionModel, OutStockSessionRequest, OutStockSessionSearch>
    {
        private IConfiguration configuration;
        protected readonly IOutStockSessionService outStockSessionService;
        public OutStockSessionController(IServiceProvider serviceProvider, ILogger<BaseController<OutStockSession, OutStockSessionModel, OutStockSessionRequest, OutStockSessionSearch>> logger, IWebHostEnvironment env, IConfiguration configuration) : base(serviceProvider, logger, env)
        {
            this.configuration = configuration;
            this.domainService = serviceProvider.GetRequiredService<IOutStockSessionService>();
            outStockSessionService = serviceProvider.GetRequiredService<IOutStockSessionService>();
        }

        /// <summary>
        /// Cập nhật các trạng thái như Thanh toán bằng ví, thanh toán bằng tiền mặt
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("update-status")]
        public async Task<AppDomainResult> UpdateStatus([FromBody] FieldForUpdatedOutStockSessionRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            success = await outStockSessionService.UpdateStatus(itemModel.Id, itemModel.Status, itemModel.isPaymentWallet);
            if (success)
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;

            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật các trạng thái
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("status")]
        public async Task<AppDomainResult> UpdateOutStockStatus([FromBody] OutStockSessionRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            success = await outStockSessionService.UpdateOutStockStatus(itemModel.Id, itemModel.Status ?? 0);
            if (success)
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;

            return appDomainResult;
        }

        /// <summary>
        /// Ẩn đơn chưa thanh toán
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete-not-payment")]
        public async Task<AppDomainResult> DeleteNotPayment(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (id == 0)
                throw new KeyNotFoundException("Id không tồn tại");
            var item = await this.domainService.GetByIdAsync(id);
            if (item == null)
                throw new KeyNotFoundException("Item không tồn tại");
            success = await outStockSessionService.DeleteNotPayment(item);
            if (success)
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;
            return appDomainResult;
        }

        /// <summary>
        /// Xuất kho
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("export")]
        public async Task<AppDomainResult> Export(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = await outStockSessionService.Export(id);
            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                appDomainResult.Success = success;
            }
            else
                throw new Exception("Lỗi trong quá trình xử lý");

            return appDomainResult;
        }

        #region Excel

        /// <summary>
        /// Xuất Excel
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpPost("export-excel")]
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] OutStockSessionSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<OutStockSessionModel> pagedListModel = new PagedList<OutStockSessionModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<OutStockSession> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<OutStockSessionModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("OutStockSessionTemplate.xlsx");
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new OutStockSessionModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);
            // Xuất biểu đồ nếu có
            //fileByteReport = await this.ExportChart(fileByteReport, pagedListModel.Items);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "OutStockSession");
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

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<OutStockSessionModel> listData)
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
        /// <param name="pagedList"></param>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<OutStockSessionModel> pagedList, OutStockSessionSearch baseSearch)
        {
            return await Task.Run(() =>
            {
                IDictionary<string, object> dictionaries = new Dictionary<string, object>();
                return dictionaries;
            });
        }

        #endregion
    }
}
