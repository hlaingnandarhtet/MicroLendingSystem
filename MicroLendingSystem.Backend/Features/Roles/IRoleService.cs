using MicroLendingSystem.Shared.Models;

namespace MicroLendingSystem.Backend.Features.Roles;

public interface IRoleService
{
    Task<PagedResult<PagedPayload<RoleSummaryDto>>> GetRolesAsync(int page, int pageSize, CancellationToken ct);

    Task<Result<RoleDetailDto>> GetByIdAsync(int id, CancellationToken ct);

    Task<Result<RoleDetailDto>> CreateAsync(CreateRoleRequest request, CancellationToken ct);

    Task<Result<RoleDetailDto>> UpdateAsync(int id, UpdateRoleRequest request, CancellationToken ct);

    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);

    Task<Result<RoleDetailDto>> AssignPermissionsAsync(int id, AssignPermissionsToRoleRequest request, CancellationToken ct);
}
