using Microlending.Shared.Models;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace microlending_API.Features.Borrowers;

// Database access done here, we will implement the IBorrowerService interface in this class.
public class BorrowerService : IBorrowerService
{
    private readonly AppDbContext _context;

    // We inject the AppDbContext into the service to perform database operations.
    public BorrowerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BorrowersPagedResponse> GetBorrowersAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Borrowers.AsNoTracking().Where(b => b.IsDeleted != true);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(b => b.FullName)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken);

        return new BorrowersPagedResponse
        {
            Items = items,
            Pagination = new PaginationMetadata { TotalCount = total, PageSize = pageSize, CurrentPage = page }
        };
    }

    public async Task<Borrower?> GetBorrowersByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Borrowers.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);
    }

    public async Task<Borrower> CreateBorrowersAsync(Borrower model, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        model.CreatedAt = now;
        model.UpdatedAt = now;
        model.IsDeleted = false;

        _context.Borrowers.Add(model);
        await _context.SaveChangesAsync(ct);
        return model;
    }

    public async Task<Borrower?> UpdateBorrowersAsync(int id, Borrower model, CancellationToken ct)
    {
        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        if (entity is null) return null;

        entity.FullName = model.FullName;
        entity.UserName = model.UserName;
        entity.Nrcno = model.Nrcno;
        entity.PhoneNo = model.PhoneNo;
        entity.Address = model.Address;
        entity.DocumentId = model.DocumentId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteBorrowersAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, ct);

        if (entity is null) return false;

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public Task<Borrower?> GetByIdAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Borrower> CreateAsync(Borrower borrower, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Borrower?> UpdateAsync(int id, Borrower borrower, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}