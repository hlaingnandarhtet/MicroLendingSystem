using System.Security.Claims;
using MicroLendingSystem.Database.AppDbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace MicroLendingSystem.Backend.Authorization;

public sealed class PermissionAuthorizationFilter(string requiredPermission, AppDbContext db) : IAsyncActionFilter
{
    private readonly AppDbContext _db = db;
    private readonly string _requiredPermission = requiredPermission;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var endpoint = context.ActionDescriptor.EndpointMetadata;
        if (endpoint.OfType<AllowAnonymousAttribute>().Any())
        {
            await next();
            return;
        }

        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var sub =
            context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.HttpContext.User.FindFirstValue("sub");

        if (!int.TryParse(sub, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var slice = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId && (u.IsDeleted != true))
            .Select(u => new { RoleName = u.Role.Name, Perms = u.Role.RolePermissions.Select(rp => rp.Permission.Name).ToList() })
            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

        if (slice is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (string.Equals(slice.RoleName, PermissionNames.AdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        var allowed = slice.Perms.Exists(p =>
            string.Equals(p, _requiredPermission, StringComparison.Ordinal));

        if (!allowed)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        await next();
    }
}
