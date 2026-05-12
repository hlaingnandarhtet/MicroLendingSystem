using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroLendingSystem.Backend.Features.Permissions;

public sealed class PermissionService(AppDbContext context) : IPermissionService
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResult<PagedPayload<PermissionDto>>> GetPermissionsAsync(int page, int pageSize, CancellationToken ct)
    {
        if (page < 1 || pageSize < 1)
        {
            return PagedResult<PagedPayload<PermissionDto>>.Failure("Invalid pagination parameters.", 400);
        }

        var query = _context.Permissions.AsNoTracking().OrderBy(p => p.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PermissionDto { Id = p.Id, Name = p.Name })
            .ToListAsync(ct);

        var payload = new PagedPayload<PermissionDto>
        {
            Items = items,
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize
        };

        return PagedResult<PagedPayload<PermissionDto>>.Success(payload);
    }

    public async Task<Result<PermissionDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var dto = await _context.Permissions.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PermissionDto { Id = p.Id, Name = p.Name })
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<PermissionDto>.Failure("Permission not found.", 404) : Result<PermissionDto>.Success(dto);
    }

    public async Task<Result<PermissionDto>> CreateAsync(CreatePermissionRequest request, CancellationToken ct)
    {
        var trimmed = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Result<PermissionDto>.Failure("Permission name is required.", 400);
        }

        if (await _context.Permissions.AnyAsync(p => p.Name == trimmed, ct))
        {
            return Result<PermissionDto>.Failure("A permission with this name already exists.", 409);
        }

        var entity = new Permission { Name = trimmed };
        _context.Permissions.Add(entity);
        await _context.SaveChangesAsync(ct);

        return Result<PermissionDto>.Success(new PermissionDto { Id = entity.Id, Name = entity.Name });
    }

    public async Task<Result<PermissionDto>> UpdateAsync(int id, UpdatePermissionRequest request, CancellationToken ct)
    {
        var entity = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null)
        {
            return Result<PermissionDto>.Failure("Permission not found.", 404);
        }

        var trimmed = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Result<PermissionDto>.Failure("Permission name is required.", 400);
        }

        if (await _context.Permissions.AnyAsync(p => p.Name == trimmed && p.Id != id, ct))
        {
            return Result<PermissionDto>.Failure("A permission with this name already exists.", 409);
        }

        entity.Name = trimmed;
        await _context.SaveChangesAsync(ct);

        return Result<PermissionDto>.Success(new PermissionDto { Id = entity.Id, Name = entity.Name });
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is null)
        {
            return Result<bool>.Failure("Permission not found.", 404);
        }

        _context.Permissions.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
