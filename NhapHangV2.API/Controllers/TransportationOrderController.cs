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
using NhapHangV2.Interface.Services.Auth;
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
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.API.Controllers
{
    [Route("api/transportation-order")]
    [ApiController]
    [Description("Đơn hàng vận chuyển hộ")]
    [Authorize]
    public class TransportationOrderController : BaseController<TransportationOrder, TransportationOrderModel, TransportationOrderRequest, TransportationOrderSearch>
    {
        protected IConfiguration configuration;
        protected readonly ITransportationOrderService transportationOrderService;
        protected readonly ISmallPackageService smallPackageService;
        protected readonly IUserService userService;
        protected readonly IUserInGroupService userInGroupService;
        protected readonly IConfigurationsService configurationsService;

        public TransportationOrderController(IServiceProvider serviceProvider, ILogger<BaseController<TransportationOrder, TransportationOrderModel, TransportationOrderRequest, TransportationOrderSearch>> logger, IWebHostEnvironment env, IConfiguration configuration) : base(serviceProvider, logger, env)
        {
            this.configuration = configuration;
            this.domainService = this.serviceProvider.GetRequiredService<ITransportationOrderService>();
            transportationOrderService = serviceProvider.GetRequiredService<ITransportationOrderService>();
            smallPackageService = serviceProvider.GetRequiredService<ISmallPackageService>();
            userService = serviceProvider.GetRequiredService<IUserService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
        }

        ///<summary>
        ///Thông tin đơn hàng trang đơn hàng của user
        /// </summary>
        [HttpGet("get-transportations-infor")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public AppDomainResult GetTransportationsInfor([FromQuery] TransportationOrderSearch transportationOrderSearch)
        {
            var transportationsInfor = transportationOrderService.GetTransportationsInfor(transportationOrderSearch);
            return new AppDomainResult
            {
                Data = transportationsInfor,
                ResultCode = (int)HttpStatusCode.OK,
                Success = true
            };
        }

        ///<summary>
        ///Thông tin tiền hàng trang đơn hàng của user
        /// </summary>
        [HttpGet("get-transportations-amount")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetTransportationsAmount()
        {
            int UID = LoginContext.Instance.CurrentUser.UserId;
            var users = await userService.GetSingleAsync(x => x.Id == UID);
            if (users == null)
                throw new AppException("Người dùng không tồn tại");
            var transportationsAmount = transportationOrderService.GetTransportationsAmount(UID);
            return new AppDomainResult
            {
                ResultCode = (int)HttpStatusCode.OK,
                Success = true,
                Data = transportationsAmount
            };
        }

        /// <summary>
        /// Lấy danh sách item phân trang
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [AppAuthorize(new int[] { CoreContants.ViewAll })]
        public override async Task<AppDomainResult> Get([FromQuery] TransportationOrderSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                PagedList<TransportationOrder> pagedData = await this.domainService.GetPagedListData(baseSearch);
                PagedList<TransportationOrderModel> pagedDataModel = mapper.Map<PagedList<TransportationOrderModel>>(pagedData);

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
        /// Thêm mới item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public override async Task<AppDomainResult> AddItem([FromBody] TransportationOrderRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            var user = new Users();
            if (itemModel.UID != null)
            { //admin tạo dùm
                user = await userService.GetByIdAsync(itemModel.UID ?? 0);
                itemModel.SalerID = user.SaleId;
            }
            else
            { //user đang login tạo
                user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
                itemModel.SalerID = user.SaleId;
            }
            bool success = false;
            if (ModelState.IsValid)
            {
                List<TransportationOrder> transportationOrders = new List<TransportationOrder>();

                var configurations = await configurationsService.GetSingleAsync();
                decimal currency = Convert.ToDecimal(configurations.AgentCurrency);

                if (user.Currency > 0)
                    currency = user.Currency ?? 0;

                List<SmallPackageRequest> listSmallPackages = itemModel.SmallPackages.DistinctBy(x => x.OrderTransactionCode.Trim()).ToList();
                foreach (var list in listSmallPackages)
                {
                    TransportationOrder data = new TransportationOrder();
                    data.UID = user.Id;
                    data.Currency = currency;
                    data.WareHouseFromId = itemModel.WareHouseFromId;
                    data.WareHouseId = itemModel.WareHouseId;
                    data.ShippingTypeId = itemModel.ShippingTypeId;

                    data.Status = (int)StatusGeneralTransportationOrder.ChoDuyet;
                    data.Note = list.UserNote;
                    data.OrderTransactionCode = list.OrderTransactionCode.Trim().Replace(" ", "");
                    data.IsCheckProduct = list.IsCheckProduct;
                    data.IsInsurance = list.IsInsurance;
                    data.IsPacked = list.IsPacked;
                    data.Amount = list.Amount;
                    data.Category = list.Category;
                    data.CODFee = list.FeeShip * currency;
                    data.CODFeeTQ = list.FeeShip;
                    data.TotalPriceCNY = list.FeeShip;
                    data.TotalPriceVND = list.FeeShip * currency;
                    data.TotalPriceVND = Math.Round(data.TotalPriceVND ?? 0, 0);
                    data.SalerID = itemModel.SalerID;

                    transportationOrders.Add(data);
                }

                if (transportationOrders.Any())
                {
                    success = await this.domainService.CreateAsync(transportationOrders);
                    if (success)
                    {
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                        appDomainResult.Data = transportationOrders;
                    }

                    else
                        throw new Exception("Lỗi trong quá trình xử lý");
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
        public override async Task<AppDomainResult> UpdateItem([FromBody] TransportationOrderRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var item = await this.domainService.GetByIdAsync(itemModel.Id);
                if (item != null)
                {
                    item = await CalculatePrice(itemModel, item);
                    mapper.Map(item.SmallPackages, itemModel.SmallPackages);
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
        /// Hủy kiện yêu cầu ký gửi
        /// </summary>
        /// <param name="id">Id ký gửi</param>
        /// <param name="note">Ghi chú</param>
        /// <returns></returns>
        [HttpPut("cancel-{id}")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> CancelItem(int id, string note)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            var currentDate = DateTime.Now;
            bool success = false;
            if (string.IsNullOrEmpty(note))
                throw new AppException("Chưa nhập lý do hủy đơn hàng");
            var trans = await this.domainService.GetByIdAsync(id);
            if (trans.SmallPackageId != null && trans.SmallPackageId > 0)
            {
                if (!(await smallPackageService.DeleteAsync(trans.SmallPackageId ?? 0)))
                    throw new AppException("Xóa mã vận đơn thất bại");
            }
            // Kiểm tra item có tồn tại chưa?
            var messageUserCheck = await this.domainService.GetExistItemMessage(trans);
            if (!string.IsNullOrEmpty(messageUserCheck))
                throw new KeyNotFoundException(messageUserCheck);
            if (trans.Status != (int?)StatusGeneralTransportationOrder.ChoDuyet) //Phải là đơn hàng mới thì mới được hủy
                throw new AppException("Trạng thái đơn hàng sai. Vui lòng kiểm tra lại");

            trans.Status = (int?)StatusGeneralTransportationOrder.Huy;
            trans.CancelReason = note;
            if (trans.CancelDate == null)
                trans.CancelDate = currentDate;
            trans.Updated = currentDate;
            trans.UpdatedBy = LoginContext.Instance.CurrentUser.UserName;

            List<TransportationOrder> listTrans = new List<TransportationOrder>();
            listTrans.Add(trans);
            success = await transportationOrderService.UpdateAsync(listTrans, (int)StatusGeneralTransportationOrder.Huy, 0);
            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            }
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;

            return appDomainResult;
        }

        /// <summary>
        /// Lấy thông tin thanh toán xuất kho
        /// </summary>
        /// <param name="listID">Danh sách ID</param>
        /// <param name="isAll">TRUE: Chọn tất cả, FALSE: Chọn từng cái</param>
        /// <returns></returns>
        [HttpGet("get-billing-info")]
        [AppAuthorize(new int[] { CoreContants.ViewAll })]
        public async Task<AppDomainResult> GetBillingInfo([FromQuery] List<int> listID, bool isAll)
        {
            string userName = LoginContext.Instance.CurrentUser.UserName;
            var user = await userService.GetSingleAsync(x => x.UserName == userName);
            if (isAll) //Nếu lấy tất cả
            {
                var listTransOrder = await this.domainService.GetAsync(x => !x.Deleted
                            && x.UID == user.Id
                            && x.Status == (int?)StatusGeneralTransportationOrder.VeKhoVN);
                if (!listTransOrder.Any())
                    throw new KeyNotFoundException("Không có đơn hàng nào để thanh toán. Vui lòng kiểm tra lại");

                listID = new List<int>();
                listID.AddRange(listTransOrder.Select(x => x.Id));
            }

            var items = await transportationOrderService.GetBillingInfo(listID, false);

            return new AppDomainResult()
            {
                Success = true,
                Data = items,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Thanh toán tại kho, thanh toán bằng ví
        /// </summary>
        /// <param name="listID">Danh sách ID ký gửi</param>
        /// <param name="shippingType">Hình thức vận chuyển trong nước</param>
        /// <param name="note">Ghi chú</param>
        /// <param name="type">0: Thanh toán tại kho, 2: Thanh toán qua ví</param>
        /// <returns></returns>
        [HttpPut("payment")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> Payment([FromQuery] List<int> listID, int shippingType, string note, int type)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            var listTransOrder = await this.domainService.GetAsync(x => !x.Deleted && x.Active
                && (listID.Contains(x.Id))
            );
            if (!listTransOrder.Any())
                throw new KeyNotFoundException("Không có đơn hàng nào. Vui lòng kiểm tra lại");

            int checkStatus = 0;
            //Kiểm tra đơn hàng đủ điều kiện hay không
            checkStatus = listTransOrder.Where(x => x.Status == (int?)StatusGeneralTransportationOrder.VeKhoVN).Count();
            if (listTransOrder.Count() != checkStatus) //Nếu số lượng khác nhau
            {
                List<int> listError = listTransOrder.Where(x => x.Status != (int?)StatusGeneralTransportationOrder.VeKhoVN).Select(x => x.Id).ToList();
                throw new AppException(string.Format("Đơn này bị sai trạng thái Thanh toán, vui lòng kiểm tra lại"));
            }

            if (type != 2 && type != 0)
                throw new AppException(string.Format("Sai loại thanh toán. Vui lòng kiểm tra lại"));

            listTransOrder.ToList().ForEach(x =>
            {
                x.ExportRequestNote = note;
                x.DateExportRequest = DateTime.Now;
                x.ShippingTypeVN = shippingType;
                x.Status = (int?)StatusGeneralTransportationOrder.DaThanhToan;
                if (x.PaidDate == null)
                    x.PaidDate = DateTime.Now;
                //Còn gán thằng TotalPrice thì trong Service đó
            });

            success = await transportationOrderService.UpdateAsync(listTransOrder, (int)StatusGeneralTransportationOrder.DaThanhToan, type);
            if (success)
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;

            return appDomainResult;
        }

        /// <summary>
        /// Thanh toán đơn ký gửi cho user
        /// </summary>
        /// <param name="listId">Danh sách ID</param>
        /// <returns></returns>
        [HttpPut("payemnt-order")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> PaymentTransportationOrder([FromBody] List<int> listId)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            int userId = LoginContext.Instance.CurrentUser.UserId;
            success = await transportationOrderService.UpdateTransportationOrder(listId, userId);
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
        public async Task<AppDomainResult> UpdateStaff([FromBody] TransportationOrderRequest itemModel)
        {
            var result = await transportationOrderService.UpdateStaffAsync(itemModel.Id, itemModel.SalerID.Value);
            return new AppDomainResult
            {
                ResultCode = (int)HttpStatusCode.OK,
                Success = result
            };
        }
        #region Excel

        /// <summary>
        /// Xuất Excel
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpPost("export-excel")]
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] TransportationOrderSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<TransportationOrderModel> pagedListModel = new PagedList<TransportationOrderModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<TransportationOrder> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<TransportationOrderModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("TransportationOrderTemplate.xlsx");
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new TransportationOrderModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);
            //// Xuất biểu đồ nếu có
            //fileByteReport = await this.ExportChart(fileByteReport, pagedListModel.Items);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "TransportationOrder");
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

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<TransportationOrderModel> listData)
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
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<TransportationOrderModel> pagedList, TransportationOrderSearch baseSearch)
        {
            return await Task.Run(() =>
            {
                IDictionary<string, object> dictionaries = new Dictionary<string, object>();
                return dictionaries;
            });
        }
        #endregion


        private async Task<TransportationOrder> CalculatePrice(TransportationOrderRequest itemModel, TransportationOrder transportationOrder)
        {
            decimal? totalVND = 0;
            decimal? totalCNY = 0;

            totalVND += (itemModel.AdditionFeeVND ?? 0) + (itemModel.SensorFeeVND ?? 0);

            var smallPackage = transportationOrder.SmallPackages.FirstOrDefault();
            transportationOrder.SmallPackages = new List<SmallPackage>();
            if (smallPackage != null)
            {
                decimal? priceWeight = 0;
                decimal? priceVolume = 0;
                if (itemModel.FeeWeightPerKg != null && itemModel.FeeWeightPerKg > 0)
                {
                    priceWeight = itemModel.FeeWeightPerKg * smallPackage.PayableWeight;
                    smallPackage.PriceWeight = itemModel.FeeWeightPerKg;
                }
                if (itemModel.FeePerVolume != null && itemModel.FeePerVolume > 0)
                {
                    priceVolume = itemModel.FeePerVolume * smallPackage.VolumePayment;
                    smallPackage.PriceVolume = itemModel.FeePerVolume;
                }
                //Tiền vận chuyển của mã vận đơn
                decimal? deliveryFeeSM = priceVolume > priceWeight ? priceVolume : priceWeight;
                totalVND += deliveryFeeSM ?? 0;
                if (smallPackage.PayableWeight != null)
                    transportationOrder.PayableWeight = smallPackage.PayableWeight;
                smallPackage.AdditionFeeCNY = itemModel.AdditionFeeCNY;
                smallPackage.AdditionFeeVND = itemModel.AdditionFeeVND;
                smallPackage.SensorFeeCNY = itemModel.SensorFeeCNY;
                smallPackage.SensorFeeVND = itemModel.SensorFeeVND;
                smallPackage.TotalPrice = (deliveryFeeSM ?? 0) + (smallPackage.AdditionFeeCNY ?? 0)
                    + (smallPackage.AdditionFeeVND ?? 0) + (smallPackage.SensorFeeCNY ?? 0) + (smallPackage.SensorFeeVND ?? 0);
                transportationOrder.SmallPackages.Add(smallPackage);
            }
            //Cộng phí kiểm đếm, phí đóng gỗ, phí bảo hiểm, phí giao hàng
            if (itemModel.IsCheckProduct == null || itemModel.IsCheckProduct.Value == false)
                itemModel.IsCheckProductPrice = 0;
            else
            {
                transportationOrder.IsCheckProductPrice = itemModel.IsCheckProductPrice;
                transportationOrder.IsCheckProduct = itemModel.IsCheckProduct;
                totalVND += itemModel.IsCheckProductPrice ?? 0;
            }

            if (itemModel.IsPacked == null || itemModel.IsPacked.Value == false)
                itemModel.IsPackedPrice = 0;
            else
            {
                transportationOrder.IsPackedPrice = itemModel.IsPackedPrice;
                transportationOrder.IsPacked = itemModel.IsPacked;
                totalVND += itemModel.IsPackedPrice ?? 0;
            }

            if (itemModel.IsInsurance == null || itemModel.IsInsurance.Value == false)
                itemModel.InsuranceMoney = 0;
            else
            {
                transportationOrder.InsuranceMoney = itemModel.InsuranceMoney;
                transportationOrder.IsInsurance = itemModel.IsInsurance;
                totalVND += itemModel.InsuranceMoney ?? 0;
            }
            if (itemModel.DeliveryPrice != null)
            {
                transportationOrder.DeliveryPrice = itemModel.DeliveryPrice;
            }
            if (itemModel.CODFee != null)
            {
                transportationOrder.CODFee = itemModel.CODFee;
                totalVND += itemModel.CODFee ?? 0;
            }
            var config = await configurationsService.GetSingleAsync();
            totalCNY = Math.Round((totalVND / transportationOrder.Currency) ?? 0, 1);
            transportationOrder.TotalPriceVND = Math.Round(totalVND ?? 0, 0);
            transportationOrder.TotalPriceCNY = totalCNY ?? 0;
            return transportationOrder;
        }
    }


}
