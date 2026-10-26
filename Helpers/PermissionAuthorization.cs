using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
namespace ShopMvc.Helpers;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string code) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) { context.Result = new ChallengeResult(); return; }
        var db = context.HttpContext.RequestServices.GetRequiredService<ShopDbContext>();
        var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var roleIds = db.UserRoles.Where(x => x.UserId == userId).Select(x => x.RoleId);
        var isAdmin = userId is not null && await db.UserRoles.AnyAsync(x => x.UserId == userId && db.Roles.Any(r => r.Id == x.RoleId && r.Name == "Admin"));
        var allowed = isAdmin || await db.RoleClaims.AnyAsync(x => roleIds.Contains(x.RoleId) && x.ClaimType == "Permission" && x.ClaimValue == code);
        if (!allowed) context.Result = new ForbidResult();
    }
}
public static class PermissionViewExtensions
{
    public static async Task<bool> HasPermissionAsync(this HttpContext http, string code)
    {
        var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return false;
        var db = http.RequestServices.GetRequiredService<ShopDbContext>();
        var roleIds = db.UserRoles.Where(x => x.UserId == userId).Select(x => x.RoleId);
        return await db.RoleClaims.AnyAsync(x => roleIds.Contains(x.RoleId) && x.ClaimType == "Permission" && x.ClaimValue == code);
    }
}
