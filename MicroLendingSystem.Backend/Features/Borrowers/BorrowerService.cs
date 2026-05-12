using MicroLendingSystem.Backend.Infrastructure;
using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace microlending_API.Features.Borrowers;

public class BorrowerService(AppDbContext context, ICurrentUserAccessor currentUser, IPasswordHasher<User> passwordHasher) : IBorrowerService
{
    private readonly AppDbContext _context = context;
    private readonly ICurrentUserAccessor _currentUser = currentUser;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private const string BorrowerRoleName = "Borrower";

    //public async Task<PagedResult<PagedPayload<BorrowerDto>>> GetBorrowersAsync(int page, int pageSize, string? fullName, string? userName, string? phoneNo, string? nrcNo, CancellationToken cancellationToken)
    //{
    //    if (page < 1 || pageSize < 1)
    //        return PagedResult<PagedPayload<BorrowerDto>>.Failure("Invalid pagination parameters.", 400);

    //    var query = _context.Borrowers.AsNoTracking().Where(b => b.IsDeleted != true);

    //    if (!string.IsNullOrWhiteSpace(fullName))
    //        query = query.Where(b => b.FullName.Contains(fullName));
    //    if (!string.IsNullOrWhiteSpace(userName))
    //        query = query.Where(b => b.UserName != null && b.UserName.Contains(userName));
    //    if (!string.IsNullOrWhiteSpace(phoneNo))
    //        query = query.Where(b => b.PhoneNo != null && b.PhoneNo.Contains(phoneNo));
    //    if (!string.IsNullOrWhiteSpace(nrcNo))
    //        query = query.Where(b => b.Nrcno != null && b.Nrcno.Contains(nrcNo));

    //    if (!_currentUser.IsAdmin)
    //    {
    //        if (!_currentUser.TryGetUserId(out var loggedId))
    //        {
    //            return PagedResult<PagedPayload<BorrowerDto>>.Success(new PagedPayload<BorrowerDto>
    //            {
    //                Items = new List<BorrowerDto>(),
    //                TotalCount = 0,
    //                CurrentPage = page,
    //                PageSize = pageSize
    //            });
    //        }

    //        if (_currentUser.IsBorrower)
    //            query = query.Where(b => b.UserId == loggedId);
    //        else
    //            query = query.Where(b => b.CreatedById == loggedId);
    //    }

    //    var total = await query.CountAsync(cancellationToken);
    //    var entities = await query.OrderByDescending(b => b.Id) 
    //    .Skip((page - 1) * pageSize)
    //    .Take(pageSize)
    //    .ToListAsync(cancellationToken);

    //    return PagedResult<PagedPayload<BorrowerDto>>.Success(new PagedPayload<BorrowerDto>
    //    {
    //        Items = entities.Select(MapToDto).ToList(),
    //        TotalCount = total,
    //        CurrentPage = page,
    //        PageSize = pageSize
    //    });
    //}

    public async Task<PagedResult<PagedPayload<BorrowerDto>>> GetBorrowersAsync(int page, int pageSize, string? fullName, string? userName, string? phoneNo, string? nrcNo, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize < 1)
            return PagedResult<PagedPayload<BorrowerDto>>.Failure("Invalid pagination parameters.", 400);
        try
        {
            var query = _context.Borrowers.AsNoTracking().Where(b => b.IsDeleted != true);

            if (!string.IsNullOrWhiteSpace(fullName))
                query = query.Where(b => b.FullName.Contains(fullName));
            if (!string.IsNullOrWhiteSpace(userName))
                query = query.Where(b => b.UserName != null && b.UserName.Contains(userName));
            if (!string.IsNullOrWhiteSpace(phoneNo))
                query = query.Where(b => b.PhoneNo != null && b.PhoneNo.Contains(phoneNo));
            if (!string.IsNullOrWhiteSpace(nrcNo))
                query = query.Where(b => b.Nrcno != null && b.Nrcno.Contains(nrcNo));

            if (_currentUser != null && !_currentUser.IsAdmin)
            {
                if (!_currentUser.TryGetUserId(out var loggedId))
                {
                    return PagedResult<PagedPayload<BorrowerDto>>.Success(new PagedPayload<BorrowerDto>
                    {
                        Items = new List<BorrowerDto>(),
                        TotalCount = 0,
                        CurrentPage = page,
                        PageSize = pageSize
                    });
                }

                if (_currentUser.IsBorrower)
                    query = query.Where(b => b.UserId == loggedId);
                else
                    query = query.Where(b => b.CreatedById == loggedId);
            }

            var total = await query.CountAsync(cancellationToken);
            var entities = await query.OrderByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult<PagedPayload<BorrowerDto>>.Success(new PagedPayload<BorrowerDto>
            {
                Items = entities != null ? entities.Select(MapToDto).ToList() : new List<BorrowerDto>(),
                TotalCount = total,
                CurrentPage = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return PagedResult<PagedPayload<BorrowerDto>>.Success(new PagedPayload<BorrowerDto>
            {
                Items = new List<BorrowerDto>(),
                TotalCount = 0,
                CurrentPage = page,
                PageSize = pageSize
            });
        }
    }
    public async Task<Result<BorrowerDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Borrowers.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        if (entity is null)
            return Result<BorrowerDto>.Failure("Borrower not found.", 404);
        if (!CanAccessBorrower(entity))
            return Result<BorrowerDto>.Failure("Borrower not found.", 404);

        return Result<BorrowerDto>.Success(MapToDto(entity));
    }

