using AutoMapper;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAmCo.Data;
using ThAmCo.Repo;
using ThAmCo.Repo.Models;

namespace AuthTests
{
    public class RepositoryTests
    {
        public UserPutModel userPutModel;
        public IMapper mapper;
        public IQueryable<AppUser> dbAppUsers;
        public AppUser dbAppUser;
        public Mock<DbSet<AppUser>> mockAppUsers;
        public IQueryable<AppRole> dbAppRoles;
        public AppRole dbAppRole;
        public Mock<DbSet<AppRole>> mockAppRoles;
        public Mock<AccountDbContext> mockDbContext;
        public Repository repo;
        public string email = "email@email.com";
        public string password = "password";
        public string authId = "authId";
        public int customerId = 1;

        public void SetupUserPutModel()
        {
            userPutModel = new UserPutModel
            {
                Email = email,
                Password = password
            };
        }

        private void SetupDbAppUser()
        {
            dbAppUser = new AppUser
            {
                Email = email,
                PasswordHash = password.Sha256(),
                Id = authId,
                CustomerId = customerId,
                UserName = email
            };
        }

        private void SetupDbAppUsers()
        {
            SetupDbAppUser();
            dbAppUsers = new List<AppUser>
            {
                dbAppUser
            }.AsQueryable();
        }

        private void SetupMockCustomers()
        {
            mockAppUsers = new Mock<DbSet<AppUser>>();
            mockAppUsers.As<IQueryable<AppUser>>().Setup(m => m.Provider).Returns(dbAppUsers.Provider);
            mockAppUsers.As<IQueryable<AppUser>>().Setup(m => m.Expression).Returns(dbAppUsers.Expression);
            mockAppUsers.As<IQueryable<AppUser>>().Setup(m => m.ElementType).Returns(dbAppUsers.ElementType);
            mockAppUsers.As<IQueryable<AppUser>>().Setup(m => m.GetEnumerator()).Returns(dbAppUsers.GetEnumerator());
        }

        private void SetupDbAppRole()
        {
            dbAppRole = new AppRole
            {
                Name = "Customer"
            };
        }

        private void SetupDbAppRoless()
        {
            SetupDbAppRole();
            dbAppRoles = new List<AppRole>
            {
                dbAppRole
            }.AsQueryable();
        }

        private void SetupMockAppRoles()
        {
            mockAppRoles = new Mock<DbSet<AppRole>>();
            mockAppRoles.As<IQueryable<AppRole>>().Setup(m => m.Provider).Returns(dbAppRoles.Provider);
            mockAppRoles.As<IQueryable<AppRole>>().Setup(m => m.Expression).Returns(dbAppRoles.Expression);
            mockAppRoles.As<IQueryable<AppRole>>().Setup(m => m.ElementType).Returns(dbAppRoles.ElementType);
            mockAppRoles.As<IQueryable<AppRole>>().Setup(m => m.GetEnumerator()).Returns(dbAppRoles.GetEnumerator());
        }

        private void SetupMockDbContext()
        {
            mockDbContext = new Mock<AccountDbContext>();
            mockDbContext.Setup(m => m.Users).Returns(mockAppUsers.Object);
        }

        //ran out of time
    }
}
