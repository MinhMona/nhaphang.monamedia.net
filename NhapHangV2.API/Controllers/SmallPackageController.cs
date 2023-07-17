using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Models;
using NhapHangV2.Request;
using NhapHangV2.Service;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;
using SmallPackageToolRequest = NhapHangV2.Entities.SmallPackageToolRequest;

namespace NhapHangV2.API.Controllers
{
    [Route("api/small-package")]
    [ApiController]
    [Description("Kiện trôi nổi")]
    public class SmallPackageController : ControllerBase
    {
        protected IMapper mapper;
        protected IWebHostEnvironment env;
        protected IConfiguration configuration;

        protected readonly IUserService userService;
        protected readonly ISmallPackageService smallPackageService;
        protected readonly IBigPackageService bigPackageService;
        protected readonly IMainOrderService mainOrderService;
        protected readonly ITransportationOrderService transportationOrderService;
        protected readonly IHubContext<DomainHub, IDomainHub> hubContext;

        public SmallPackageController(IServiceProvider serviceProvider, ILogger<SmallPackageController> logger, IWebHostEnvironment env, IMapper mapper, IConfiguration configuration, IHubContext<DomainHub, IDomainHub> hubContext)
        {
            this.mapper = mapper;
            this.env = env;
            this.configuration = configuration;
            userService = serviceProvider.GetRequiredService<IUserService>();
            smallPackageService = serviceProvider.GetRequiredService<ISmallPackageService>();
            bigPackageService = serviceProvider.GetRequiredService<IBigPackageService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            transportationOrderService = serviceProvider.GetRequiredService<ITransportationOrderService>();
            this.hubContext = hubContext;
        }


        /// <summary>
        /// Create tool
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        [HttpPost("tool")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public AppDomainResult CreateTool([FromBody] List<SmallPackageToolRequest> requests)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException(ModelState.GetErrorMessage());
            }
            smallPackageService.CreateSmallPackageTool(requests);
            return new AppDomainResult()
            {
                ResultCode = (int)HttpStatusCode.OK,
                Success = true
            };
        }


