using MicroLendingSystem.Database.Models;

namespace microlending_API.Features.Borrowers;

// what we will do is to define the service interface for borrowers,
// which will be implemented by the BorrowerService class. This interface will contain methods for CRUD operations and pagination.
public interface IBorrowerService
{
    Task<BorrowersPagedResponse> GetBorrowersAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Borrower?> GetByIdAsync(int id, CancellationToken ct);
    Task<Borrower> CreateAsync(Borrower borrower, CancellationToken ct);
    Task<Borrower?> UpdateAsync(int id, Borrower borrower, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}