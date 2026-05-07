using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace microlending_API.Features.Borrowers;

public class BorrowerService : IBorrowerService
{
    private readonly AppDbContext _context;

    public BorrowerService(AppDbContext context) => _context = context;

    public async Task<PagedResult<PagedPayload<BorrowerDto>>> GetBorrowersAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize < 1)
        {
            return PagedResult<PagedPayload<BorrowerDto>>.Failure("Invalid pagination parameters.", 400);
        }

        var query = _context.Borrowers.AsNoTracking().Where(b => b.IsDeleted != true);
        var total = await query.CountAsync(cancellationToken);
        var entities = await query.OrderBy(b => b.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var payload = new PagedPayload<BorrowerDto>
        {
            Items = entities.Select(MapToDto).ToList(),
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize
        };

        return PagedResult<PagedPayload<BorrowerDto>>.Success(payload);
    }

    public async Task<Result<BorrowerDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Borrowers.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        return entity is null
            ? Result<BorrowerDto>.Failure("Borrower not found.", 404)
            : Result<BorrowerDto>.Success(MapToDto(entity));
    }

    public async Task<Result<BorrowerDto>> CreateAsync(CreateBorrowerRequest request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var entity = new Borrower
        {
            FullName = request.FullName,
            UserName = request.UserName,
            Nrcno = request.Nrcno,
            PhoneNo = request.PhoneNo,
            Address = request.Address,
            DocumentId = request.DocumentId,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        _context.Borrowers.Add(entity);
        await _context.SaveChangesAsync(ct);

        return Result<BorrowerDto>.Success(MapToDto(entity));
    }

    public async Task<Result<BorrowerDto>> UpdateAsync(int id, UpdateBorrowerRequest request, CancellationToken ct)
    {
        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        if (entity is null)
        {
            return Result<BorrowerDto>.Failure("Borrower not found.", 404);
        }

        entity.FullName = request.FullName;
        entity.UserName = request.UserName;
        entity.Nrcno = request.Nrcno;
        entity.PhoneNo = request.PhoneNo;
        entity.Address = request.Address;
        entity.DocumentId = request.DocumentId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Result<BorrowerDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        if (entity is null)
        {
            return Result<bool>.Failure("Borrower not found.", 404);
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private static BorrowerDto MapToDto(Borrower b) =>
        new()
        {
            Id = b.Id,
            FullName = b.FullName,
            UserName = b.UserName,
            Nrcno = b.Nrcno,
            PhoneNo = b.PhoneNo,
            Address = b.Address,
            //DocumentId = b.DocumentId,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };
}
