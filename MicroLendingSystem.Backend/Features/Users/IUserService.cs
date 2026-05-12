using MicroLendingSystem.Shared.Models;

namespace MicroLendingSystem.Backend.Features.Users;

public interface IUserService
{
    Task<Result<LoginDto>> LoginAsync(LoginRequest request, CancellationToken ct);

    Task<Result<UserDto>> BootstrapAdminAsync(BootstrapAdminRequest request, CancellationToken ct);

    Task<PagedResult<PagedPayload<UserDto>>> GetUsersAsync(int page, int pageSize, string? name, string? email, string? role, CancellationToken ct);

    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct);

    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct);

    Task<Result<UserDto>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct);

    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);

    Task<Result<UserDto>> AssignRoleAsync(int id, AssignUserRoleRequest request, CancellationToken ct);
}
