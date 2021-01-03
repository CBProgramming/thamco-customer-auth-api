using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThAmCo.Data;
using ThAmCo.Repo.Models;

namespace ThAmCo.Repo
{
    public interface IRepository
    {
        Task<UserGetModel> NewUser(UserPutModel newUser);

        Task<bool> EditUser(UserPutModel updatedUser, string userId);

        Task<bool> EditUserPassword(AppUserModel user, string password);

        Task<bool> DeleteUser(string authId);

        Task<IList<string>> GetRoles(string userId);

        Task<AppUser> GetUser(string authId);
    }
}