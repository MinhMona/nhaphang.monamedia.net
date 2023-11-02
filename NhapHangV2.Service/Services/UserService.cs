using NhapHangV2.Entities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Utilities;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.ExpressionGraph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Entities.Search;
using NhapHangV2.Entities.Auth;
using static NhapHangV2.Utilities.CoreContants;
using Microsoft.Extensions.DependencyInjection;
using OneSignalApi.Client;

namespace NhapHangV2.Service.Services
{
    public class UserService : DomainService<Users, UserSearch>, IUserService
    {
        protected IAppDbContext coreDbContext;
        protected IOrderShopTempService orderShopTempService;

        public UserService(IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext coreDbContext, IServiceProvider serviceProvider) : base(unitOfWork, mapper)
        {
            this.coreDbContext = coreDbContext;
            orderShopTempService = serviceProvider.GetRequiredService<IOrderShopTempService>();
        }
        public async Task<string> GetSaleName(int? saleId)
        {
            var sale = await unitOfWork.Repository<Users>().GetQueryable().FirstOrDefaultAsync(x => x.Id == saleId);
            return sale != null ? sale.UserName : "Chưa xác định";
        }
        protected override string GetStoreProcName()
        {
            return "User_GetPagingData";
        }

        /// <summary>
        /// Kiểm tra user đã tồn tại chưa?
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override async Task<string> GetExistItemMessage(Users item)
        {
            List<string> messages = new List<string>();
            string result = string.Empty;
            bool isExistEmail = !string.IsNullOrEmpty(item.Email) && await Queryable.AnyAsync(x => !x.Deleted && x.Id != item.Id && x.Email == item.Email);
            bool isExistPhone = !string.IsNullOrEmpty(item.Phone) && await Queryable.AnyAsync(x => !x.Deleted && x.Id != item.Id && x.Phone == item.Phone);
            bool isExistUserName = !string.IsNullOrEmpty(item.UserName)
                && await Queryable.AnyAsync(x => !x.Deleted && x.Id != item.Id
                && (x.UserName == item.UserName
                || x.Email == item.UserName
                || x.Phone == item.UserName
                ));
            bool isPhone = ValidateUserName.IsPhoneNumber(item.UserName);
            bool isEmail = ValidateUserName.IsEmail(item.UserName);


            if (isExistEmail)
                messages.Add("Email đã tồn tại!");
            if (isExistPhone)
                messages.Add("Số điện thoại đã tồn tại!");
            if (isExistUserName)
            {
                if (isPhone)
                    messages.Add("Số điện thoại đã tồn tại!");
                else if (isEmail)
                    messages.Add("Email đã tồn tại!");
                else
                    messages.Add("User name đã tồn tại!");
            }
            if (messages.Any())
                result = string.Join(" ", messages);
            return result;
        }

        public override async Task<Users> GetByIdAsync(int id)
        {
            var user = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();
            if (user == null) return null;
            var userInGroups = await unitOfWork.Repository<UserInGroups>().GetQueryable().Where(e => !e.Deleted && e.UserId == id).OrderByDescending(o => o.Id).ToListAsync();
            if (userInGroups != null)
            {
                foreach (var userInGroup in userInGroups)
                {
                    var userGroup = await unitOfWork.Repository<UserGroups>().GetQueryable().Where(e => e.Id == userInGroup.UserGroupId).FirstOrDefaultAsync();
                    if (userGroup == null) continue;
                    user.UserGroupId = userGroup.Id;
                    user.UserGroupName = userGroup.Description;
                    break;
                }
            }

            //Tổng tiền đã nạp
            var adminSendUserWallet = await unitOfWork.Repository<AdminSendUserWallet>().GetQueryable().Where(e => !e.Deleted && e.UID == id && e.Status == (int)WalletStatus.DaDuyet).OrderByDescending(o => o.Id).ToListAsync();
            user.SumAmount = adminSendUserWallet.Sum(e => e.Amount) ?? 0;

            return user;
        }

