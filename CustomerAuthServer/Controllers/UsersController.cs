using AutoMapper;
using CustomerAuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThAmCo.Repo;
using ThAmCo.Repo.Models;

namespace CustomerAuthServer.Controllers
{
    //
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IRepository _repo;
        private IMapper _mapper;
        private string authId, clientId, tokenCustomerId;

        public UsersController(IRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        //only customers can access
        [Authorize(AuthenticationSchemes = "customer_web_app")]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserPutDto newUser)
        {
            if (newUser == null)
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(newUser.Email)
                || string.IsNullOrEmpty(newUser.Password))
            {
                return UnprocessableEntity();
            }
            var result = _mapper.Map<UserGetDto>(await _repo.NewUser(_mapper.Map<UserPutModel>(newUser)));
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        //both customers and staff can access (via customer_web_app and customer_account_api respectively
        [Authorize(AuthenticationSchemes = "customer_account_api,customer_web_app")]
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string userId,
                                                    [FromBody] UserPutDto updatedUser)
        {
            if (string.IsNullOrEmpty(userId) || updatedUser == null)
            {
                return BadRequest();
            }
            if (string.IsNullOrEmpty(updatedUser.Email)
                || string.IsNullOrEmpty(updatedUser.Password))
            {
                return UnprocessableEntity();
            }
            var user = _mapper.Map<UserGetDto>(await _repo.GetUser(userId));
            if (user == null || !await _repo.EditUser(_mapper.Map<UserPutModel>(updatedUser), userId))
            {
                return NotFound();
            }
            user.Email = updatedUser.Email;
            user.UserName = updatedUser.Email;
            user.Roles = await _repo.GetRoles(userId);
            return Ok(user);
        }

        //both customers and staff can access (via customer_web_app and customer_account_api respectively
        [Authorize(AuthenticationSchemes = "customer_account_api,customer_web_app")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveUser([FromRoute] string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }
            if (await _repo.DeleteUser(userId))
            {
                return Ok();
            }
            return NotFound();
        }

        [HttpGet("{userId}")]
        [Authorize(AuthenticationSchemes = "customer_account_api,customer_web_app")]
        public async Task<IActionResult> GetUser([FromRoute] string userId)
        {
            var user = _mapper.Map<UserGetDto>(await _repo.GetUser(userId));
            if (user == null)
            {
                return NotFound();
            }
            user.Roles = await _repo.GetRoles(userId);

            return Ok(user);
        }
    }
}
