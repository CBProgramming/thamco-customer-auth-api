using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThAmCo.Data;
using ThAmCo.Repo.Models;

namespace ThAmCo.Repo
{
    public class FakeRepository : IRepository
    {
        public string DefaultUserId = "defaultUserId";
        public string DefaultRole = "Customer";
        public int DefaultCustomerId = 1;
        public string Password;
        public string Email;
        public string UserName;
        public bool Succeeds = true;
        public UserGetModel UserGetModel;
        public AppUserModel AppUser;

        public async Task<bool> DeleteUser(string authId)
        {
            if (Succeeds && authId == DefaultUserId)
            {
                DefaultUserId = null;
                return true;
            }
            return false;
        }

        public async Task<bool> EditUser(UserPutModel updatedUser, string userId)
        {
            if (Succeeds)
            {
                Password = updatedUser.Password;
                Email = updatedUser.Email;
                UserName = updatedUser.Email;
                return true;
            }
            return false;
        }

        public async  Task<IList<string>> GetRoles(string userId)
        {
            if (Succeeds)
            {
                return new List<string> { DefaultRole };
            }
            return new List<string>();
        }

        public async Task<AppUserModel> GetUser(string authId)
        {
            if (Succeeds && AppUser !=null && authId == AppUser.Id)
            {
                return AppUser;
            }
            return null;
        }

        public async Task<UserGetModel> NewUser(UserPutModel newUser)
        {
            if (Succeeds)
            {
                Password = newUser.Password;
                return new UserGetModel
                {
                    CustomerId = DefaultCustomerId,
                    Id = DefaultUserId,
                    Email = newUser.Email,
                    Roles = new List<string> { DefaultRole },
                    UserName = newUser.Email
                };
            }
            return null;
        }
    }
}
