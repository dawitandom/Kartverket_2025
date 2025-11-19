using System.Collections.Generic;

namespace FirstWebApplication.Models
{
    public class MyProfileViewModel
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = new();
        public List<string> Organizations { get; set; } = new();
    }
}