    public async Task<Result<BorrowerDto>> CreateAsync(CreateBorrowerRequest request, CancellationToken ct)
    {
        if (!_currentUser.TryGetUserId(out var creatorId))
            return Result<BorrowerDto>.Failure("Unauthorized.", 401);

        var creatorExists = await _context.Users.AsNoTracking()
            .AnyAsync(u => u.Id == creatorId && u.IsDeleted != true, ct);
        if (!creatorExists)
            return Result<BorrowerDto>.Failure("Your user account was not found. Cannot create borrower.", 403);

        int? documentId = request.DocumentId is > 0 ? request.DocumentId : null;
        if (documentId is int did)
        {
            var docOk = await _context.Documents.AsNoTracking()
                .AnyAsync(d => d.Id == did && d.IsDeleted != true, ct);
            if (!docOk)
                return Result<BorrowerDto>.Failure("Invalid DocumentId: no matching document.", 400);
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                if (request.CreateLoginAccount)
                {
                    if (string.IsNullOrWhiteSpace(request.LoginEmail) || string.IsNullOrWhiteSpace(request.LoginPassword))
                    {
                        await tx.RollbackAsync(ct);
                        return Result<BorrowerDto>.Failure("Email and password are required when creating a login account.", 400);
                    }

                    var role = await _context.Roles.AsNoTracking()
                        .FirstOrDefaultAsync(r => r.Name == BorrowerRoleName, ct);
                    if (role is null)
                    {
                        await tx.RollbackAsync(ct);
                        return Result<BorrowerDto>.Failure("Borrower role is not seeded.", 503);
                    }

                    var roleStillExists = await _context.Roles.AsNoTracking()
                        .AnyAsync(r => r.Id == role.Id, ct);
                    if (!roleStillExists)
                    {
                        await tx.RollbackAsync(ct);
                        return Result<BorrowerDto>.Failure("Borrower role is invalid.", 400);
                    }

                    var email = request.LoginEmail.Trim();
                    if (await _context.Users.AnyAsync(u => u.Email == email && u.IsDeleted != true, ct))
                    {
                        await tx.RollbackAsync(ct);
                        return Result<BorrowerDto>.Failure("Email is already in use.", 409);
                    }

                    var now = DateTime.UtcNow;
                    var user = new User
                    {
                        Name = request.FullName.Trim(),
                        Email = email,
                        PasswordHash = string.Empty,
                        RoleId = role.Id,
                        CreatedAt = now,
                        UpdatedAt = now,
                        IsDeleted = false
                    };
                    user.PasswordHash = _passwordHasher.HashPassword(user, request.LoginPassword!);
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(ct);

                    var newUserId = user.Id;
                    if (newUserId <= 0 || !await _context.Users.AsNoTracking()
                            .AnyAsync(u => u.Id == newUserId && u.IsDeleted != true, ct))
                    {
                        await tx.RollbackAsync(ct);
                        return Result<BorrowerDto>.Failure("Failed to create user account.", 500);
                    }

                    var borrower = new Borrower
                    {
                        FullName = request.FullName,
                        UserName = request.UserName,
                        Nrcno = request.Nrcno,
                        PhoneNo = request.PhoneNo,
                        Address = request.Address,
                        DocumentId = documentId,
                        UserId = newUserId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        IsDeleted = false,
                        CreatedById = creatorId
                    };
                    _context.Borrowers.Add(borrower);
                    await _context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return Result<BorrowerDto>.Success(MapToDto(borrower));
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var entity = new Borrower
                    {
                        FullName = request.FullName,
                        UserName = request.UserName,
                        Nrcno = request.Nrcno,
                        PhoneNo = request.PhoneNo,
                        Address = request.Address,
                        DocumentId = documentId,
                        UserId = null,
                        CreatedAt = now,
                        UpdatedAt = now,
                        IsDeleted = false,
                        CreatedById = creatorId
                    };
                    _context.Borrowers.Add(entity);
                    await _context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return Result<BorrowerDto>.Success(MapToDto(entity));
                }
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync(ct);
                var innerError = ex.InnerException?.Message ?? ex.Message;
                return Result<BorrowerDto>.Failure(
                    $"Save failed: {innerError}. Verify database schema and IDs.", 400);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task<Result<BorrowerDto>> UpdateAsync(int id, UpdateBorrowerRequest request, CancellationToken ct)
    {
        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        if (entity is null)
            return Result<BorrowerDto>.Failure("Borrower not found.", 404);
        if (!CanAccessBorrower(entity))
            return Result<BorrowerDto>.Failure("You can only update borrowers you created.", 403);

        entity.FullName = request.FullName;
        entity.UserName = request.UserName;
        entity.Nrcno = request.Nrcno;
        entity.PhoneNo = request.PhoneNo;
        entity.Address = request.Address;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<BorrowerDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        if (entity is null)
            return Result<bool>.Failure("Borrower not found.", 404);
        if (!CanAccessBorrower(entity))
            return Result<bool>.Failure("You can only delete borrowers you created.", 403);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private bool CanAccessBorrower(Borrower b)
    {
        if (_currentUser.IsAdmin) return true;
        if (_currentUser.IsBorrower && _currentUser.TryGetUserId(out var buid))
            return b.UserId == buid;
        return _currentUser.TryGetUserId(out var uid) && b.CreatedById == uid;
    }

    private static BorrowerDto MapToDto(Borrower b) => new()
    {
        Id = b.Id,
        FullName = b.FullName,
        UserName = b.UserName ?? string.Empty,
        Nrcno = b.Nrcno ?? string.Empty,
        PhoneNo = b.PhoneNo ?? string.Empty,
        Address = b.Address ?? "N/A",
        //DocumentId = b.DocumentId ?? 0,
        CreatedById = b.CreatedById,
        UserId = b.UserId,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}