        /// <summary>
        /// Lưu thông tin người dùng
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override async Task<bool> CreateAsync(Users item)
        {
            bool result = false;
            if (item != null)
            {
                // Tạo mới nhóm người dùng
                item.Id = 0;
                await this.unitOfWork.Repository<Users>().CreateAsync(item);
                await this.unitOfWork.SaveAsync();

                // Lưu thông tin user thuộc nhóm người dùng
                if (item.UserGroupId != 0)
                {
                    UserInGroups userInGroup = new UserInGroups()
                    {
                        UserId = item.Id,
                        UserGroupId = item.UserGroupId,
                        Id = 0
                    };
                    await this.unitOfWork.Repository<UserInGroups>().CreateAsync(userInGroup);
                }

                await this.unitOfWork.SaveAsync();
                await this.coreDbContext.SaveChangesAsync();

                this.coreDbContext.Entry<Users>(item).State = EntityState.Detached;

                result = true;
            }
            return result;
        }

        public async Task<int> CreateWithTokenAsync(Users item)
        {
            int result = 0;
            if (item != null)
            {
                // Tạo mới nhóm người dùng
                item.Id = 0;
                await this.unitOfWork.Repository<Users>().CreateAsync(item);
                await this.unitOfWork.SaveAsync();

                // Lưu thông tin user thuộc nhóm người dùng
                if (item.UserGroupId != 0)
                {
                    UserInGroups userInGroup = new UserInGroups()
                    {
                        UserId = item.Id,
                        UserGroupId = item.UserGroupId,
                        //Id = 0
                    };
                    await this.unitOfWork.Repository<UserInGroups>().CreateAsync(userInGroup);
                }

                await this.unitOfWork.SaveAsync();
                await this.coreDbContext.SaveChangesAsync();

                this.coreDbContext.Entry<Users>(item).State = EntityState.Detached;

                result = item.Id;
            }
            return result;
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override async Task<bool> UpdateAsync(Users item)
        {
            bool result = false;
            var existItem = await this.Queryable.Where(e => e.Id == item.Id).FirstOrDefaultAsync();
            int oldLevel = 0;
            if (existItem != null)
            {
                oldLevel = existItem.LevelId ?? 0;
                //Hủy đăngg ký OneSignal
                if (item.OneSignalPlayerID != null)
                {
                    if (!item.OneSignalPlayerID.Equals(existItem.OneSignalPlayerID) && existItem.OneSignalPlayerID != null)
                    {
                        var unsubscribeResult = await UnsubriseOneSignal(existItem.OneSignalPlayerID);
                    }
                }
                if (!item.IsResetPassword)
                    item.Password = existItem.Password;
                var currentCreated = existItem.Created;
                var currentCreatedByInfo = existItem.CreatedBy;
                existItem = mapper.Map<Users>(item);
                existItem.Created = currentCreated;
                existItem.CreatedBy = currentCreatedByInfo;

                //Sửa đổi tiền tích lũy
                if (item.LevelId != oldLevel)
                {
                    var userLevel = await this.unitOfWork.Repository<UserLevel>().GetQueryable().FirstOrDefaultAsync(e => e.Id == item.LevelId && !e.Deleted);
                    if (userLevel != null)
                    {
                        item.TransactionMoney = userLevel.Money ?? 0;
                    }
                }
                this.unitOfWork.Repository<Users>().Update(existItem);
                await this.unitOfWork.SaveAsync();

                //Cập nhật lại tiền ở giỏ hàng khi thay đổi tỷ giá
                var orderShopTempIds = await unitOfWork.Repository<OrderShopTemp>().GetQueryable().Where(e => !e.Deleted && e.Active && e.UID == existItem.Id).Select(s => s.Id).ToListAsync();
                if (orderShopTempIds.Any())
                {
                    foreach (var orderShopTempId in orderShopTempIds)
                    {
                        var orderShopTemp = await orderShopTempService.GetByIdAsync(orderShopTempId);

                        //Cập nhật tiền
                        orderShopTemp = await orderShopTempService.UpdatePrice(orderShopTemp);

                        unitOfWork.Repository<OrderShopTemp>().Update(orderShopTemp);
                        await this.unitOfWork.SaveAsync(); //Save thử xem sao
                    }
                }

                // Cập nhật thông tin user ở nhóm
                if (item.UserGroupId != 0)
                {
                    var existUserInGroup = await this.unitOfWork.Repository<UserInGroups>().GetQueryable()
                        .Where(e => e.UserGroupId == item.UserGroupId && e.UserId == existItem.Id).FirstOrDefaultAsync();
                    if (existUserInGroup != null)
                    {
                        existUserInGroup.UserGroupId = item.UserGroupId;
                        existUserInGroup.UserId = item.Id;
                        existUserInGroup.Updated = DateTime.Now;
                        this.unitOfWork.Repository<UserInGroups>().Update(existUserInGroup);
                    }
                    else
                    {
                        UserInGroups userInGroup = new UserInGroups()
                        {
                            Created = DateTime.Now,
                            CreatedBy = item.CreatedBy,
                            UserId = item.Id,
                            UserGroupId = item.UserGroupId,
                            Active = true,
                            Deleted = false,
                        };

                        userInGroup.Created = DateTime.Now;
                        userInGroup.UserId = item.Id;
                        userInGroup.Id = 0;
                        await this.unitOfWork.Repository<UserInGroups>().CreateAsync(userInGroup);
                    }

                    // Kiểm tra những item không có trong role chọn => Xóa đi
                    var existGroupOlds = await this.unitOfWork.Repository<UserInGroups>().GetQueryable().Where(e => e.UserGroupId != item.UserGroupId && e.UserId == existItem.Id).ToListAsync();
                    if (existGroupOlds != null)
                    {
                        foreach (var existGroupOld in existGroupOlds)
                        {
                            this.unitOfWork.Repository<UserInGroups>().Delete(existGroupOld);
                        }
                    }
                }
                else
                {
                    var userInGroups = await this.unitOfWork.Repository<UserInGroups>().GetQueryable().Where(e => e.UserId == existItem.Id).ToListAsync();
                    if (userInGroups != null && userInGroups.Any())
                    {
                        foreach (var userInGroup in userInGroups)
                        {
                            this.unitOfWork.Repository<UserInGroups>().Delete(userInGroup);
                        }
                    }
                }

                await this.unitOfWork.SaveAsync();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Cập nhật password mới cho user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserPassword(int userId, string newPassword)
        {
            bool result = false;

            var existUserInfo = await this.unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == userId).FirstOrDefaultAsync();
            if (existUserInfo != null)
            {
                existUserInfo.Password = newPassword;
                existUserInfo.Updated = DateTime.Now;
                Expression<Func<Users, object>>[] includeProperties = new Expression<Func<Users, object>>[]
                {
                    e => e.Password,
                    e => e.Updated
                };
                await this.unitOfWork.Repository<Users>().UpdateFieldsSaveAsync(existUserInfo, includeProperties);
                await this.unitOfWork.SaveAsync();
                result = true;

            }

            return result;
        }

        /// <summary>
        /// Kiểm tra user đăng nhập
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Users> Verify(string userName, string password)
        {
            var user = await Queryable
                .Where(e => !e.Deleted
                && (e.UserName == userName
                || e.Phone == userName
                || e.Email == userName
                )
                )
                .FirstOrDefaultAsync();
            if (user != null)
            {
                if (user.Status == (int)CoreContants.StatusUser.Locked)
                {
                    throw new Exception("Tài khoản đã bị khóa");
                }
                if (user.Status == (int)CoreContants.StatusUser.NotActive)
                {
                    throw new Exception("Tài khoản chưa được kích hoạt");
                }
                if (!user.IsAdmin && !user.IsCheckOTP)
                {
                    throw new Exception("Người dùng chưa xác thực OTP");
                }
                if (user.Password == SecurityUtilities.HashSHA1(password))
                {
                    return user;
                }
                else
                    throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không chính xác");

            }
            else
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không chính xác");
        }

        /// <summary>
        /// Kiểm tra user đăng nhập
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Users> VerifyForApp(string userName, string password)
        {
            var user = await Queryable
                .Where(e => !e.Deleted
                && (e.UserName == userName
                || e.Phone == userName
                || e.Email == userName
                )
                )
                .FirstOrDefaultAsync();
            if (user != null)
            {
                if (user.Status == (int)CoreContants.StatusUser.Locked)
                {
                    throw new Exception("Tài khoản đã bị khóa");
                }
                if (user.Status == (int)CoreContants.StatusUser.NotActive)
                {
                    throw new Exception("Tài khoản chưa được kích hoạt");
                }
                if (!user.IsAdmin && !user.IsCheckOTP)
                {
                    throw new Exception("Người dùng chưa xác thực OTP");
                }
                if (user.Password == SecurityUtilities.HashSHA1(password))
                {
                    return user;
                }
                else
                    throw new AppException("Tên đăng nhập hoặc mật khẩu không chính xác");

            }
            else
                throw new AppException("Tên đăng nhập hoặc mật khẩu không chính xác");
        }

        /// <summary>
        /// Kiểm tra pass word cũ đã giống chưa
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<string> CheckCurrentUserPassword(int userId, string password, string newPasssword)
        {
            string message = string.Empty;
            List<string> messages = new List<string>();
            bool isCurrentPassword = await this.Queryable.AnyAsync(x => x.Id == userId && x.Password == SecurityUtilities.HashSHA1(password));
            bool isDuplicateNewPassword = await this.Queryable.AnyAsync(x => x.Id == userId && x.Password == SecurityUtilities.HashSHA1(newPasssword));
            if (!isCurrentPassword)
                messages.Add("Mật khẩu cũ không chính xác");
            else if (isDuplicateNewPassword)
                messages.Add("Mật khẩu mới không được trùng mật khẩu cũ");
            if (messages.Any())
                message = string.Join("; ", messages);
            return message;
        }

        /// <summary>
        /// Kiểm tra quyền của user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="controller"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async Task<bool> HasPermission(int userId, string controller, IList<int> permissions)
        {
            bool hasPermit = false;

            var userInfo = await unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == userId).FirstOrDefaultAsync();
            if (userInfo != null && userInfo.Status == (int)CoreContants.StatusUser.Locked)
                throw new AppException("Tài khoản đã bị khóa!");

            if (userInfo.IsAdmin) return true;

            if (controller.Equals("Warehouse") || controller.Equals("WarehouseFrom") || controller.Equals("ShippingTypeToWareHouse"))
                return true;
            // Lấy ra những nhóm user thuộc
            var userGroupIds = await unitOfWork.Repository<UserInGroups>().GetQueryable()
                .Where(e => e.UserId == userId)
                .Select(e => e.UserGroupId).ToListAsync();

            var permissionIds = new List<string>();
            var permitObjectIds = new List<int>();

            if (userGroupIds != null && userGroupIds.Any())
            {
                var permitObjectChecks = await unitOfWork.Repository<PermitObjects>().GetQueryable().Where(e => !e.Deleted
                && !string.IsNullOrEmpty(e.ControllerNames)
                && e.ControllerNames.Contains(controller)
                ).ToListAsync();
                permitObjectChecks = permitObjectChecks.Where(e => e.ControllerNames.Split(";", StringSplitOptions.None).Contains(controller)).ToList();
                if (permitObjectChecks != null && permitObjectChecks.Any())
                {
                    var permitObjectCheckIds = permitObjectChecks.Select(e => e.Id).ToList();
                    // Lấy ra những quyền user có trong chức năng cần kiểm tra
                    var permitObjectPermissions = await unitOfWork.Repository<PermitObjectPermissions>().GetQueryable()
                    .Where(e => e.UserGroupId.HasValue
                    && userGroupIds.Contains(e.UserGroupId.Value)
                    && permitObjectCheckIds.Contains(e.PermitObjectId)
                    )
                    .ToListAsync();
                    if (permitObjectPermissions != null && permitObjectPermissions.Any())
                    {
                        permitObjectIds = permitObjectPermissions.Select(e => e.PermitObjectId).Distinct().ToList();

                        foreach (var permitObjectId in permitObjectIds)
                        {
                            // Lấy danh mục mã quyền user cần kiểm tra
                            permissionIds = permitObjectPermissions.Where(e => e.PermitObjectId == permitObjectId).Select(e => e.Permissions).ToList();
                            var result = true;
                            for (int i = 0; i < permissions.Count; i++)
                            {
                                var lst = permissionIds[i].Split(',');
                                if (Convert.ToInt16(lst[permissions[i] - 1]) == 0)
                                {
                                    result = false;
                                    break;
                                }
                            }

                            //Tạm thời là như vầy
                            hasPermit = result;

                            //// Lấy danh mục mã quyền user cần kiểm tra
                            //permissionIds = permitObjectPermissions.Where(e => e.PermitObjectId == permitObjectId).Select(e => e.PermissionId).ToList();
                            //var permissionCodes = await unitOfWork.Repository<Permissions>().GetQueryable().Where(e => permissionIds.Contains(e.Id))
                            //    .Select(e => e.Code)
                            //    .ToListAsync();

                            //// Lấy danh chức năng cần kiểm tra
                            //var permitObjectControllers = await unitOfWork.Repository<PermitObjects>().GetQueryable().Where(e => permitObjectIds.Contains(e.Id))
                            //    .Select(e => e.ControllerNames.Split(";", StringSplitOptions.None))
                            //    .ToListAsync();

                            //// Kiểm tra user có quyền trong chức năng không
                            //if (permissionCodes != null && permissionCodes.Any() && permitObjectControllers != null && permitObjectControllers.Any())
                            //{
                            //    hasPermit = permitObjectControllers.Any(x => x.Contains(controller)) && permissions.Any(x => permissionCodes.Contains(x));
                            //}
                        }
                    }
                }

            }
            return hasPermit;
        }

        /// <summary>
        /// Kiểm tra user có trong nhóm chỉ định không
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userGroupCode"></param>
        /// <returns></returns>
        public async Task<bool> IsInUserGroup(int userId, string userGroupCode)
        {
            bool result = false;
            var userGroupInfo = await this.unitOfWork.Repository<UserGroups>().GetQueryable()
                .Where(e => !e.Deleted && e.Active && e.Code == userGroupCode).FirstOrDefaultAsync();
            if (userGroupInfo != null)
            {
                result = await this.unitOfWork.Repository<UserInGroups>().GetQueryable()
                    .AnyAsync(e => !e.Deleted && e.Active && e.UserGroupId == userGroupInfo.Id && e.UserId == userId);
            }
            else throw new AppException("Không tìm thấy thông tin nhóm người dùng");


            return result;
        }

        /// <summary>
        /// Cập nhật thông tin user token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isLogin"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserToken(int userId, string token, bool isLogin = false)
        {
            bool result = false;

            var userInfo = await this.unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == userId).FirstOrDefaultAsync();
            //this.coreDbContext.Entry<Users>(userInfo).State = EntityState.Detached;
            if (userInfo != null)
            {
                if (isLogin)
                {
                    userInfo.Token = token;
                    userInfo.ExpiredDate = DateTime.Now.AddDays(1);
                    //userInfo.ExpiredDate = DateTime.Now.AddMinutes(1);
                }
                else
                {
                    userInfo.Token = string.Empty;
                    userInfo.ExpiredDate = null;
                }
                Expression<Func<Users, object>>[] includeProperties = new Expression<Func<Users, object>>[]
                {
                    e => e.Token,
                    e => e.ExpiredDate
                };
                this.unitOfWork.Repository<Users>().UpdateFieldsSave(userInfo, includeProperties);
                await this.unitOfWork.SaveAsync();
                result = true;
            }

            return result;
        }

