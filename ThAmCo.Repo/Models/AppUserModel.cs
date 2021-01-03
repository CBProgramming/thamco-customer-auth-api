using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace ThAmCo.Repo.Models
{
    public class AppUserModel : IdentityUser
    {
        public int CustomerId { get; set; }
    }
}
