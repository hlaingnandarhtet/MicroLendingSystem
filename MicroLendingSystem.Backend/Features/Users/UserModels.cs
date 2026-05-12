namespace MicroLendingSystem.Backend.Features.Users;

public sealed class UserDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public int RoleId { get; init; }

    public string RoleName { get; init; } = string.Empty;

    public DateTime? CreatedAt { get; init; }
}

public sealed class BootstrapAdminRequest
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public sealed class CreateUserRequest
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    /// <summary>Optional for future use; callers must supply <see cref="RoleId"/> for standard creation flows.</summary>
    public int? RoleId { get; init; }
}

public sealed class UpdateUserRequest
{
    public string? Name { get; init; }

    public string? Email { get; init; }

    public string? Password { get; init; }
}

public sealed class AssignUserRoleRequest
{
    public int RoleId { get; init; }
}

public sealed class LoginRequest
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public sealed class LoginDto
{
    public string Token { get; init; } = string.Empty;

    public int ExpiresInMinutes { get; init; }

    public UserDto User { get; init; } = null!;
}