        public async Task<string[]> GetPermission(int userId, string controller)
        {
            List<string> result = new List<string>();

            var userInfo = await unitOfWork.Repository<Users>().GetQueryable().Where(e => e.Id == userId).FirstOrDefaultAsync();
            if (userInfo != null && userInfo.Status == (int)CoreContants.StatusUser.Locked)
                throw new AppException("Tài khoản đã bị khóa!");

            // Lấy ra những nhóm user thuộc
            var userGroupIds = await unitOfWork.Repository<UserInGroups>().GetQueryable()
                .Where(e => e.UserId == userId)
                .Select(e => e.UserGroupId).ToListAsync();

            var permissionIds = new List<string>();
            var permitObjectIds = new List<int>();

            if (userGroupIds != null && userGroupIds.Any())
            {
                var permitObjectChecks = await unitOfWork.Repository<PermitObjects>().GetQueryable().Where(e => !e.Deleted
                && !string.IsNullOrEmpty(e.ControllerNames)
                && e.ControllerNames.Contains(controller)
                ).ToListAsync();
                permitObjectChecks = permitObjectChecks.Where(e => e.ControllerNames.Split(";", StringSplitOptions.None).Contains(controller)).ToList();
                if (permitObjectChecks != null && permitObjectChecks.Any())
                {
                    var permitObjectCheckIds = permitObjectChecks.Select(e => e.Id).ToList();
                    // Lấy ra những quyền user có trong chức năng cần kiểm tra
                    var permitObjectPermissions = await unitOfWork.Repository<PermitObjectPermissions>().GetQueryable()
                    .Where(e => e.UserGroupId.HasValue
                    && userGroupIds.Contains(e.UserGroupId.Value)
                    && permitObjectCheckIds.Contains(e.PermitObjectId)
                    )
                    .ToListAsync();
                    if (permitObjectPermissions != null && permitObjectPermissions.Any())
                    {
                        permitObjectIds = permitObjectPermissions.Select(e => e.PermitObjectId).Distinct().ToList();

                        foreach (var permitObjectId in permitObjectIds)
                        {
                            // Lấy danh mục mã quyền user cần kiểm tra
                            permissionIds = permitObjectPermissions.Where(e => e.PermitObjectId == permitObjectId).Select(e => e.Permissions).ToList();
                            for (int i = 0; i < permissionIds.Count; i++)
                            {
                                var lst = permissionIds[i].Split(',');
                                for (int j = 0; j < lst.Length; j++)
                                {
                                    if (Convert.ToInt32(lst[j]) == 0) continue;
                                    result.Add((j + 1).ToString());
                                }
                            }
                        }
                    }
                }
            }
            return result.ToArray();
        }

