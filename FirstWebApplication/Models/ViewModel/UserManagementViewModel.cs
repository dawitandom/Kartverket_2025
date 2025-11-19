using System.Collections.Generic;

namespace FirstWebApplication.Models;

public class UserManagementViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> Organizations { get; set; } = new List<string>();
}