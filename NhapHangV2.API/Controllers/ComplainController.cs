using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Auth;
using NhapHangV2.Entities.Catalogue;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Models;
using NhapHangV2.Request;
using NhapHangV2.Service;
using NhapHangV2.Service.Services.Auth;
using NhapHangV2.Service.Services.Configurations;
using NhapHangV2.Utilities;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.API.Controllers
{
    [Route("api/complain")]
    [ApiController]
    [Description("Khiếu nại")]
    [Authorize]
    public class ComplainController : BaseController<Complain, ComplainModel, ComplainRequest, ComplainSearch>
    {
        protected readonly IComplainService complainService;
        private readonly IConfiguration configuration;
        protected IUserInGroupService userInGroupService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly INotificationTemplateService notificationTemplateService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly ISMSEmailTemplateService sMSEmailTemplateService;
        private readonly IMainOrderService mainOrderService;
        private readonly ITransportationOrderService transportationOrderService;
        public ComplainController(IServiceProvider serviceProvider, ILogger<BaseController<Complain, ComplainModel, ComplainRequest, ComplainSearch>> logger, IWebHostEnvironment env, IConfiguration configuration) : base(serviceProvider, logger, env)
        {
            this.domainService = this.serviceProvider.GetRequiredService<IComplainService>();
            complainService = serviceProvider.GetRequiredService<IComplainService>();
            this.configuration = configuration;
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            notificationTemplateService = serviceProvider.GetRequiredService<INotificationTemplateService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            sMSEmailTemplateService = serviceProvider.GetRequiredService<ISMSEmailTemplateService>();
            transportationOrderService = serviceProvider.GetRequiredService<ITransportationOrderService>();

        }

        /// <summary>
        /// Thêm mới item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public override async Task<AppDomainResult> AddItem([FromBody] ComplainRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = false;
            if (ModelState.IsValid)
            {
                decimal? maxAmountRequest = 0;

                var mainOrderComplain = new MainOrder();
                var transOrderComplain = new TransportationOrder();
                if (itemModel.MainOrderId > 0)
                {
                    mainOrderComplain = await mainOrderService.GetByIdAsync(itemModel?.MainOrderId ?? 0);
                }
                else if (itemModel.TransportationOrderId > 0)
                {
                    transOrderComplain = await transportationOrderService.GetByIdAsync(itemModel?.TransportationOrderId ?? 0);
                }
                if (mainOrderComplain == null && transOrderComplain == null)
                    throw new KeyNotFoundException("Đơn hàng không tồn tại");
                else if (mainOrderComplain != null)
                    maxAmountRequest = mainOrderComplain.TotalPriceVND;
                else if (transOrderComplain != null)
                    maxAmountRequest = transOrderComplain.TotalPriceVND;

                if (itemModel.Amount > maxAmountRequest)
                    throw new AppException("Số tiền yêu cầu lớn hơn tổng tiền đơn hàng");
                var item = mapper.Map<Complain>(itemModel);

                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await this.domainService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new KeyNotFoundException(messageUserCheck);
                    success = await this.domainService.CreateAsync(item);
                    if (success)
                    {
                        #region Thông báo khiếu nại cho Admin và Manager
                        var notificationSetting = await notificationSettingService.GetByIdAsync(10);
                        var notiTemplate = await notificationTemplateService.GetByIdAsync(2);
                        if (notiTemplate != null && notificationSetting.Active)
                        {
                            var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync("AKNM");
                            string subject = emailTemplate.Subject;
                            string emailContent = string.Format(emailTemplate.Body);
                            await sendNotificationService.SendNotification(notificationSetting, notiTemplate, item.MainOrderId.ToString(), string.Format(Complain_Admin), "", null, subject, emailContent); //Thông báo Email
                        }
                        #endregion
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
        /// Cập nhật các trạng thái như đang xử lý, hủy,...
        /// 0: đã hủy, 1: chưa duyệt, 2: đang xử lý, 3 đã xử lý
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("update-complain")]
        public async Task<AppDomainResult> UpdateComplain([FromBody] FieldForUpdatedComplainRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            success = await complainService.UpdateStatus(itemModel.Id, itemModel.Amount, itemModel.Status);
            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            }
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
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] ComplainSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<ComplainModel> pagedListModel = new PagedList<ComplainModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<Complain> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<ComplainModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("ComplainTemplate.xlsx");
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new ComplainModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);
            // Xuất biểu đồ nếu có
            //fileByteReport = await this.ExportChart(fileByteReport, pagedListModel.Items);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "Complain");
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

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<ComplainModel> listData)
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
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<ComplainModel> pagedList, ComplainSearch baseSearch)
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
