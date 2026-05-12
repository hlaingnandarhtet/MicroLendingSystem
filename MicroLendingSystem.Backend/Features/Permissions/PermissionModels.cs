namespace MicroLendingSystem.Backend.Features.Permissions;

public sealed class PermissionDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class CreatePermissionRequest
{
    public string Name { get; init; } = string.Empty;
}

public sealed class UpdatePermissionRequest
{
    public string Name { get; init; } = string.Empty;
}
