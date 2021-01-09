using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThAmCo.Data;
using ThAmCo.Repo.Models;

namespace ThAmCo.Repo
{
    public class Repository : IRepository
    {
        private UserManager<AppUser> UserManager { get; }
        private IMapper _mapper;

        public Repository(UserManager<AppUser> userManager, IMapper mapper)
        {
            UserManager = userManager;
            _mapper = mapper;
        }
        public async Task<bool> DeleteUser(string authId)
        {
            //var user = await UserManager.Users.FirstOrDefaultAsync(u => u.Id == authId);
            var user = await UserManager.FindByIdAsync(authId);
            if (user == null)
            {
                return false;
            }

            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> EditUser(UserPutModel updatedUser, string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            user.Email = updatedUser.Email ?? user.Email;
            user.UserName = user.Email;
            try
            {
                await UserManager.UpdateAsync(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                try
                {
                    await UserManager.RemovePasswordAsync(_mapper.Map<AppUser>(user));
                    await UserManager.AddPasswordAsync(_mapper.Map<AppUser>(user), updatedUser.Password);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<UserGetModel> NewUser(UserPutModel newUser)
        {
            var user = new AppUser
            {
                Email = newUser.Email,
                UserName = newUser.Email,
            };
            var result = await UserManager.CreateAsync(user, newUser.Password);
            if (!result.Succeeded)
            {
                return null;
            }
            try
            {
                await UserManager.AddToRoleAsync(user, "Customer");
            }
            catch (DbUpdateException)
            {

            }
            
            var roles = await UserManager.GetRolesAsync(user);

            var dto = new UserGetModel
            {
                Id = user.Id,
                CustomerId = user.CustomerId,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            };
            return dto;
        }

        public async Task<AppUserModel> GetUser(string authId)
        {
            var result = _mapper.Map<AppUserModel>(await GetAppUser(authId));
            return result;
        }

        public async Task<IList<string>> GetRoles(string userId)
        {
            return await UserManager.GetRolesAsync(await GetAppUser(userId));
        }

        private async Task<AppUser> GetAppUser(string authId)
        {
            AppUser user = await UserManager.FindByIdAsync(authId);
            if (user == null)
            {
                return null;
            }
            return user;
        }
    }
}
