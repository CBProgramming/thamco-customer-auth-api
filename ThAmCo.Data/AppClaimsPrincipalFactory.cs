﻿using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ThAmCo.Data
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
    {
        public AppClaimsPrincipalFactory(UserManager<AppUser> userManager,
                                         RoleManager<AppRole> roleManager,
                                         IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public async override Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var principal = await base.CreateAsync(user);
            var claimsIdentity = principal.Identity as ClaimsIdentity;
            claimsIdentity.AddClaims(new[] {
                //new Claim(JwtClaimTypes.Role, user.Role),
                new Claim(JwtClaimTypes.Id, user.CustomerId.ToString())
            });

            return principal;
        }
    }
}