        public async Task<Users> GetUserByIdAndGroupId(int UID, int groupId)
        {
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == UID && !x.Deleted).FirstOrDefaultAsync();
            if (user != null)
            {
                var userGroup = await unitOfWork.Repository<UserGroups>().GetQueryable().Where(x => x.Id == groupId && !x.Deleted).FirstOrDefaultAsync();
                if (userGroup != null)
                    user.UserGroupName = userGroup.Description;
                return user;
            }
            return null;
        }

        public async Task<Users> GetUserByFireBaseIdToken(string idToken)
        {
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.FireBaseID.Equals(idToken)).FirstOrDefaultAsync();
            return user;
        }

        public async Task<Users> CreateUserTransactionMoney(Users users, decimal totalMoney)
        {
            var userTransactionMoney = users.TransactionMoney += totalMoney;
            var userLevel = await unitOfWork.Repository<UserLevel>().GetQueryable()
                .FirstOrDefaultAsync(x => !x.Deleted && userTransactionMoney >= x.Money && userTransactionMoney < x.MoneyTo);
            if (userLevel.Id != (users.LevelId ?? 0))
            {
                users.LevelId = userLevel.Id;
                users.DateUpLevel = DateTime.Now;
            }
            return users;
        }

        private async Task<string> UnsubriseOneSignal(string playerId)
        {
            var config = await this.unitOfWork.Repository<NhapHangV2.Entities.Configurations>().GetQueryable().FirstOrDefaultAsync();
            var appConfig = new Configuration();
            appConfig.BasePath = "https://onesignal.com/api/v1";
            appConfig.AccessToken = config.RestAPIKey;
            var api = new OneSignalApi.Api.DefaultApi(appConfig);
            try
            {
                var result = await api.DeletePlayerAsync(config.OneSignalAppID, playerId);
                return result.Success;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
