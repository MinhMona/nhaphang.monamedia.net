using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Models;
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
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.API.Controllers
{
    [Route("api/pay-help")]
    [ApiController]
    [Description("Yêu cầu thanh toán hộ")]
    [Authorize]
    public class PayHelpController : BaseController<PayHelp, PayHelpModel, PayHelpRequest, PayHelpSearch>
    {
        protected IConfiguration configuration;
        protected readonly IPayHelpService payHelpService;
        protected readonly IPayHelpDetailService payHelpDetailService;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IUserService userService;
        protected readonly IHistoryServicesService historyServicesService;
        public PayHelpController(IServiceProvider serviceProvider, ILogger<BaseController<PayHelp, PayHelpModel, PayHelpRequest, PayHelpSearch>> logger, IWebHostEnvironment env, IConfiguration configuration) : base(serviceProvider, logger, env)
        {
            this.configuration = configuration;
            this.domainService = this.serviceProvider.GetRequiredService<IPayHelpService>();
            payHelpService = serviceProvider.GetRequiredService<IPayHelpService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            userService = serviceProvider.GetRequiredService<IUserService>();
            historyServicesService = serviceProvider.GetRequiredService<IHistoryServicesService>();
            payHelpDetailService = serviceProvider.GetRequiredService<IPayHelpDetailService>();
        }

        /// <summary>
        /// Cập nhật tỷ giá
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("currency")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateCurrency([FromBody] PayHelpRequest itemModel)
        {
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = await payHelpService.UpdateCurrency(itemModel.Id, itemModel.Currency);
            if (success)
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;

            return appDomainResult;
        }


        [HttpGet("count-status")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public AppDomainResult CountStatus([FromQuery] PayHelpSearch payHelpSearch)
        {
            var transportationsInfor = payHelpService.CountStatus(payHelpSearch);
            return new AppDomainResult
            {
                Data = transportationsInfor,
                ResultCode = (int)HttpStatusCode.OK,
                Success = true
            };
        }

        /// <summary>
        /// Cập nhật các trạng thái như Xóa, Thanh toán của User
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("update-pay-help")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdatePayHelp([FromBody] PayHelpRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            var item = await this.domainService.GetByIdAsync(itemModel.Id);
            item.PayHelpDetails = mapper.Map<List<PayHelpDetail>>(itemModel.PayHelpDetails);

            if (item != null)
            {
                success = await payHelpService.UpdateStatus(item, itemModel.Status ?? 0, item.Status ?? 0, item.TotalPriceVND);
                if (success)
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                else
                    throw new Exception("Lỗi trong quá trình xử lý");
                appDomainResult.Success = success;
            }
            else
            {
                throw new KeyNotFoundException("Item không tồn tại");
            }
            return appDomainResult;
        }

        /// <summary>
        /// Lấy tỷ giá thanh toán hộ theo cấu hình
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-exchange-rate/{price}")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetCurrency(decimal price)
        {
            return new AppDomainResult()
            {
                Data = await configurationsService.GetCurrentPayHelp(price),
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public override async Task<AppDomainResult> UpdateItem([FromBody] PayHelpRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var item = await this.domainService.GetByIdAsync(itemModel.Id);
                if (item == null)
                    throw new KeyNotFoundException("Item không tồn tại");
                decimal? totalPriceVNDOld = item.TotalPriceVND;
                item.Note = itemModel.Note;
                item.SalerID = itemModel.SalerID;
                item.TotalPriceVND = itemModel.TotalPriceVND;
                item.TotalPriceVND += ((itemModel.FeeService ?? 0) - (item.FeeService ?? 0));
                item.Currency = itemModel.Currency;
                item.CurrencyConfig = itemModel.CurrencyConfig;
                item.TotalPriceVNDGiaGoc = itemModel.TotalPriceVNDGiaGoc;
                item.FeeService = itemModel.FeeService;
                item.PayHelpDetails = mapper.Map<List<PayHelpDetail>>(itemModel.PayHelpDetails);
                success = await payHelpService.UpdateStatus(item, itemModel.Status ?? 0, item.Status ?? 0, totalPriceVNDOld);
                if (success)
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                else
                    throw new Exception("Lỗi trong quá trình xử lý");
                appDomainResult.Success = success;
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }
        /// <summary>
        /// Cập nhật thông tin duyệt đơn
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("confirm")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateConfirm([FromBody] PayHelpRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            var item = await this.domainService.GetByIdAsync(itemModel.Id);
            if (item == null)
                throw new KeyNotFoundException("Item không tồn tại");
            item.Status = (int)StatusPayHelp.DaDuyet;
            if (item.ConfirmDate == null)
                item.ConfirmDate = DateTime.Now;
            success = await this.domainService.UpdateFieldAsync(item, new Expression<Func<PayHelp, object>>[]
            {
                    s => s.Status,
                    s => s.ConfirmDate,
                    s => s.Updated,
                    s => s.UpdatedBy,
            });
            if (success)
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;



            return appDomainResult;
        }
        /// <summary>
        /// Cập nhật nhân viên sale
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("update-staff")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateStaff([FromBody] PayHelpRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            var item = await this.domainService.GetByIdAsync(itemModel.Id);
            if (item == null)
                throw new KeyNotFoundException("Item không tồn tại");
            int oldSalerId = item.SalerID ?? 0;
            item.SalerID = itemModel.SalerID;
            success = await payHelpService.UpdateStaff(item, oldSalerId);
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
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpPost("export-excel")]
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] PayHelpSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<PayHelpModel> pagedListModel = new PagedList<PayHelpModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<PayHelp> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<PayHelpModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("PayHelpTemplate.xlsx");
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new PayHelpModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);
            // Xuất biểu đồ nếu có
            //fileByteReport = await this.ExportChart(fileByteReport, pagedListModel.Items);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "PayHelp");
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

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<PayHelpModel> listData)
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
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<PayHelpModel> pagedList, PayHelpSearch baseSearch)
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
