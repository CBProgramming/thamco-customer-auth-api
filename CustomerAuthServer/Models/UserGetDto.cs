using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAuthServer.Models
{
    public class UserGetDto
    {
        public string Id { get; set; }
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
