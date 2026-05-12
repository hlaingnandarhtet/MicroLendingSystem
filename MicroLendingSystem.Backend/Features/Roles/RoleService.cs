using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MicroLendingSystem.Backend.Authorization;

namespace MicroLendingSystem.Backend.Features.Roles;

public sealed class RoleService(AppDbContext context) : IRoleService
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResult<PagedPayload<RoleSummaryDto>>> GetRolesAsync(int page, int pageSize, CancellationToken ct)
    {
        if (page < 1 || pageSize < 1)
        {
            return PagedResult<PagedPayload<RoleSummaryDto>>.Failure("Invalid pagination parameters.", 400);
        }

        var query = _context.Roles.AsNoTracking().OrderBy(r => r.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new RoleSummaryDto { Id = r.Id, Name = r.Name })
            .ToListAsync(ct);

        var payload = new PagedPayload<RoleSummaryDto>
        {
            Items = items,
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize
        };

        return PagedResult<PagedPayload<RoleSummaryDto>>.Success(payload);
    }

    public async Task<Result<RoleDetailDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var dto = await _context.Roles.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RoleDetailDto
            {
                Id = r.Id,
                Name = r.Name,
                Permissions = r.RolePermissions
                    .OrderBy(rp => rp.Permission.Name)
                    .Select(rp => rp.Permission.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<RoleDetailDto>.Failure("Role not found.", 404) : Result<RoleDetailDto>.Success(dto);
    }

    public async Task<Result<RoleDetailDto>> CreateAsync(CreateRoleRequest request, CancellationToken ct)
    {
        var trimmed = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Result<RoleDetailDto>.Failure("Role name is required.", 400);
        }

        if (await _context.Roles.AnyAsync(r => r.Name == trimmed, ct))
        {
            return Result<RoleDetailDto>.Failure("A role with this name already exists.", 409);
        }

        var entity = new Role { Name = trimmed };
        _context.Roles.Add(entity);
        await _context.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<Result<RoleDetailDto>> UpdateAsync(int id, UpdateRoleRequest request, CancellationToken ct)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null)
        {
            return Result<RoleDetailDto>.Failure("Role not found.", 404);
        }

        var trimmed = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Result<RoleDetailDto>.Failure("Role name is required.", 400);
        }

        if (await _context.Roles.AnyAsync(r => r.Name == trimmed && r.Id != id, ct))
        {
            return Result<RoleDetailDto>.Failure("A role with this name already exists.", 409);
        }

        entity.Name = trimmed;
        await _context.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entity is null)
        {
            return Result<bool>.Failure("Role not found.", 404);
        }

        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.RoleId == id && !(u.IsDeleted ?? false), ct))
        {
            return Result<bool>.Failure("Cannot delete a role assigned to active users.", 409);
        }

        if (string.Equals(entity.Name, PermissionNames.AdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure("The system Admin role cannot be deleted.", 400);
        }

        _context.Roles.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<RoleDetailDto>> AssignPermissionsAsync(int id, AssignPermissionsToRoleRequest request, CancellationToken ct)
    {
        if (!await _context.Roles.AsNoTracking().AnyAsync(r => r.Id == id, ct))
        {
            return Result<RoleDetailDto>.Failure("Role not found.", 404);
        }

        var ids = request.PermissionIds.Distinct().ToArray();

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        await _context.RolePermissions.Where(rp => rp.RoleId == id).ExecuteDeleteAsync(ct);

        if (ids.Length > 0)
        {
            var found = await _context.Permissions.AsNoTracking().Where(p => ids.Contains(p.Id)).CountAsync(ct);
            if (found != ids.Length)
            {
                await transaction.RollbackAsync(ct);
                return Result<RoleDetailDto>.Failure("One or more permission ids were not found.", 400);
            }

            foreach (var pid in ids)
            {
                _context.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = pid });
            }

            await _context.SaveChangesAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return await GetByIdAsync(id, ct);
    }
}
