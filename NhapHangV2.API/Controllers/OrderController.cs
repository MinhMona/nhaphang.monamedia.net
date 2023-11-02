using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Models;
using NhapHangV2.Request;
using NhapHangV2.Service.Services.Configurations;
using NhapHangV2.Utilities;
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
    [Route("api/order")]
    [Description("Quản lý sản phẩm")]
    [ApiController]
    [Authorize]
    public class OrderController : BaseController<Order, OrderModel, OrderRequest, OrderSearch>
    {
        protected readonly IUserService userService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly INotificationTemplateService notificationTemplateService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly IMainOrderService mainOrderService;
        private readonly ISMSEmailTemplateService sMSEmailTemplateService;
        protected readonly IConfiguration configuration;

        public OrderController(IServiceProvider serviceProvider, ILogger<BaseController<Order, OrderModel, OrderRequest, OrderSearch>> logger, IWebHostEnvironment env) : base(serviceProvider, logger, env)
        {
            this.domainService = this.serviceProvider.GetRequiredService<IOrderService>();
            configuration = serviceProvider.GetRequiredService<IConfiguration>();
            userService = this.serviceProvider.GetRequiredService<IUserService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            notificationTemplateService = serviceProvider.GetRequiredService<INotificationTemplateService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            sMSEmailTemplateService = serviceProvider.GetRequiredService<ISMSEmailTemplateService>();

        }

        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public override async Task<AppDomainResult> UpdateItem([FromBody] OrderRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
                var item = await this.domainService.GetByIdAsync(itemModel.Id);

                if (item != null)
                {
                    var mainOrder = await mainOrderService.GetByIdAsync(item.MainOrderId ?? 0);
                    if (mainOrder == null)
                    {
                        throw new AppException("Không tìm thấy đơn hàng");
                    }
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await this.domainService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new AppException(messageUserCheck);
                    if (itemModel.ProductStatus == 2) //Hết hàng
                    {
                        itemModel.Quantity = 0; //Số lượng = 0
                        itemModel.PriceOrigin = itemModel.RealPrice = 0; //Giá sản phẩm, giá mua thực tế = 0
                    }

                    decimal price = item.PriceOrigin ?? 0;
                    if (item.PricePromotion > 0 && item.PricePromotion < item.PriceOrigin)
                        price = item.PricePromotion ?? 0;

                    item.HistoryOrderChanges = new List<HistoryOrderChange>();
                    var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.SanPhamMuaHo);

                    if (itemModel.PriceOrigin != price)
                    {
                        item.HistoryOrderChanges.Add(new HistoryOrderChange()
                        {
                            MainOrderId = item.MainOrderId,
                            UID = user.Id,
                            HistoryContent = String.Format("{0} đã đổi giá sản phẩm ID #{1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, price.ToString(), itemModel.PriceOrigin.ToString()),
                            Type = (int?)TypeHistoryOrderChange.TienDatCoc
                        });
                        //Thông báo
                        sendNotificationService.SendNotification(notificationSetting,
                            new List<string>() { item.MainOrderId.ToString(), user.UserName, item.Id.ToString() },
                            new UserNotification() { UserId = mainOrder.UID, OrdererId = mainOrder.DatHangId, SaleId = mainOrder.SalerId });
                    }

                    if (itemModel.Quantity != item.Quantity)
                    {
                        item.HistoryOrderChanges.Add(new HistoryOrderChange()
                        {
                            MainOrderId = item.MainOrderId,
                            UID = user.Id,
                            HistoryContent = String.Format("{0} đã đổi số lượng sản phẩm ID #{1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, item.Quantity.ToString(), itemModel.Quantity.ToString()),
                            Type = (int?)TypeHistoryOrderChange.TienDatCoc
                        });
                        sendNotificationService.SendNotification(notificationSetting,
                            new List<string>() { item.MainOrderId.ToString(), user.UserName, item.Id.ToString() },
                            new UserNotification() { UserId = mainOrder.UID, OrdererId = mainOrder.DatHangId, SaleId = mainOrder.SalerId });
                    }

                    if (itemModel.ProductStatus != item.ProductStatus)
                    {
                        string productStatusNew = "";
                        switch (itemModel.ProductStatus)
                        {
                            case 1:
                                productStatusNew = "Còn hàng";
                                break;
                            case 2:
                                productStatusNew = "Hết hàng";
                                break;
                            default:
                                break;
                        }

                        string productStatusOld = "";
                        switch (item.ProductStatus)
                        {
                            case 1:
                                productStatusOld = "Còn hàng";
                                break;
                            case 2:
                                productStatusOld = "Hết hàng";
                                break;
                            default:
                                break;
                        }

                        item.HistoryOrderChanges.Add(new HistoryOrderChange()
                        {
                            MainOrderId = item.MainOrderId,
                            UID = user.Id,
                            HistoryContent = String.Format("{0} đã đổi trạng thái sản phẩm của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.MainOrderId, productStatusOld, productStatusNew),
                            Type = (int?)TypeHistoryOrderChange.TienDatCoc
                        });
                        sendNotificationService.SendNotification(notificationSetting,
                            new List<string>() { item.MainOrderId.ToString(), user.UserName, item.Id.ToString() },
                            new UserNotification() { UserId = mainOrder.UID, OrdererId = mainOrder.DatHangId, SaleId = mainOrder.SalerId });
                    }
                    item.PriceCNY = itemModel.PriceOrigin * itemModel.Quantity;
                    item.PriceVND = (itemModel.PriceOrigin * itemModel.Quantity) * item.CurrentCNYVN;
                    item.PricePromotion = itemModel.PriceOrigin;
                    mapper.Map(itemModel, item);

                    success = await this.domainService.UpdateAsync(item);

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
        /// Xuất Excel
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpPost("export-excel")]
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] OrderSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<OrderModel> pagedListModel = new PagedList<OrderModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<Order> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<OrderModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("OrderTemplate.xlsx"); //Danh sách đơn hàng mua hộ
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new OrderModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);
            // Xuất biểu đồ nếu có

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "Order");
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
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<OrderModel> pagedList, OrderSearch baseSearch)
        {
            return await Task.Run(() =>
            {
                IDictionary<string, object> dictionaries = new Dictionary<string, object>();
                return dictionaries;
            });
        }
    }
}
