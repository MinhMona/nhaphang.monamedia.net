using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NhapHangV2.Utilities;
using NhapHangV2.Entities.Configuration;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Auth;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Models;
using NhapHangV2.Models.DomainModels;
using NhapHangV2.Request.Auth;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Users = NhapHangV2.Entities.Users;
using Microsoft.Win32;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.BaseAPI.Controllers.Auth
{
    [ApiController]
    public abstract class AuthController : ControllerBase
    {
        protected readonly ILogger<AuthController> logger;
        protected IUserService userService;
        protected IUserGroupService userGroupService;
        protected IUserInGroupService userInGroupService;
        protected IConfiguration configuration;
        protected IMapper mapper;
        private IEmailConfigurationService emailConfigurationService;
        private readonly ISMSConfigurationService sMSConfigurationService;
        private readonly IOTPHistoryService oTPHistoryService;
        private readonly ISMSEmailTemplateService sMSEmailTemplateService;
        private readonly ITokenManagerService tokenManagerService;
        private readonly INotificationSettingService notificationSettingService;
        private readonly INotificationTemplateService notificationTemplateService;
        private readonly ISendNotificationService sendNotificationService;
        protected readonly IHubContext<DomainHub, IDomainHub> hubContext;
        //private readonly FirebaseApp _firebaseApp;
        //private readonly FirebaseAuth _firebaseAuth;
        public AuthController(IServiceProvider serviceProvider
            , IConfiguration configuration
            , IMapper mapper, ILogger<AuthController> logger
            )
        {
            this.logger = logger;
            this.configuration = configuration;
            this.mapper = mapper;

            userService = serviceProvider.GetRequiredService<IUserService>();
            userInGroupService = serviceProvider.GetRequiredService<IUserInGroupService>();
            userGroupService = serviceProvider.GetRequiredService<IUserGroupService>();
            tokenManagerService = serviceProvider.GetRequiredService<ITokenManagerService>();
            emailConfigurationService = serviceProvider.GetRequiredService<IEmailConfigurationService>();
            sMSConfigurationService = serviceProvider.GetRequiredService<ISMSConfigurationService>();
            oTPHistoryService = serviceProvider.GetRequiredService<IOTPHistoryService>();
            sMSEmailTemplateService = serviceProvider.GetRequiredService<ISMSEmailTemplateService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            notificationTemplateService = serviceProvider.GetRequiredService<INotificationTemplateService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
            hubContext = serviceProvider.GetRequiredService<IHubContext<DomainHub, IDomainHub>>();
            //_firebaseApp = firebaseApp;
            //_firebaseAuth = FirebaseAuth.GetAuth(_firebaseApp);
        }

        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public virtual async Task<AppDomainResult> LoginAsync([FromForm] Login loginModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (ModelState.IsValid)
            {
                var userInfos = await this.userService.Verify(loginModel.UserName, loginModel.Password);
                if (userInfos != null)
                {
                    var userModel = mapper.Map<UserModel>(userInfos);
                    var token = await GenerateJwtTokenForLogin(userModel);

                    // Lưu giá trị token
                    await this.userService.UpdateUserToken(userModel.Id, token, true);

                    appDomainResult = new AppDomainResult()
                    {
                        Success = true,
                        Data = new
                        {
                            token = token
                        },
                        ResultCode = (int)HttpStatusCode.OK
                    };

                }

            }
            else
                throw new AppException(ModelState.GetErrorMessage());
            return appDomainResult;
        }

        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login-app")]
        public virtual async Task<AppDomainResult> LoginForAppAsync([FromForm] Login loginModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (ModelState.IsValid)
            {
                var userInfos = await this.userService.VerifyForApp(loginModel.UserName, loginModel.Password);
                if (userInfos != null)
                {
                    var userModel = mapper.Map<UserModel>(userInfos);
                    var token = await GenerateJwtTokenForLogin(userModel);

                    // Lưu giá trị token
                    await this.userService.UpdateUserToken(userModel.Id, token, true);

                    appDomainResult = new AppDomainResult()
                    {
                        Success = true,
                        Data = new
                        {
                            token = token
                        },
                        ResultCode = (int)HttpStatusCode.OK
                    };

                }

            }
            else
                throw new AppException(ModelState.GetErrorMessage());
            return appDomainResult;
        }

        /// <summary>
        /// Kiểm tra mã OTP đăng nhập hệ thống
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="otpValue"></param>
        /// <returns></returns>
        [HttpPost("send-otp/{phoneNumber}/{otpValue}")]
        [AllowAnonymous]
        public virtual async Task<AppDomainResult> SendOTP(string phoneNumber, string otpValue)
        {
            bool isValidPhoneNumber = ValidateUserName.IsPhoneNumber(phoneNumber);
            if (!isValidPhoneNumber) throw new AppException("Số điện thoại không hợp lệ!");
            var userInfos = await this.userService.GetAsync(e => !e.Deleted && e.Phone == phoneNumber);
            if (userInfos != null && userInfos.Any() && userInfos.Count == 1)
            {
                var userInfo = userInfos.FirstOrDefault();
                var otpHistoriesChecks = await this.oTPHistoryService.GetAsync(e => !e.Deleted && e.UserId == userInfo.Id && e.Phone == userInfo.Phone && e.OTPValue == otpValue);
                if (otpHistoriesChecks != null && otpHistoriesChecks.Any())
                {
                    var latestOTPCheck = otpHistoriesChecks.OrderByDescending(e => e.Created).FirstOrDefault();
                    if (DateTime.UtcNow.AddHours(7) > latestOTPCheck.ExpiredDate)
                        throw new AppException("OTP đã hết hạn, vui lòng lấy lại mã OTP khác!");
                    userInfo.IsCheckOTP = true;
                    userInfo.Updated = DateTime.UtcNow.AddHours(7);
                    userInfo.UpdatedBy = userInfo.UserName;
                    Expression<Func<Users, object>>[] inCludeProperties = new Expression<Func<Users, object>>[]
                    {
                        e => e.IsCheckOTP,
                        e => e.Updated,
                        e => e.UpdatedBy
                    };
                    await this.userService.UpdateFieldAsync(userInfo, inCludeProperties);
                }
                else throw new AppException("Mã OTP không chính xác!");
            }
            else throw new AppException("Số điện thoại chưa được đăng ký!");

            return new AppDomainResult()
            {
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Kiểm tra OTP để đổi mật khẩu cho điện thoại
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="otpValue"></param>
        /// <returns></returns>
        [HttpPost("send-otp-forget-password/{phoneNumber}/{otpValue}")]
        [AllowAnonymous]
        public virtual async Task<AppDomainResult> SendOTPGetPassword(string phoneNumber, string otpValue)
        {
            bool isValidPhoneNumber = ValidateUserName.IsPhoneNumber(phoneNumber);
            if (!isValidPhoneNumber) throw new AppException("Số điện thoại không hợp lệ!");
            var userInfos = await this.userService.GetAsync(e => !e.Deleted && e.Phone == phoneNumber);
            if (userInfos != null && userInfos.Any() && userInfos.Count == 1)
            {
                var userInfo = userInfos.FirstOrDefault();
                var otpHistoriesChecks = await this.oTPHistoryService.GetAsync(e => !e.Deleted && !e.IsEmail && e.UserId == userInfo.Id && e.Phone == userInfo.Phone && e.OTPValue == otpValue);
                if (otpHistoriesChecks != null && otpHistoriesChecks.Any())
                {
                    var latestOTPCheck = otpHistoriesChecks.OrderByDescending(e => e.Created).FirstOrDefault();
                    if (DateTime.UtcNow.AddHours(7) > latestOTPCheck.ExpiredDate)
                        throw new AppException("OTP đã hết hạn, vui lòng lấy lại mã OTP khác!");
                    var userModel = mapper.Map<UserModel>(userInfo);
                    var token = await GenerateJwtTokenForLogin(userModel, true);
                    // Lưu giá trị token
                    await this.userService.UpdateUserToken(userModel.Id, token, true);
                    return new AppDomainResult()
                    {
                        Success = true,
                        Data = new
                        {
                            token = token
                        },
                        ResultCode = (int)HttpStatusCode.OK
                    };
                }
                else throw new AppException("Mã OTP không chính xác!");
            }
            else throw new AppException("Số điện thoại chưa được đăng ký!");
        }

        /// <summary>
        /// Kiểm tra OTP để đổi mật khẩu cho Email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="otpValue"></param>
        /// <returns></returns>
        [HttpPost("send-otp-email-forget-password/{email}/{otpValue}")]
        [AllowAnonymous]
        public virtual async Task<AppDomainResult> SendOTPGetPasswordEmail(string email, string otpValue)
        {
            bool isValidEmail = ValidateUserName.IsEmail(email);
            if (!isValidEmail) throw new AppException("Email không hợp lệ!");
            var userInfos = await this.userService.GetAsync(e => !e.Deleted && e.Email == email);
            if (userInfos != null && userInfos.Any() && userInfos.Count == 1)
            {
                var userInfo = userInfos.FirstOrDefault();
                var otpHistoriesChecks = await this.oTPHistoryService.GetAsync(e => !e.Deleted && e.IsEmail && e.UserId == userInfo.Id && e.Email == userInfo.Email && e.OTPValue == otpValue);
                if (otpHistoriesChecks != null && otpHistoriesChecks.Any())
                {
                    var latestOTPCheck = otpHistoriesChecks.OrderByDescending(e => e.Created).FirstOrDefault();
                    if (DateTime.UtcNow.AddHours(7) > latestOTPCheck.ExpiredDate)
                        throw new AppException("OTP đã hết hạn, vui lòng lấy lại mã OTP khác!");
                    var userModel = mapper.Map<UserModel>(userInfo);
                    var token = await GenerateJwtTokenForLogin(userModel, true);
                    // Lưu giá trị token
                    await this.userService.UpdateUserToken(userModel.Id, token, true);
                    return new AppDomainResult()
                    {
                        Success = true,
                        Data = new
                        {
                            token = token
                        },
                        ResultCode = (int)HttpStatusCode.OK
                    };
                }
                else throw new AppException("Mã OTP không chính xác!");
            }
            else throw new AppException("Email chưa được đăng ký!");
        }

        /// <summary>
        /// Gửi mã OTP theo sdt có trong hệ thống
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-otp-code/{phoneNumber}")]
        public virtual async Task<AppDomainResult> GenerateOTPCode(string phoneNumber)
        {
            bool isValidPhoneNumber = ValidateUserName.IsPhoneNumber(phoneNumber);
            if (!isValidPhoneNumber) throw new AppException("Số điện thoại không hợp lệ!");
            var smsTemplateInfos = await this.sMSEmailTemplateService.GetAsync(e => !e.Deleted && e.Active && e.Code == CoreContants.SMS_XNOTP);
            var userInfos = await this.userService.GetAsync(e => !e.Deleted && e.Phone == phoneNumber);
            if (userInfos != null && userInfos.Any() && userInfos.Count == 1)
            {
                var userInfo = userInfos.FirstOrDefault();
                if (smsTemplateInfos != null && smsTemplateInfos.Any())
                {
                    var smsTemplateInfo = smsTemplateInfos.FirstOrDefault();
                    var otpValue = RandomUtilities.RandomOTPString(6);
                    bool isSendSMS = await sMSConfigurationService.SendSMS(userInfo.Phone, string.Format(smsTemplateInfo.Body, otpValue));
                    if (isSendSMS)
                    {
                        // Lưu lịch sử OTP ứng với thông tin user
                        OTPHistories oTPHistories = new OTPHistories()
                        {
                            Created = DateTime.Now,
                            CreatedBy = "system",
                            UserId = userInfo.Id,
                            Active = true,
                            Deleted = false,
                            OTPValue = otpValue,
                            Phone = userInfo.Phone,
                            ExpiredDate = DateTime.UtcNow.AddHours(7).AddMinutes(1),
                            Status = 0
                        };
                        await this.oTPHistoryService.CreateAsync(oTPHistories);
                    }
                    else throw new AppException("Gửi tin nhắn thất bại!");
                }
                else throw new AppException("Không tìm thấy nội dung tin nhắn!");
            }
            else throw new AppException("Số điện thoại chưa được đăng ký!");

            return new AppDomainResult()
            {
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Tạo OTP gửi qua email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-otp-code-email/{email}")]
        public virtual async Task<AppDomainResult> GenerateOTPCodeEmail(string email)
        {
            bool success = false;

            bool isValieEmail = ValidateUserName.IsEmail(email);
            if (!isValieEmail) throw new AppException("Email không hợp lệ!");
            var userInfos = await this.userService.GetAsync(e => !e.Deleted && e.Email == email);
            if (userInfos != null && userInfos.Any() && userInfos.Count == 1)
            {
                var userInfo = userInfos.FirstOrDefault();
                var otpValue = RandomUtilities.RandomOTPString(6);
                string emailBody = string.Format("<p>Mã OTP của bạn là: {0}. Thời hạn OTP hiệu lực trong 1 phút.</p>", otpValue);
                await emailConfigurationService.Send("OTP Xác thực", new string[] { userInfo.Email }, null, null, new EmailContent()
                {
                    Content = emailBody,
                    IsHtml = true,
                });
                // Lưu lịch sử OTP ứng với thông tin user
                OTPHistories oTPHistories = new OTPHistories()
                {
                    Created = DateTime.UtcNow.AddHours(7),
                    CreatedBy = "system",
                    UserId = userInfo.Id,
                    Active = true,
                    Deleted = false,
                    OTPValue = otpValue,
                    Phone = userInfo.Phone,
                    Email = userInfo.Email,
                    IsEmail = true,
                    ExpiredDate = DateTime.UtcNow.AddHours(7).AddMinutes(1),
                    Status = 0
                };
                await this.oTPHistoryService.CreateAsync(oTPHistories);
            }
            else throw new AppException("Email chưa được đăng ký!");

            return new AppDomainResult
            {
                Success = success,
                ResultCode = (int)HttpStatusCode.OK,
            };
        }

        /// <summary>
        /// Đăng ký
        /// </summary>
        /// <param name="register"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public virtual async Task<AppDomainResult> Register([FromBody] Register register)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                var user = new Users()
                {
                    UserName = register.UserName,
                    FullName = register.FullName,
                    Password = register.Password,
                    FirstName = register.FirstName,
                    LastName = register.LastName,
                    Created = DateTime.UtcNow.AddHours(7),
                    CreatedBy = register.UserName,
                    Active = true,
                    Phone = register.Phone,
                    Email = register.Email,
                    IsCheckOTP = true,
                    UserGroupId = 2,
                    Wallet = 0,
                    WalletCNY = 0,
                    LevelId = 1
                };
                // Kiểm tra item có tồn tại chưa?
                var messageUserCheck = await this.userService.GetExistItemMessage(user);
                if (!string.IsNullOrEmpty(messageUserCheck))
                    throw new AppException(messageUserCheck);
                user.Password = SecurityUtilities.HashSHA1(register.Password);

                //Thêm token
                var userModel = mapper.Map<UserModel>(user);

                userModel.Id = await userService.CreateWithTokenAsync(user);
                if (userModel.Id > 0)
                {
                    var token = await GenerateJwtTokenForLogin(userModel);
                    // Lưu giá trị token
                    await this.userService.UpdateUserToken(userModel.Id, token, true);

                    //Thông báo cho admin có người dùng mới
                    var notificationSetting = await notificationSettingService.GetByIdAsync((int)NotificationSettingId.DangKy);
                    sendNotificationService.SendNotification(notificationSetting,
                            new List<string>() { userModel.Id.ToString() },
                            new UserNotification());
                    appDomainResult = new AppDomainResult()
                    {
                        Success = true,
                        Data = new
                        {
                            token = token
                        },
                        ResultCode = (int)HttpStatusCode.OK
                    };
                }
                else
                {
                    var resultMessage = "Đăng ký không thành công";
                    throw new AppException(resultMessage);
                }
            }
            else
            {
                var resultMessage = ModelState.GetErrorMessage();
                throw new AppException(resultMessage);
            }
            return appDomainResult;
        }

        /// <summary>
        /// Hàm check kiểm tra tồn tại
        /// </summary>
        /// <param name="name">UserName || Email || Số điện thoại</param>
        /// <param name="type">
        /// 1. UserName
        /// 2. Email
        /// 3. Số điện thoại
        /// </param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("check-validate")]
        public virtual async Task<AppDomainResult> CheckValidate(string name, int type)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool isSuccess = true;
            var user = new Users();
            switch (type)
            {
                case 1: //UserName
                    user = await userService.GetSingleAsync(e => e.UserName.Equals(name.Trim()));
                    break;
                case 2: //Email
                    user = await userService.GetSingleAsync(e => e.Email.Equals(name.Trim()));
                    break;
                case 3: //Số điện thoại
                    user = await userService.GetSingleAsync(e => e.Phone.Equals(name.Trim()));
                    break;
                default:
                    throw new AppException("Đã có lỗi xảy ra");
            }
            if (user != null) isSuccess = false;
            return new AppDomainResult
            {
                Data = isSuccess,
                ResultCode = (int)HttpStatusCode.OK,
            };
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="changePasswordModel"></param>
        /// <returns></returns>
        [HttpPut("changePassword")]
        [Authorize]
        public virtual async Task<AppDomainResult> ChangePassword([FromBody] ChangePassword changePasswordModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (ModelState.IsValid)
            {
                // Check current user
                if (LoginContext.Instance.CurrentUser != null && LoginContext.Instance.CurrentUser.UserId != changePasswordModel.userId)
                    throw new AppException("Không phải người dùng hiện tại");
                // Check old Password + new Password
                string messageCheckPassword = await this.userService.CheckCurrentUserPassword(changePasswordModel.userId, changePasswordModel.OldPassword, changePasswordModel.NewPassword);
                if (!string.IsNullOrEmpty(messageCheckPassword))
                    throw new AppException(messageCheckPassword);

                var userInfo = await this.userService.GetByIdAsync(changePasswordModel.userId);
                string newPassword = SecurityUtilities.HashSHA1(changePasswordModel.NewPassword);
                userInfo.IsResetPassword = true;
                appDomainResult.Success = await userService.UpdateUserPassword(changePasswordModel.userId, newPassword);
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            }
            else
                throw new AppException(ModelState.GetErrorMessage());
            return appDomainResult;
        }

        /// <summary>
        /// Quên mật khẩu
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut("forgot-password")]
        public virtual async Task<AppDomainResult> ForgotPassword(Register itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool isValidEmail = ValidateUserName.IsEmail(itemModel.Email);
            // Kiểm tra đúng định dạng email và số điện thoại chưa
            //if (!isValidEmail && !isValidPhone)
            //    throw new AppException("Vui lòng nhập email hoặc số điện thoại!");
            // Tạo mật khẩu mới
            // Kiểm tra email/phone đã tồn tại chưa?
            var userInfos = await this.userService.GetAsync(e => !e.Deleted && isValidEmail == true && e.Email == itemModel.Email);
            Users userInfo = null;
            if (userInfos != null && userInfos.Any())
                userInfo = userInfos.FirstOrDefault();
            if (userInfo == null)
                throw new AppException("Email không tồn tại");
            // Cấp mật khẩu mới
            bool success = false;
            var newPasswordRandom = RandomUtilities.RandomString(8);
            var template = await sMSEmailTemplateService.GetByIdAsync(32);
            string body = string.Format(template.Body, userInfo.UserName, userInfo.Email, newPasswordRandom);
            if (isValidEmail)
            {
                userInfo.Password = SecurityUtilities.HashSHA1(newPasswordRandom);
                userInfo.Updated = DateTime.UtcNow.AddHours(7);
                Expression<Func<Users, object>>[] includeProperties = new Expression<Func<Users, object>>[]
                {
                e => e.Password,
                e => e.Updated
                };
                success = await this.userService.UpdateFieldAsync(userInfo, includeProperties);
                var config = await emailConfigurationService.GetEmailConfig();
                string[] email = { itemModel.Email };
                await emailConfigurationService.Send($"[{config.FromDisplayName}] Thay đổi mật khẩu", body, email);
            }
            else success = true;
            return new AppDomainResult()
            {
                Success = success,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("logout")]
        public virtual async Task<AppDomainResult> Logout()
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (LoginContext.Instance.CurrentUser != null)
                await this.userService.UpdateUserToken(LoginContext.Instance.CurrentUser.UserId, string.Empty, false);
            await this.tokenManagerService.DeactivateCurrentAsync();
            appDomainResult = new AppDomainResult()
            {
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
            return appDomainResult;
        }

        #region Private methods

        /// <summary>
        /// Tạo token từ thông tin user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isConfirmOTP"></param>
        /// <returns></returns>
        protected async Task<List<string>> GenerateJwtToken(UserModel user, bool isConfirmOTP = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var appSettingsSection = configuration.GetSection("AppSettings");
            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            var userInGroups = await userInGroupService.GetAsync(e => !e.Deleted && e.UserId == user.Id);
            if (userInGroups != null)
            {
                foreach (var userInGroup in userInGroups)
                {
                    user.UserGroupId = userInGroup.UserGroupId;
                    break;
                }
            }
            var userLoginModel = new UserLoginModel()
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserGroupId = user.UserGroupId,
                IsCheckOTP = user.IsCheckOTP,
                IsConfirmOTP = isConfirmOTP,
            };
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();
            var controllers = new List<ControllerModel>();
            var roles = new List<Role>();
            var roleAddToCart = new List<Role>();
            List<string> listToken = new List<string>();
            foreach (Assembly assem in assems)
            {
                var controller = assem.GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
              .Select(e => new ControllerModel()
              {
                  Id = e.Name.Replace("Controller", string.Empty),
                  Name = string.Format("{0}", ReflectionUtilities.GetClassDescription(e)).Replace("Controller", string.Empty)
              }).OrderBy(e => e.Name)
                  .Distinct();
                controllers.AddRange(controller);
            }
            if (controllers.Any())
            {
                foreach (var controller in controllers)
                {
                    var Permissions = await this.userService.GetPermission(userLoginModel.UserId, controller.Id);
                    if (Permissions.Length == 0) continue;
                    roles.Add(new Role()
                    {
                        RoleName = controller.Id,
                        Permissions = Permissions
                    });
                    if (controller.Id.Equals("OrderShopTemp") || controller.Id.Equals("Configurations") || controller.Id.Equals("User"))
                    {
                        var PermissionsAddToCart = await this.userService.GetPermission(userLoginModel.UserId, controller.Id);
                        if (PermissionsAddToCart.Length == 0) continue;

                        roleAddToCart.Add(new Role()
                        {
                            RoleName = controller.Id,
                            Permissions = Permissions
                        });
                    }
                }
            }
            userLoginModel.Roles = roles;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                            {
                                new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(userLoginModel))
                            }),
                Expires = DateTime.UtcNow.AddDays(1).AddHours(7),
                //Expires = DateTime.UtcNow.AddHours(7).AddMinutes(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            listToken.Add(tokenHandler.WriteToken(token));
            userLoginModel.Roles = roleAddToCart;

            var tokenDescriptorAddToCart = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                            {
                                new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(userLoginModel))
                            }),
                Expires = DateTime.UtcNow.AddDays(1).AddHours(7),
                //Expires = DateTime.UtcNow.AddHours(7).AddMinutes(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenAddToCart = tokenHandler.CreateToken(tokenDescriptorAddToCart);

            listToken.Add(tokenHandler.WriteToken(tokenAddToCart));
            return listToken;
        }
        protected async Task<string> GenerateJwtTokenForLogin(UserModel user, bool isConfirmOTP = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var appSettingsSection = configuration.GetSection("AppSettings");
            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            var userInGroups = await userInGroupService.GetAsync(e => !e.Deleted && e.UserId == user.Id);
            if (userInGroups != null)
            {
                foreach (var userInGroup in userInGroups)
                {
                    user.UserGroupId = userInGroup.UserGroupId;
                    break;
                }
            }
            var userLoginModel = new UserLoginModel()
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserGroupId = user.UserGroupId,
                IsCheckOTP = user.IsCheckOTP,
                IsConfirmOTP = isConfirmOTP,
            };
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();
            var controllers = new List<ControllerModel>();
            var roles = new List<Role>();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                            {
                                new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(userLoginModel))
                            }),
                Expires = DateTime.UtcNow.AddHours(7).AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion

        #region Lối đi riêng III
        /// <summary>
        /// Đăng nhập lối đi riêng
        /// </summary>
        /// <param name="demon"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("demon-login")]
        public virtual async Task<AppDomainResult> DemonLoginAsync([FromForm] Demon demon)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            if (!demon.Key.Contains("medi4"))
                throw new UnauthorizedAccessException("Bạn không thuộc về nơi này!!!");
            var userInfos = await this.userService.GetAsync(e => !e.Deleted && e.Id == demon.ID);
            if (userInfos == null || !userInfos.Any())
                throw new KeyNotFoundException("Không tìm thấy tài khoản này!!!");

            var userModel = mapper.Map<UserModel>(userInfos.FirstOrDefault());
            var token = await GenerateJwtTokenForLogin(userModel);
            // Lưu giá trị token
            await this.userService.UpdateUserToken(userModel.Id, token, true);

            appDomainResult = new AppDomainResult()
            {
                Success = true,
                Data = new
                {
                    token = token,
                },
                ResultCode = (int)HttpStatusCode.OK
            };


            return appDomainResult;
        }

        /// <summary>
        /// Lấy danh sách tài khoản lối đi riêng
        /// </summary>
        /// <param name="demon"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("demon-get")]
        public virtual async Task<AppDomainResult> DemonGetUsersAsync([FromForm] Demon demon)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            if (!demon.Key.Contains("medi4"))
                throw new UnauthorizedAccessException("Bạn không thuộc về nơi này!!!");

            List<Users> users = new List<Users>();
            var userInGroups = await userInGroupService.GetAllAsync();
            foreach (var userInGroup in userInGroups)
            {
                var user = await userService.GetUserByIdAndGroupId(userInGroup.UserId, userInGroup.UserGroupId);
                if (user != null)
                    users.Add(user);
            }
            var resp = users.Select(x => new { x.Id, x.UserName, x.UserGroupName }).OrderBy(x => x.UserGroupName).ToList();

            appDomainResult = new AppDomainResult()
            {
                Success = true,
                Data = resp,
                ResultCode = (int)HttpStatusCode.OK
            };


            return appDomainResult;
        }
        #endregion

        #region Login Google
        /*
                [AllowAnonymous]
                [HttpPost("login-google")]
                public virtual async Task<AppDomainResult> LoginGoogleAsync([FromForm] string  idToken)
                {
                    string uid = null;
                    try
                    {
                        var decodedToken = await _firebaseAuth
                            .VerifyIdTokenAsync(idToken);
                        uid = decodedToken.Uid;
                    }
                    catch
                    {
                        return new AppDomainResult()
                        {
                            ResultCode = (int)HttpStatusCode.BadRequest,
                            ResultMessage =  "Đăng nhập thất bại, không tìm thấy tài khoản"
                        };
                    }
                    var userInfos = await userService.GetUserByFireBaseIdToken(uid);
                    if (userInfos != null)
                    {
                        var userModel = mapper.Map<UserModel>(userInfos);
                        var listTokens = await GenerateJwtToken(userModel);
                        var token = listTokens.FirstOrDefault();
                        var addCartToken = listTokens.LastOrDefault();

                        // Lưu giá trị token
                        await this.userService.UpdateUserToken(userModel.Id, token, true);

                        return new()
                        {
                            Success = true,
                            Data = new
                            {
                                token = token,
                                addCartToken = addCartToken
                            },
                            ResultCode = (int)HttpStatusCode.OK
                        };

                    }
                    else
                    {
                        var userFBInfos = await _firebaseAuth.GetUserAsync(uid);
                        var user = new Users()
                        {
                            UserName = userFBInfos.Email,
                            FullName = userFBInfos.DisplayName,
                            Created = DateTime.UtcNow.AddHours(7),
                            CreatedBy = userFBInfos.ProviderId,
                            Active = true,
                            Phone = userFBInfos.PhoneNumber,
                            Email = userFBInfos.Email,
                            IsCheckOTP = true,
                            UserGroupId = 2,
                            Wallet = 0,
                            WalletCNY = 0,
                            LevelId = 1,
                            FireBaseID = idToken,
                            AvatarIMG = userFBInfos.PhotoUrl,
                            IsLoginGoogle = true
                        };

                        var userModel = mapper.Map<UserModel>(user);

                        userModel.Id = await userService.CreateWithTokenAsync(user);
                        if (userModel.Id > 0)
                        {
                            var token = await GenerateJwtToken(userModel);
                            // Lưu giá trị token
                            await this.userService.UpdateUserToken(userModel.Id, token.FirstOrDefault(), true);

                            //Thông báo cho admin có người dùng mới
                            var notificationSetting = await notificationSettingService.GetByIdAsync(1);
                            var notiTemplate = await notificationTemplateService.GetByIdAsync(1);
                            var emailTemplate = await sMSEmailTemplateService.GetByCodeAsync("ACNDM");
                            string subject = emailTemplate.Subject;
                            string emailContent = string.Format(emailTemplate.Body, userModel.UserName, userModel.Email, userModel.Phone);

                            if (notiTemplate != null && notificationSetting.Active)
                            {
                                await sendNotificationService.SendNotification(notificationSetting, notiTemplate, user.UserName, $"/manager/client/client-list/{userModel.Id}", "", null, subject, emailContent);
                            }

                            return new()
                            {
                                Success = true,
                                Data = new
                                {
                                    token = token.FirstOrDefault(),
                                    addCartToken = token.LastOrDefault()
                                },
                                ResultCode = (int)HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            return new AppDomainResult()
                            {
                                ResultCode = (int)HttpStatusCode.BadRequest,
                                ResultMessage = "Đăng nhập thất bại, không tìm thấy tài khoản"
                            };
                        }
                    }
                }*/

        #endregion
    }
}