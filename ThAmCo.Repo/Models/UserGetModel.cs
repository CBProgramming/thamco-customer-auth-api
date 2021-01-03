using System;
using System.Collections.Generic;
using System.Text;

namespace ThAmCo.Repo.Models
{
    public class UserGetModel
    {
        public string Id { get; set; }
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}
