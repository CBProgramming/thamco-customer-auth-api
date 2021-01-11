using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using CustomerAuthServer;
using CustomerAuthServer.Controllers;
using CustomerAuthServer.Models;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ThAmCo.Data;
using ThAmCo.Repo;
using ThAmCo.Repo.Models;
using Xunit;

namespace AuthTests
{
    public class UsersControllerTests
    {
        private UserPutDto userPutDto;
        private UserGetModel userGetModel;
        private AppUserModel appUser;
        private FakeRepository fakeRepo;
        private Mock<IRepository> mockRepo;
        private IMapper mapper;
        private ILogger<UsersController> logger;
        private UsersController controller;
        private readonly string standardRole = "Customer";
        private readonly string email = "test@email.com";
        private readonly string password = "Password1!";
        private readonly string authId = "defaultUserId";
        private IList<string> roles;
        private bool userExists = true;
        private bool deleteUserSucceeds = true;
        private bool editUserSucceeds = true;
        private bool editUserPasswordSucceeds = true;
        private bool getRolesSucceeds = true;
        private bool getUserSucceeds = true;
        private bool newUserSucceeds = true;

        private void SetupRoles()
        {
            roles = new List<string> { standardRole };
        }

        private void SetupUserPutDto()
        {
            userPutDto = new UserPutDto
            {
                Email = email,
                Password = password
            };
        }

        private void SetupUserGetModel()
        {
            userGetModel = new UserGetModel
            {
                Id = authId,
                CustomerId = 1,
                UserName = email,
                Email = email,
                Roles = new List<string>
                {
                    standardRole
                }
            };
        }

        private void SetupUserPutModel()
        {
            userGetModel = new UserGetModel
            {
                Id = authId,
                CustomerId = 1,
                UserName = email,
                Email = email,
                Roles = new List<string>
                {
                    standardRole
                }
            };
        }

        private void SetupAppUser()
        {
            appUser = new AppUserModel
            {
                CustomerId = 1,
                Id = authId,
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                PasswordHash = password.Sha256(),
                SecurityStamp = "UNKNOWN",
                ConcurrencyStamp = "unknown",
                LockoutEnabled = true
            };
        }

        private void SetFakeRepo()
        {
            fakeRepo = new FakeRepository
            {
                UserGetModel = userGetModel,
                AppUser = appUser
            };
        }

        private void SetMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void SetLogger()
        {
            logger = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<UsersController>();
        }

        private void SetMockReviewRepo()
        {
            mockRepo = new Mock<IRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.DeleteUser(It.IsAny<string>()))
                .ReturnsAsync(userExists && deleteUserSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.EditUser(It.IsAny<UserPutModel>(),It.IsAny<string>()))
                .ReturnsAsync(userExists && editUserSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.GetRoles(It.IsAny<string>()))
                .ReturnsAsync(userExists && deleteUserSucceeds?roles:new List<string>()).Verifiable();
            mockRepo.Setup(repo => repo.GetUser(It.IsAny<string>()))
                .ReturnsAsync(userExists && getUserSucceeds? appUser : null).Verifiable();
            mockRepo.Setup(repo => repo.NewUser(It.IsAny<UserPutModel>()))
                .ReturnsAsync(!userExists && newUserSucceeds ? userGetModel : null).Verifiable();
        }

