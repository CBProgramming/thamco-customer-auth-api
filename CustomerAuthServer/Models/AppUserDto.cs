using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAuthServer.Models
{
    public class AppUserDto : IdentityUser
    {
        public int CustomerId { get; set; }
    }
}
