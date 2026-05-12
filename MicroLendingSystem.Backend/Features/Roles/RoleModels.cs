using MicroLendingSystem.Shared.Models;

namespace MicroLendingSystem.Backend.Features.Roles;

public sealed class RoleSummaryDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class RoleDetailDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
}

public sealed class CreateRoleRequest
{
    public string Name { get; init; } = string.Empty;
}

public sealed class UpdateRoleRequest
{
    public string Name { get; init; } = string.Empty;
}

public sealed class AssignPermissionsToRoleRequest
{
    public IReadOnlyList<int> PermissionIds { get; init; } = Array.Empty<int>();
}
