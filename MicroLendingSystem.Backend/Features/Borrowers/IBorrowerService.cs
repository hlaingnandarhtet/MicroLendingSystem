using MicroLendingSystem.Shared.Models;

namespace microlending_API.Features.Borrowers;

public interface IBorrowerService
{
    Task<PagedResult<PagedPayload<BorrowerDto>>> GetBorrowersAsync(int page, int pageSize, string? fullName, string? userName, string? phoneNo, string? nrcNo, CancellationToken cancellationToken);
    Task<Result<BorrowerDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<BorrowerDto>> CreateAsync(CreateBorrowerRequest request, CancellationToken ct);
    Task<Result<BorrowerDto>> UpdateAsync(int id, UpdateBorrowerRequest request, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);
}