        /// <summary>
        /// Xác nhận lệnh trôi nổi (User)
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("confirm")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> Confirm([FromBody] ConfirmSmallPackageRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var smallPackage = await smallPackageService.GetByIdAsync(itemModel.Id);
                if (smallPackage == null)
                    throw new KeyNotFoundException("Không tìm thấy kiện trôi nổi");

                var users = await userService.GetSingleAsync(x => x.UserName == LoginContext.Instance.CurrentUser.UserName);
                var currentDate = DateTime.Now;

                smallPackage.Updated = currentDate;
                smallPackage.UpdatedBy = users.UserName;

                smallPackage.FloatingUserName = users.UserName;
                smallPackage.FloatingUserPhone = itemModel.Phone;
                smallPackage.UserNote = itemModel.Note;
                smallPackage.FloatingStatus = (int)StatusConfirmSmallPackage.DangChoXacNhan;
                smallPackage.IMG = itemModel.Images.ElementAt(0);

                success = await smallPackageService.UpdateAsync(smallPackage);
                if (success)
                {
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                }

                appDomainResult.Success = success;
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Tracking theo mã vận đơn cho User (Kiểm hàng cho Quản lý )
        /// </summary>
        /// <param name="transactionCode"></param>
        /// <param name="isAssign">Gán kiện ký gửi cho khách</param>
        /// <returns></returns>
        [HttpGet("get-by-transaction-code")]
        //[AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> Tracking(string transactionCode, bool isAssign)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (string.IsNullOrEmpty(transactionCode))
                throw new KeyNotFoundException("Code không tồn tại");

            IList<SmallPackage> items = new List<SmallPackage>();

            var bigPackage = await bigPackageService.GetSingleAsync(e => !e.Deleted
                        && (e.Code == transactionCode));

            if (bigPackage != null)
            {
                items = await smallPackageService.GetAsync(x => !x.Deleted
                    && (x.BigPackageId == bigPackage.Id));
            }
            else
            {
                items = await smallPackageService.GetAsync(x => !x.Deleted
                    && (x.OrderTransactionCode == transactionCode));
            }

            if (items.Any())
            {
                items = await smallPackageService.CheckBarCode(items.ToList(), isAssign);
                var itemModel = mapper.Map<List<SmallPackageModel>>(items);
                for (int i = 0; i < itemModel.Count; i++)
                {
                    var bigPackageName = await bigPackageService.GetByIdAsync(itemModel[i].BigPackageId ?? 0);
                    if (bigPackageName != null)
                        itemModel[i].BigPackageName = bigPackageName.Name;
                }
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
        /// Xuất kho dựa vào mã barcode (mã vận đơn)
        /// </summary>
        /// <param name="uid">UID người dùng</param>
        /// <param name="barcode">Mã vận đơn</param>
        /// <param name="statusType">
        /// 1: Xuất kho ký gửi chưa yêu cầu
        /// 2: Xuất kho ký gửi đã yêu cầu
        /// 3: Xuất kho
        /// </param>
        /// <returns></returns>
        [HttpGet("get-export-for-barcode")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> ExportForBarCode(int uid, string barcode, int statusType)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            var items = await smallPackageService.ExportForBarCode(uid, barcode, statusType);
            if (items.Any())
            {
                var itemModels = mapper.Map<List<SmallPackageModel>>(items);
                appDomainResult = new AppDomainResult()
                {
                    Success = true,
                    Data = itemModels,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
                throw new KeyNotFoundException("Item không tồn tại");
            return appDomainResult;
        }

        /// <summary>
        /// Xuất kho dựa vào UserName
        /// </summary>
        /// <param name="UID">UID người dùng</param>
        /// <param name="OrderId">(Xuất kho) Mã đơn hàng </param>
        /// <param name="StatusType">
        /// 1: Xuất kho ký gửi chưa yêu cầu
        /// 2: Xuất kho ký gửi đã yêu cầu
        /// 3: Xuất kho
        /// </param>
        /// <param name="OrderType">(Xuất kho)
        /// 0: Tất cả
        /// 1: ĐH mua hộ
        /// 2: ĐH vận chuyển hộ (Ký gửi)
        /// </param>
        /// <returns></returns>
        [HttpGet("get-export-for-username")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> ExportForUserName(int UID, int OrderId, int StatusType, int OrderType)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            var items = await smallPackageService.ExportForUserName(UID, OrderId, StatusType, OrderType);
            if (items.Any())
            {
                var itemModels = mapper.Map<List<SmallPackageModel>>(items);
                appDomainResult = new AppDomainResult()
                {
                    Success = true,
                    Data = itemModels,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
                throw new KeyNotFoundException("Mã vận đơn không tồn tại");
            return appDomainResult;
        }

        /// <summary>
        /// Thêm mới item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public async Task<AppDomainResult> AddItem([FromBody] SmallPackageRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var item = mapper.Map<SmallPackage>(itemModel);
                if (item != null)
                {
                    DateTime currentDate = DateTime.Now;

                    //Kiểm hàng Trung quốc
                    if (itemModel.IsWarehouseTQ)
                    {
                        item.Status = (int)StatusSmallPackage.DaVeKhoTQ;
                        item.DateInTQWarehouse = item.DateScanTQ = currentDate;
                        item.IsTemp = true;

                        // Kiểm tra item có tồn tại chưa?
                        var smallPackageCheck = await smallPackageService.GetSingleAsync(e => e.OrderTransactionCode == item.OrderTransactionCode);
                        if (smallPackageCheck != null)
                            throw new AppException("Mã vận đã tồn tại");
                        else
                        {
                            if (string.IsNullOrEmpty(item.OrderTransactionCode) || string.IsNullOrWhiteSpace(item.OrderTransactionCode))
                                throw new AppException("Mã vận đơn không hợp lệ");
                        }
                        #region Tách mã vận đơn (chưa dùng tới)
                        //if (smallPackageCheck != null) //Tồn tại
                        //{
                        //    item.UID = smallPackageCheck.UID;
                        //    item.MainOrderId = smallPackageCheck.MainOrderId;
                        //    item.TransportationOrderId = smallPackageCheck.TransportationOrderId;
                        //    string newOrderTransactionCode = smallPackageCheck.OrderTransactionCode + "-" + currentDate.Second.ToString();
                        //    item.OrderTransactionCode = newOrderTransactionCode;
                        //    if (item.TransportationOrderId > 0)
                        //    {
                        //        var orderTrans = await transportationOrderService.GetByIdAsync(item.TransportationOrderId ?? 0);
                        //        orderTrans.OrderTransactionCode = newOrderTransactionCode;
                        //        bool updateResult = await transportationOrderService.UpdateFieldAsync(orderTrans, new Expression<Func<TransportationOrder, object>>[]
                        //        {
                        //            t => t.OrderTransactionCode
                        //        });
                        //        if (!updateResult)
                        //            throw new AppException("Không thể cập nhật mã vận đơn cho đơn ký gửi");
                        //    }
                        //}
                        //else
                        //{
                        //    if (string.IsNullOrEmpty(item.OrderTransactionCode))
                        //    {
                        //        item.OrderTransactionCode = currentDate.Year.ToString() +
                        //            currentDate.Month.ToString() +
                        //            currentDate.Day.ToString() +
                        //            currentDate.Hour.ToString() +
                        //            currentDate.Minute.ToString() +
                        //            currentDate.Second.ToString() +
                        //            currentDate.Millisecond.ToString();
                        //    }
                        //}
                        #endregion
                    }

                    //Kiểm hàng VN
                    if (itemModel.IsWarehouseVN)
                    {
                        item.Status = (int)StatusSmallPackage.DaVeKhoVN;
                        item.DateInLasteWareHouse = item.DateScanVN = currentDate;

                        // Kiểm tra item có tồn tại chưa?
                        var smallPackageCheck = await smallPackageService.GetSingleAsync(e => e.OrderTransactionCode == item.OrderTransactionCode);
                        if (smallPackageCheck != null) //Tồn tại
                        {
                            throw new AppException("Mã vận đã tồn tại");
                        }
                        else
                            throw new KeyNotFoundException("Mã vận đơn không tồn tại");
                    }

                    var exists = await smallPackageService.GetByOrderTransactionCode(itemModel.OrderTransactionCode);
                    if (exists != null)
                        throw new AppException(string.Format("Mã vận đơn {0} đã tồn tại", itemModel.OrderTransactionCode));
                    success = await smallPackageService.CreateAsync(item);
                    if (success)
                    {
                        //Quét mã lại
                        var items = await smallPackageService.CheckBarCode(new List<SmallPackage> { item }, false);

                        appDomainResult.Data = mapper.Map<List<SmallPackageModel>>(items);
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;


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
        /// <param name="itemModels"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateItem([FromBody] List<SmallPackageRequest> itemModels)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                IList<SmallPackage> items = new List<SmallPackage>();
                foreach (var itemModel in itemModels)
                {
                    var item = await smallPackageService.GetByIdAsync(itemModel.Id);
                    if (item != null)
                    {
                        // Kiểm tra item có tồn tại chưa?
                        var messageUserCheck = await smallPackageService.GetExistItemMessage(item);
                        if (!string.IsNullOrEmpty(messageUserCheck))
                            throw new KeyNotFoundException(messageUserCheck);
                        //Cập nhật kiện trôi nổi
                        if (item.IsFloating)
                        {
                            if (item.OrderTransactionCode != itemModel.OrderTransactionCode) // Thay đổi mã vận đơn
                            {
                                var checkCode = await smallPackageService.GetSingleAsync(e => !e.Deleted && e.OrderTransactionCode.Equals(itemModel.OrderTransactionCode));
                                if (checkCode != null)
                                    throw new AppException("Mã vận đơn đã tồn tại, vui lòng nhập mã vận đơn khác");
                            }
                        }
                        mapper.Map(itemModel, item);
                        items.Add(item);
                    }
                    else
                        throw new KeyNotFoundException("Không tìm thấy Item");
                }
                success = await smallPackageService.UpdateAsync(items);
                if (success)
                {
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                }
                appDomainResult.Success = success;
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Chọn kiện thất lạc
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("update-is-lost")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateIsLost(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var item = await smallPackageService.GetByIdAsync(id);
                if (item != null)
                {
                    success = await smallPackageService.UpdateIsLost(item);
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

        #region Excel

        /// <summary>
        /// Template Import Kho TQ
        /// </summary>
        /// <returns></returns>
        [HttpGet("download-template-import")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.Download })]
        public virtual AppDomainResult DownloadTemplateImport()
        {
            string fileResultPath = string.Empty;
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string path = System.IO.Path.Combine(currentDirectory, CoreContants.TEMPLATE_FOLDER_NAME, "ImportCodeTemplate.xlsx");
            if (!System.IO.File.Exists(path))
                throw new AppException("File template không tồn tại!");
            var currentLinkSite = $"{Extensions.HttpContext.Current.Request.Scheme}://{Extensions.HttpContext.Current.Request.Host}/{CoreContants.TEMPLATE_FOLDER_NAME}";
            fileResultPath = Path.Combine(currentLinkSite, Path.GetFileName(path));
            return new AppDomainResult()
            {
                Data = fileResultPath,
                ResultCode = (int)HttpStatusCode.OK,
                Success = true,
            };
        }
        /// <summary>
        /// Import Kho TQ
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("import-template-file")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.Import })]
        public virtual async Task<AppDomainImportResult> ImportTemplateFile(ImportSmallPackageRequest request)
        {
            AppDomainImportResult appDomainImportResult = new AppDomainImportResult();
            using (HttpClient client = new HttpClient())
            {
                using Stream streamToReadFrom = await client.GetStreamAsync(request.FileURL);
                var webRoot = env.ContentRootPath;
                string fileToWriteTo = Path.Combine(webRoot, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.TEMP_FOLDER_NAME, "SmallPackageTemp.xlsx");

                using Stream streamToWriteTo = System.IO.File.Open(fileToWriteTo, FileMode.Create);
                await streamToReadFrom.CopyToAsync(streamToWriteTo);

                appDomainImportResult = await smallPackageService.ImportTemplateFile(streamToWriteTo, request.BigPackageId, LoginContext.Instance.CurrentUser.UserName);
                System.IO.File.Delete(fileToWriteTo);
                string fileInputName = request.FileURL.Split("/").LastOrDefault().ToString();
                string fileInputNamePath = Path.Combine(webRoot, CoreContants.UPLOAD_FOLDER_NAME, fileInputName);
                System.IO.File.Delete(fileInputNamePath);

                return appDomainImportResult;
            }
        }

        /// <summary>
        /// Xuất Excel
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpPost("export-excel")]
        [Authorize]
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] SmallPackageSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<SmallPackageModel> pagedListModel = new PagedList<SmallPackageModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<SmallPackage> pagedData = await smallPackageService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<SmallPackageModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("SmallPackageTemplate.xlsx"); //Quản lý mã vận đơn      
            if (baseSearch.Menu == 0) //Danh sách kiện trôi nổi
                getTemplateFilePath = GetTemplateFilePath("SmallPackageFloatingTemplate.xlsx");
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new SmallPackageModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);
            // Xuất biểu đồ nếu có
            //fileByteReport = await this.ExportChart(fileByteReport, pagedListModel.Items);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "SmallPackage");
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

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<SmallPackageModel> listData)
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
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<SmallPackageModel> pagedList, SmallPackageSearch baseSearch)
        {
            return await Task.Run(() =>
            {
                IDictionary<string, object> dictionaries = new Dictionary<string, object>();
                return dictionaries;
            });
        }
        #endregion

