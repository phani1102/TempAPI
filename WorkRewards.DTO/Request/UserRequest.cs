using System;
using System.Collections.Generic;
using System.Text;

namespace WorkRewards.DTO.Request
{
    public class UserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public int RoleId { get; set; }
    }
}
