using System.Collections.Generic;

namespace FirstWebApplication.Models;

public class OrgMembersViewModel
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public List<OrgMemberDto> Members { get; set; } = new();
}

public class OrgMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
}