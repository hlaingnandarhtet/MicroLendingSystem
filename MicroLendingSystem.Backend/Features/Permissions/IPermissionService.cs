using MicroLendingSystem.Shared.Models;

namespace MicroLendingSystem.Backend.Features.Permissions;

public interface IPermissionService
{
    Task<PagedResult<PagedPayload<PermissionDto>>> GetPermissionsAsync(int page, int pageSize, CancellationToken ct);

    Task<Result<PermissionDto>> GetByIdAsync(int id, CancellationToken ct);

    Task<Result<PermissionDto>> CreateAsync(CreatePermissionRequest request, CancellationToken ct);

    Task<Result<PermissionDto>> UpdateAsync(int id, UpdatePermissionRequest request, CancellationToken ct);

    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);
}
