using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities.Report;
using NhapHangV2.Entities.Search.Report;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services.Report;
using NhapHangV2.Models.Report;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NhapHangV2.API.Controllers.Report
{
    [Route("api/report-main-order")]
    [ApiController]
    [Description("Thống kê đơn hàng - Thống kê lợi nhuận mua hàng hộ")]
    [Authorize]
    public class MainOrderReportController : BaseReportController<MainOrderReport, MainOrderReportModel, MainOrderReportSearch>
    {
        private IMainOrderReportService mainOrderReportService;
        public MainOrderReportController(IServiceProvider serviceProvider, ILogger<BaseReportController<MainOrderReport, MainOrderReportModel, MainOrderReportSearch>> logger, IWebHostEnvironment env, IConfiguration configuration) : base(serviceProvider, logger, env, configuration)
        {
            this.domainService = serviceProvider.GetRequiredService<IMainOrderReportService>();
            mainOrderReportService = serviceProvider.GetRequiredService<IMainOrderReportService>();
        }

        protected override string GetTemplateFilePath(string fileTemplateName)
        {
            return base.GetTemplateFilePath("MainOrderReportTemplate.xlsx");
        }

        protected override string GetReportName()
        {
            return "MainOrder_Report";
        }



        
        [Description("Thống kê tống quát đơn hàng - doanh thu")]
        [HttpGet("get-total-overview")]
        [AppAuthorize(new int[] { CoreContants.ViewAll })]
        public virtual async Task<AppDomainResult> GetTotalRevenue([FromQuery] MainOrderReportSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                List<MainOrderReportOverView> pagedData = await mainOrderReportService.GetRevenueOverview(baseSearch);
               // PagedList<MainOrderReportModel> pagedDataModel = mapper.Map<PagedList<MainOrderReportModel>>(pagedData);
                appDomainResult = new AppDomainResult
                {
                    Data = pagedData,
                    Success = true,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Xuất báo cáo
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpPost("export-profit")]
        public async Task<AppDomainResult> ExportReport([FromQuery] MainOrderReportSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<MainOrderReportModel> pagedListModel = new PagedList<MainOrderReportModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<MainOrderReport> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<MainOrderReportModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(GetTemplateFilePath2("MainOrderReportProfitTemplate.xlsx"));

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new MainOrderReportModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), GetReportName());
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

        /// <summary>
        /// Lấy đường dẫn file template
        /// </summary>
        /// <param name="fileTemplateName"></param>
        /// <returns></returns>
        protected string GetTemplateFilePath2(string fileTemplateName)
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string path = System.IO.Path.Combine(currentDirectory, CoreContants.TEMPLATE_FOLDER_NAME, fileTemplateName);
            if (!System.IO.File.Exists(path))
                throw new AppException("File template không tồn tại!");
            return path;
        }

    }
}