        private void SetupUser(UsersController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "name"),
                                        new Claim(ClaimTypes.Name, "name"),
                                        new Claim(OpenIdConnectConstants.Claims.Subject, authId ),
                                        new Claim("client_id","customer_web_app"),
                                        new Claim("id", "1")
                                   }, "TestAuth")); ; ;
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void DefaultSetup()
        {
            SetupRoles();
            SetupUserPutDto();
            SetupUserGetModel();
            SetupAppUser();
            SetMapper();
            SetLogger();
        }

        private void SetupWithFakes()
        {
            DefaultSetup();
            SetFakeRepo();
            controller = new UsersController(fakeRepo, mapper);
            SetupUser(controller);
        }

        private void SetupWithMocks()
        {
            DefaultSetup();
            SetMockReviewRepo();
            controller = new UsersController(mockRepo.Object, mapper);
            SetupUser(controller);
        }

        [Fact]
        public async void AddUser_ShouldOkObject()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            var okObjResult = result as OkObjectResult;
            Assert.NotNull(okObjResult);
            var userGetDto = okObjResult.Value as UserGetDto;
            Assert.NotNull(userGetDto);
            Assert.Equal(fakeRepo.DefaultCustomerId, userGetDto.CustomerId);
            Assert.Equal(fakeRepo.DefaultUserId, userGetDto.Id);
            Assert.Equal(userPutDto.Email, userGetDto.Email);
            Assert.Equal(userPutDto.Email, userGetDto.UserName);
            Assert.Equal(roles.Count, userGetDto.Roles.Count);
            for (int i = 0; i < userGetDto.Roles.Count; i++)
            {
                Assert.Equal(roles[i], userGetDto.Roles[i]);
            }
            Assert.Equal(userPutDto.Password, fakeRepo.Password);
        }

        [Fact]
        public async void AddUser_NullEmail_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            userPutDto.Email = null;

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void AddUser_EmptyEmail_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            userPutDto.Email = "";

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void AddUser_NullPassword_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            userPutDto.Password = null;

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void AddUser_EmptyPassword_ShouldUnprocessableEntity()
        {
            //Arrange
            SetupWithFakes();
            userPutDto.Password = "";

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void AddUser_RepoFails_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Succeeds = false;

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as NotFoundResult;
            Assert.NotNull(result);
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_ShouldOkObject()
        {
            //Arrange
            SetupWithFakes();
            string newEmail = "newEmail@email.com";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var okObjResult = result as OkObjectResult;
            Assert.NotNull(okObjResult);
            var userDto = okObjResult.Value as UserGetDto;
            Assert.NotNull(userDto);
            Assert.Equal(fakeRepo.DefaultCustomerId, userDto.CustomerId);
            Assert.Equal(fakeRepo.DefaultUserId, userDto.Id);
            Assert.Equal(userPutDto.Email, userDto.Email);
            Assert.Equal(userPutDto.Email, userDto.UserName);
            Assert.Equal(roles.Count, userDto.Roles.Count);
            for (int i = 0; i < userDto.Roles.Count; i++)
            {
                Assert.Equal(roles[i], userDto.Roles[i]);
            }
            Assert.Equal(userPutDto.Password, fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_NullId_ShouldBadRequest()
        {
            //Arrange
            SetupWithFakes();
            string newEmail = "newEmail@email.com";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(null, userPutDto);

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_EmptyId_ShouldBadRequest()
        {
            //Arrange
            SetupWithFakes();
            string newEmail = "newEmail@email.com";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser("", userPutDto);

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_NullPassword_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            string newEmail = "newEmail@email.com";
            string newPassword = null;
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_EmptyPassword_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            string newEmail = "newEmail@email.com";
            string newPassword = "";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_NullEmail_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            string newEmail = null;
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_EmptyEmail_ShouldUnprocessable()
        {
            //Arrange
            SetupWithFakes();
            string newEmail = "";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_AuthIdDoesntExists_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            

            //Act
            var result = await controller.UpdateUser("different", userPutDto);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void EditUser_RepoFails_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Succeeds = false;


            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            Assert.Equal(userGetModel.CustomerId, fakeRepo.DefaultCustomerId);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
            fakeRepo.Succeeds = true;
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
            Assert.Null(fakeRepo.Password);
        }

        [Fact]
        public async void RemoveUser_ShouldOk()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.RemoveUser(authId);

            //Assert
            Assert.NotNull(result);
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
            Assert.Null(fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
        }

        [Fact]
        public async void RemoveUser_NullId_ShouldBadRequest()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.RemoveUser(null);

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
        }

        [Fact]
        public async void RemoveUser_EmptyId_ShouldBadRequest()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.RemoveUser("");

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
        }

        [Fact]
        public async void RemoveUser_UserDoesntExists_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.RemoveUser("different");

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
        }

        [Fact]
        public async void RemoveUser_RepoFails_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Succeeds = false;

            //Act
            var result = await controller.RemoveUser(authId);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            Assert.Equal(authId, fakeRepo.DefaultUserId);
            Assert.Null(fakeRepo.Email);
            Assert.Null(fakeRepo.UserName);
        }

        [Fact]
        public async void GetUser_ShouldOkObject()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.GetUser(authId);

            //Assert
            Assert.NotNull(result);
            var okObjResult = result as OkObjectResult;
            Assert.NotNull(okObjResult);
            var user = okObjResult.Value as UserGetDto;
            Assert.NotNull(user);
            Assert.Equal(fakeRepo.AppUser.Id, user.Id);
            Assert.Equal(fakeRepo.AppUser.CustomerId, user.CustomerId);
            Assert.Equal(fakeRepo.AppUser.Email, user.Email);
            Assert.Equal(fakeRepo.AppUser.UserName, user.UserName);
            var repoRoles = await fakeRepo.GetRoles(authId);
            Assert.Equal(roles.Count, repoRoles.Count);
            for (int i = 0; i < repoRoles.Count; i++)
            {
                Assert.Equal(roles[i], repoRoles[i]);
            }
        }

        [Fact]
        public async void GetUser_UserDoesntExist_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();

            //Act
            var result = await controller.GetUser("different");

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
        }

        [Fact]
        public async void GetUser_RepoFails_ShouldNotFound()
        {
            //Arrange
            SetupWithFakes();
            fakeRepo.Succeeds = false;

            //Act
            var result = await controller.GetUser(authId);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
        }

        [Fact]
        public async void AddUser_ShouldOkObject_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            var okObjResult = result as OkObjectResult;
            Assert.NotNull(okObjResult);
            var userGetDto = okObjResult.Value as UserGetDto;
            Assert.NotNull(userGetDto);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Once);
        }

        [Fact]
        public async void AddUser_NullEmail_ShouldUnprocessableEntity_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();
            userPutDto.Email = null;

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void AddUser_EmptyEmail_ShouldUnprocessableEntity_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();
            userPutDto.Email = "";

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void AddUser_NullPassword_ShouldUnprocessableEntity_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();
            userPutDto.Password = null;

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void AddUser_EmptyPassword_ShouldUnprocessableEntity_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();
            userPutDto.Password = "";

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as UnprocessableEntityResult;
            Assert.NotNull(result);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void AddUser_RepoFails_ShouldNotFound_CheckMocks()
        {
            //Arrange
            userExists = false;
            newUserSucceeds = false;
            SetupWithMocks();

            //Act
            var result = await controller.AddUser(userPutDto);

            //Assert
            Assert.NotNull(result);
            result = result as NotFoundResult;
            Assert.NotNull(result);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Once);
        }

        [Fact]
        public async void EditUser_ShouldOkObject_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            string newEmail = "newEmail@email.com";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var okObjResult = result as OkObjectResult;
            Assert.NotNull(okObjResult);
            var userDto = okObjResult.Value as UserGetDto;
            Assert.NotNull(userDto);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_NullId_ShouldBadRequest_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            string newEmail = "newEmail@email.com";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(null, userPutDto);

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_EmptyId_ShouldBadRequest_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            string newEmail = "newEmail@email.com";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser("", userPutDto);

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_NullPassword_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            string newEmail = "newEmail@email.com";
            string newPassword = null;
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_EmptyPassword_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            string newEmail = "newEmail@email.com";
            string newPassword = "";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_NullEmail_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            string newEmail = null;
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_EmptyEmail_ShouldUnprocessable_CheckMocks()
        {
            //Arrange
            SetupWithMocks();
            string newEmail = "";
            string newPassword = "newPassword";
            userPutDto.Email = newEmail;
            userPutDto.Password = newPassword;

            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var ueResult = result as UnprocessableEntityResult;
            Assert.NotNull(ueResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_AuthIdDoesntExists_ShouldNotFound_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();


            //Act
            var result = await controller.UpdateUser("different", userPutDto);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void EditUser_RepoFails_ShouldNotFound_CheckMocks()
        {
            //Arrange
            editUserSucceeds = false;
            SetupWithMocks();


            //Act
            var result = await controller.UpdateUser(authId, userPutDto);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void RemoveUser_ShouldOk_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.RemoveUser(authId);

            //Assert
            Assert.NotNull(result);
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void RemoveUser_NullId_ShouldBadRequest_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.RemoveUser(null);

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void RemoveUser_EmptyId_ShouldBadRequest_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.RemoveUser("");

            //Assert
            Assert.NotNull(result);
            var brResult = result as BadRequestResult;
            Assert.NotNull(brResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void RemoveUser_UserDoesntExists_ShouldNotFound_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.RemoveUser("different");

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void RemoveUser_RepoFails_ShouldNotFound_CheckMocks()
        {
            //Arrange
            deleteUserSucceeds = false;
            SetupWithMocks();

            //Act
            var result = await controller.RemoveUser(authId);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void GetUser_ShouldOkObject_CheckMocks()
        {
            //Arrange
            SetupWithMocks();

            //Act
            var result = await controller.GetUser(authId);

            //Assert
            Assert.NotNull(result);
            var okObjResult = result as OkObjectResult;
            Assert.NotNull(okObjResult);
            var user = okObjResult.Value as UserGetDto;
            Assert.NotNull(user);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void GetUser_UserDoesntExist_ShouldNotFound_CheckMocks()
        {
            //Arrange
            userExists = false;
            SetupWithMocks();

            //Act
            var result = await controller.GetUser("different");

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }

        [Fact]
        public async void GetUser_RepoFails_ShouldNotFound_CheckMocks()
        {
            //Arrange
            getUserSucceeds = false;
            SetupWithMocks();

            //Act
            var triggerRelease = "trigger";
            var result = await controller.GetUser(authId);

            //Assert
            Assert.NotNull(result);
            var nfResult = result as NotFoundResult;
            Assert.NotNull(nfResult);
            mockRepo.Verify(repo => repo.DeleteUser(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.EditUser(It.IsAny<UserPutModel>(), It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetRoles(It.IsAny<string>()), Times.Never);
            mockRepo.Verify(repo => repo.GetUser(It.IsAny<string>()), Times.Once);
            mockRepo.Verify(repo => repo.NewUser(It.IsAny<UserPutModel>()), Times.Never);
        }
    }
}
