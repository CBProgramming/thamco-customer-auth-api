using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThAmCo.Data
{
    public class AppRole : IdentityRole
    {
        public string Descriptor { get; set; }
    }
}
