using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ThAmCo.Data
{
    public class AppUser : IdentityUser
    {
        public int CustomerId { get; set; }
    }
}
