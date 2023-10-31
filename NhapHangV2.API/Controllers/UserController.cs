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
using System.Net;
using System.Threading.Tasks;
using NhapHangV2.Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using NhapHangV2.Request;
using NhapHangV2.Entities.Search;
using NhapHangV2.BaseAPI.Controllers;
using static NhapHangV2.Utilities.CoreContants;
using Microsoft.AspNetCore.SignalR;

namespace NhapHangV2.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Description("Quản lý thông tin người dùng")]
    [Authorize]
    public class UserController : BaseController<Users, UserModel, UserRequest, UserSearch>
    {
        protected IUserService userService;
        protected IUserInGroupService userInGroupService;
        protected IUserGroupService userGroupService;
        protected IMainOrderService mainOrderService;
        protected IPayHelpService payHelpService;
        protected ITransportationOrderService transportationOrderService;
        protected IUserLevelService userLevelService;
        protected IHubContext<DomainHub, IDomainHub> hubContext;
        private IConfiguration configuration;
        public UserController(IServiceProvider serviceProvider, ILogger<BaseController<Users, UserModel, UserRequest, UserSearch>> logger
            , IConfiguration configuration
            , IWebHostEnvironment env
            , IHubContext<DomainHub, IDomainHub> hubContext) : base(serviceProvider, logger, env)
        {
            this.domainService = serviceProvider.GetRequiredService<IUserService>();
            this.userService = serviceProvider.GetRequiredService<IUserService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            userGroupService = serviceProvider.GetRequiredService<IUserGroupService>();
            userLevelService = serviceProvider.GetRequiredService<IUserLevelService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            payHelpService = serviceProvider.GetRequiredService<IPayHelpService>();
            transportationOrderService = serviceProvider.GetRequiredService<ITransportationOrderService>();
            this.configuration = configuration;
            this.hubContext = hubContext;
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
            if (id == 0)
            {
                throw new KeyNotFoundException("id không tồn tại");
            }
            var item = await this.domainService.GetByIdAsync(id);
            //decimal? totalOrderPrice = 0;
            //decimal? totalPaidPrice = 0;
            //decimal? totalUnPaidPrice = 0;

            //var mainOrders = await mainOrderService.GetTotalOrderPriceByUID(id);
            //totalOrderPrice += mainOrders.TotalOrderPrice ?? 0;
            //totalPaidPrice += mainOrders.TotalPaidPrice ?? 0;
            //var payHelps = await payHelpService.GetTotalOrderPriceByUID(id);
            //totalOrderPrice += payHelps.TotalOrderPrice ?? 0;
            //totalPaidPrice += payHelps.TotalPaidPrice ?? 0;
            //var transportationOrders = await transportationOrderService.GetTotalOrderPriceByUID(id);
            //totalOrderPrice += transportationOrders.TotalOrderPrice ?? 0;
            //totalUnPaidPrice = (totalOrderPrice - totalPaidPrice) ?? 0;

            if (item != null)
            {
                if (LoginContext.Instance.CurrentUser != null)
                {
                    var itemModel = mapper.Map<UserModel>(item);

                    itemModel.ConfirmPassWord = item.Password;
                    //itemModel.TotalOrderPrice = totalOrderPrice;
                    //itemModel.TotalPaidPrice = totalPaidPrice;
                    //itemModel.TotalUnPaidPrice = totalUnPaidPrice;

                    appDomainResult = new AppDomainResult()
                    {
                        Success = true,
                        Data = itemModel,
                        ResultCode = (int)HttpStatusCode.OK
                    };
                }
                else throw new InvalidCastException("Không có quyền truy cập");
            }
            else
            {
                throw new KeyNotFoundException("Item không tồn tại");
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
        public override async Task<AppDomainResult> UpdateItem([FromBody] UserRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            if (itemModel.IsResetPassword) //Có reset mật khẩu không
            {
                if (string.IsNullOrEmpty(itemModel.ConfirmPassWord)) throw new AppException("Vui lòng nhập mật khẩu cũ");
                if (itemModel.ConfirmPassWord.Length < 8) throw new AppException("Mật khẩu phải lớn hơn 8 kí tự");
                if (!itemModel.ConfirmPassWord.Equals(itemModel.Password)) throw new AppException("Mật khẩu xác nhận không giống mật khẩu cũ");

                if (string.IsNullOrEmpty(itemModel.PasswordNew)) throw new AppException("Vui lòng nhập mật khẩu mới");
                if (itemModel.PasswordNew.Length < 8) throw new AppException("Mật khẩu phải lớn hơn 8 kí tự");

                if (string.IsNullOrEmpty(itemModel.PasswordAgain)) throw new AppException("Vui lòng nhập mật khẩu cũ");
                if (itemModel.PasswordAgain.Length < 8) throw new AppException("Mật khẩu phải lớn hơn 8 kí tự");
                if (!itemModel.PasswordAgain.Equals(itemModel.PasswordNew)) throw new AppException("Mật khẩu xác nhận không giống mật khẩu mới");
            }

            var user = userService.GetById(itemModel.Id);
            string oldAvatarURL = user.AvatarIMG;
            var item = mapper.Map(itemModel, user);

            item.Updated = DateTime.UtcNow.AddHours(7);
            item.UpdatedBy = LoginContext.Instance.CurrentUser.UserName;
            if (itemModel.IsResetPassword)
                item.Password = SecurityUtilities.HashSHA1(itemModel.PasswordNew);

            if (item != null)
            {
                // Kiểm tra item có tồn tại chưa?
                var messageUserCheck = await this.domainService.GetExistItemMessage(item);
                if (!string.IsNullOrEmpty(messageUserCheck))
                    throw new KeyNotFoundException(messageUserCheck);
                success = await userService.UpdateAsync(item);
                if (success)
                {
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    await hubContext.Clients.All.ChangeTemp(true);
                }
                else
                    throw new Exception("Lỗi trong quá trình xử lý");
                appDomainResult.Success = success;
            }
            else
                throw new KeyNotFoundException("Item không tồn tại");


            return appDomainResult;
        }

        /// <summary>
        /// Thêm mới user
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public override async Task<AppDomainResult> AddItem([FromBody] UserRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                if (itemModel.IsResetPassword) //Có reset mật khẩu không
                {
                    if (string.IsNullOrEmpty(itemModel.ConfirmPassWord)) throw new AppException("Vui lòng nhập mật khẩu cũ");
                    if (itemModel.ConfirmPassWord.Length < 8) throw new AppException("Mật khẩu phải lớn hơn 8 kí tự");
                    if (!itemModel.ConfirmPassWord.Equals(itemModel.Password)) throw new AppException("Mật khẩu xác nhận không giống mật khẩu cũ");

                    if (string.IsNullOrEmpty(itemModel.NewPassWord)) throw new AppException("Vui lòng nhập mật khẩu mới");
                    if (itemModel.NewPassWord.Length < 8) throw new AppException("Mật khẩu phải lớn hơn 8 kí tự");

                    if (string.IsNullOrEmpty(itemModel.ConfirmNewPassWord)) throw new AppException("Vui lòng nhập mật khẩu cũ");
                    if (itemModel.ConfirmNewPassWord.Length < 8) throw new AppException("Mật khẩu phải lớn hơn 8 kí tự");
                    if (!itemModel.ConfirmNewPassWord.Equals(itemModel.NewPassWord)) throw new AppException("Mật khẩu xác nhận không giống mật khẩu mới");
                }
                var item = mapper.Map<Users>(itemModel);

                item.IsCheckOTP = true; //Tạo tài khoản trong trang Quản lý thì không cần check OTP
                item.Password = SecurityUtilities.HashSHA1(itemModel.Password);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await this.domainService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new KeyNotFoundException(messageUserCheck);
                    success = await userService.CreateAsync(item);
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
            {
                throw new AppException(ModelState.GetErrorMessage());
            }
            return appDomainResult;
        }

        /// <summary>
        /// Lấy danh sách item phân trang
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [AppAuthorize(new int[] { CoreContants.ViewAll })]
        public override async Task<AppDomainResult> Get([FromQuery] UserSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                PagedList<Users> pagedData = await this.domainService.GetPagedListData(baseSearch);
                PagedList<UserModel> pagedDataModel = mapper.Map<PagedList<UserModel>>(pagedData);
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
        /// Cập nhật ảnh đại diện của tài khoản
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("update-avatar")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateAvatarIMG([FromBody] UpdateAvatarRequest request)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            if (!ModelState.IsValid && request.AvatarIMG == null)
                throw new AppException(ModelState.GetErrorMessage());
            var item = await userService.GetByIdAsync(request.UserId);
            if (item == null)
                throw new KeyNotFoundException("Item không tồn tại");
            item.AvatarIMG = request.AvatarIMG;
            success = await this.domainService.UpdateAsync(item);
            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            }

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
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] UserSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<UserModel> pagedListModel = new PagedList<UserModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<Users> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<UserModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("UserTemplate.xlsx"); //Danh sách người dùng 
            if (baseSearch.UserGroupId == (int)PermissionTypes.User) //Danh sách khách hàng
                getTemplateFilePath = GetTemplateFilePath("CustomerTemplate.xlsx");
            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new UserModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);
            // Xuất biểu đồ nếu có
            //fileByteReport = await this.ExportChart(fileByteReport, pagedListModel.Items);

            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string getName = "Users";
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), getName);
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

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<UserModel> listData)
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
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<UserModel> pagedList, UserSearch baseSearch)
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
