using AutoMapper;
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
        public Mock<AccountDbContext> mockDbContext;
        public Repository repo;
    }
}
