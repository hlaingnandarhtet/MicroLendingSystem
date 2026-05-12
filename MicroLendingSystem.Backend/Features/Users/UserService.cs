using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MicroLendingSystem.Backend.Authorization;
using MicroLendingSystem.Backend.Features;
using MicroLendingSystem.Backend.Infrastructure;
using MicroLendingSystem.Backend.Options;
    
namespace MicroLendingSystem.Backend.Features.Users;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokens;
    private readonly JwtSettings _jwt;

    public UserService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokens,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokens = jwtTokens;
        _jwt = jwtSettings.Value;
    }

    public async Task<Result<LoginDto>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && (u.IsDeleted != true), ct);

        if (user is null)
        {
            return Result<LoginDto>.Failure("Invalid email or password.", 401);
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return Result<LoginDto>.Failure("Invalid email or password.", 401);
        }

        var dto = MapToDto(user);
        
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync(ct);

        var token = _jwtTokens.CreateAccessToken(user, permissions);

        var payload = new LoginDto
        {
            Token = token,
            ExpiresInMinutes = _jwt.AccessTokenExpirationMinutes,
            User = dto
        };

        return Result<LoginDto>.Success(payload);
    }

    public async Task<Result<UserDto>> BootstrapAdminAsync(BootstrapAdminRequest request, CancellationToken ct)
    {
        var exists = await _context.Users.AsNoTracking().AnyAsync(u => (u.IsDeleted != true), ct);
        if (exists)
        {
            return Result<UserDto>.Failure("Bootstrap is only allowed before any active user exists.", 400);
        }

        var adminRole = await _context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == PermissionNames.AdminRoleName, ct);
        if (adminRole is null)
        {
            return Result<UserDto>.Failure("Admin role is not seeded. Apply database migrations.", 503);
        }

        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email, ct))
        {
            return Result<UserDto>.Failure("Email is already in use.", 409);
        }

        var entity = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            RoleId = adminRole.Id,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = string.Empty,
            IsDeleted = false
        };
        entity.PasswordHash = _passwordHasher.HashPassword(entity, request.Password);

        _context.Users.Add(entity);
        await _context.SaveChangesAsync(ct);

        var loaded = await _context.Users.Include(u => u.Role)
            .AsNoTracking()
            .FirstAsync(u => u.Id == entity.Id, ct);

        return Result<UserDto>.Success(MapToDto(loaded));
    }

    public async Task<PagedResult<PagedPayload<UserDto>>> GetUsersAsync(int page, int pageSize, string? name, string? email, string? role, CancellationToken ct)
    {
        if (page < 1 || pageSize < 1)
        {
            return PagedResult<PagedPayload<UserDto>>.Failure("Invalid pagination parameters.", 400);
        }

        //var query = _context.Users.AsNoTracking().Where(u => (u.IsDeleted != true));
        var query = _context.Users
        .Include(u => u.Role)
        .AsNoTracking()
        .Where(u => u.IsDeleted != true);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(u => u.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.Contains(email));
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role.Name.Contains(role));

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => MapToDtoProjection(u))
            .ToListAsync(ct);

        var payload = new PagedPayload<UserDto>
        {
            Items = items,
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize
        };

        return PagedResult<PagedPayload<UserDto>>.Success(payload);
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var dto = await _context.Users.AsNoTracking()
            .Where(u => u.Id == id && (u.IsDeleted != true))
            .Select(u => MapToDtoProjection(u))
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? Result<UserDto>.Failure("User not found.", 404)
            : Result<UserDto>.Success(dto);
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email, ct))
        {
            return Result<UserDto>.Failure("Email is already in use.", 409);
        }

        if (request.RoleId is null)
        {
            return Result<UserDto>.Failure("RoleId is required.", 400);
        }

        if (!await _context.Roles.AsNoTracking().AnyAsync(r => r.Id == request.RoleId.Value, ct))
        {
            return Result<UserDto>.Failure("RoleId must reference an existing role.", 400);
        }

        var entity = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            RoleId = request.RoleId.Value,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = string.Empty,
            IsDeleted = false
        };

        entity.PasswordHash = _passwordHasher.HashPassword(entity, request.Password);
        _context.Users.Add(entity);
        await _context.SaveChangesAsync(ct);

        var loaded = await _context.Users.AsNoTracking()
            .Where(u => u.Id == entity.Id)
            .Select(u => MapToDtoProjection(u))
            .FirstAsync(ct);

        return Result<UserDto>.Success(loaded);
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && (u.IsDeleted != true), ct);
        if (entity is null)
        {
            return Result<UserDto>.Failure("User not found.", 404);
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim();
            var emailTaken = await _context.Users.IgnoreQueryFilters()
                .AnyAsync(u => u.Email == email && u.Id != id, ct);
            if (emailTaken)
            {
                return Result<UserDto>.Failure("Email is already in use.", 409);
            }

            entity.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            entity.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            entity.PasswordHash = _passwordHasher.HashPassword(entity, request.Password);
        }

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var dto = await _context.Users.AsNoTracking()
            .Where(u => u.Id == entity.Id)
            .Select(u => MapToDtoProjection(u))
            .FirstAsync(ct);

        return Result<UserDto>.Success(dto);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && (u.IsDeleted != true), ct);
        if (entity is null)
        {
            return Result<bool>.Failure("User not found.", 404);
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<UserDto>> AssignRoleAsync(int id, AssignUserRoleRequest request, CancellationToken ct)
    {
        var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && (u.IsDeleted != true), ct);
        if (entity is null)
        {
            return Result<UserDto>.Failure("User not found.", 404);
        }

        var roleExists = await _context.Roles.AsNoTracking().AnyAsync(r => r.Id == request.RoleId, ct);
        if (!roleExists)
        {
            return Result<UserDto>.Failure("Role does not exist.", 400);
        }

        entity.RoleId = request.RoleId;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var dto = await _context.Users.AsNoTracking()
            .Where(u => u.Id == entity.Id)
            .Select(u => MapToDtoProjection(u))
            .FirstAsync(ct);

        return Result<UserDto>.Success(dto);
    }

    private static UserDto MapToDto(User u) =>
        new()
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            RoleId = u.RoleId,
            RoleName = u.Role.Name,
            CreatedAt = u.CreatedAt
        };

    private static UserDto MapToDtoProjection(User u) =>
        new()
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            RoleId = u.RoleId,
            RoleName = u.Role?.Name ?? "N/A",
            CreatedAt = u.CreatedAt
        };
}
