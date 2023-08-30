using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Interface.Services.DomainServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Interface.Services
{
    public interface IUserService : IDomainService<Users, UserSearch>
    {
        Task<int> CreateWithTokenAsync(Users item);
        Task<Users> Verify(string userName, string password);
        Task<Users> VerifyForApp(string userName, string password);
        Task<bool> HasPermission(int userId, string controller, IList<int> permissions);
        Task<string[]> GetPermission(int userId, string controller);
        Task<string> CheckCurrentUserPassword(int userId, string password, string newPasssword);
        Task<bool> UpdateUserToken(int userId, string token, bool isLogin = false);
        Task<bool> UpdateUserPassword(int userId, string newPassword);
        Task<bool> IsInUserGroup(int userId, string userGroupCode);
        Task<Users> GetUserByIdAndGroupId(int UID, int groupId);
        Task<Users> GetUserByFireBaseIdToken(string idToken);
        Task<Users> CreateUserTransactionMoney(Users users, decimal totalMoney);
        Task<List<string>> GetUserByOrdererId(int id);
    }
}
