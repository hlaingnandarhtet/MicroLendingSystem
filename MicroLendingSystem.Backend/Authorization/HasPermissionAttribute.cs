using Microsoft.AspNetCore.Mvc;

namespace MicroLendingSystem.Backend.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HasPermissionAttribute : TypeFilterAttribute
{
    public HasPermissionAttribute(string permissionName)
        : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { permissionName };
    }
}