        #region Base Controller
        /// <summary>
        /// Lấy thông tin theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetById(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (id == 0)
            {
                throw new KeyNotFoundException("id không tồn tại");
            }
            var item = await smallPackageService.GetByIdAsync(id);
            if (item != null)
            {
                var itemModel = mapper.Map<SmallPackageModel>(item);
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
        /// Xóa item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.Delete })]
        public async Task<AppDomainResult> DeleteItem(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = await smallPackageService.DeleteAsync(id);
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
        [Authorize]
        [AppAuthorize(new int[] { CoreContants.ViewAll })]
        public async Task<AppDomainResult> Get([FromQuery] SmallPackageSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                PagedList<SmallPackage> pagedData = await smallPackageService.GetPagedListData(baseSearch);
                PagedList<SmallPackageModel> pagedDataModel = mapper.Map<PagedList<SmallPackageModel>>(pagedData);
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
        /// Lấy thông tin quyền của chức năng
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-permission-detail")]
        [Authorize]
        public async Task<AppDomainResult> GetPermission()
        {
            List<int> permissionIds = new List<int>();
            bool isViewAll = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                    , new List<int>() { CoreContants.ViewAll });
            if (isViewAll) permissionIds.Add((int)CoreContants.PermissionContants.ViewAll);

            bool isView = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                    , new List<int>() { CoreContants.View });
            if (isView) permissionIds.Add((int)CoreContants.PermissionContants.View);

            bool isAddNew = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                    , new List<int>() { CoreContants.AddNew });
            if (isAddNew) permissionIds.Add((int)CoreContants.PermissionContants.AddNew);

            bool isUpdate = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                    , new List<int>() { CoreContants.Update });
            if (isUpdate) permissionIds.Add((int)CoreContants.PermissionContants.Update);

            bool isDelete = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                    , new List<int>() { CoreContants.Delete });
            if (isDelete) permissionIds.Add((int)CoreContants.PermissionContants.Delete);

            bool isDownload = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                    , new List<int>() { CoreContants.Download });
            if (isDownload) permissionIds.Add((int)CoreContants.PermissionContants.Download);

            bool isExport = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                    , new List<int>() { CoreContants.Export });
            if (isExport) permissionIds.Add((int)CoreContants.PermissionContants.Export);

            bool isImport = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                   , new List<int>() { CoreContants.Import });
            if (isImport) permissionIds.Add((int)CoreContants.PermissionContants.Import);

            bool isUpload = await this.userService.HasPermission(LoginContext.Instance.CurrentUser.UserId, ControllerContext.ActionDescriptor.ControllerName
                   , new List<int>() { CoreContants.Upload });
            if (isUpload) permissionIds.Add((int)CoreContants.PermissionContants.Upload);

            return new AppDomainResult()
            {
                Data = permissionIds,
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
        }
        #endregion
    }

    public class ImportSmallPackageRequest
    {
        public string FileURL { get; set; }
        public int? BigPackageId { get; set; }
    }
}
