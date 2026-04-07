using LoyaltyCRM.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace LoyaltyCRM.Authorization
{
    public class RequireRoleAttribute : AuthorizeAttribute
    {
        public RequireRoleAttribute(params Role[] roles)
        {
            Roles = string.Join(",", roles.Select(r => r.ToString()));
        }
    }
}