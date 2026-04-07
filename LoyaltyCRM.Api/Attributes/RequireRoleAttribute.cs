using Microsoft.AspNetCore.Authorization;
using PapasCRM_API.Enums;

namespace PapasCRM_API.Authorization
{
    public class RequireRoleAttribute : AuthorizeAttribute
    {
        public RequireRoleAttribute(params Role[] roles)
        {
            Roles = string.Join(",", roles.Select(r => r.ToString()));
        }
    }
}