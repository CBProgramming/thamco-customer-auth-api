using AutoMapper;
using CustomerAuthServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Data;
using ThAmCo.Repo.Models;

namespace CustomerAuthServer
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserPutDto, UserPutModel>();
            CreateMap<UserPutModel, UserPutDto>();
            CreateMap<UserGetDto, UserGetModel>();
            CreateMap<UserGetModel, UserGetDto>();
            CreateMap<AppUser, AppUserModel>();
            CreateMap<AppUserModel, AppUser>();
            CreateMap<AppUser, AppUserDto>();
            CreateMap<AppUserDto, AppUser>();
            CreateMap<AppUser, UserGetDto>();
            CreateMap<UserGetDto, AppUser>();
            CreateMap<AppUserDto, AppUserModel>();
            CreateMap<AppUserModel, AppUserDto>();
            CreateMap<UserGetDto, AppUserModel>();
            CreateMap<AppUserModel, UserGetDto>();
        }
    }
}
