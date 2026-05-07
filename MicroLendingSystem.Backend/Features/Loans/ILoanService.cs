using microlending_API.Features.LoanSettings;
using MicroLendingSystem.Shared.Models;

namespace microlending_API.Features.Loans;

public interface ILoanService
{
    Task<Result<LoanDto>> CreateLoanAsync(CreateLoanRequest request, CancellationToken ct);
    Task<Result<bool>> ApproveRejectLoanAsync(int id, int statusId, CancellationToken ct);
    Task<PagedResult<PagedPayload<LoanDto>>> GetLoanAsync(GetLoansRequest request, CancellationToken ct);
    Task<Result<LoanDto>> GetLoanByIdAsync(int id, CancellationToken ct);
    Task<Result<LoanDto>> UpdateLoanStatusAsync(int id, UpdateLoanStatusRequest request, CancellationToken ct);
    Task<Result<bool>> DeleteLoanAsync(int id, CancellationToken ct);
    Task<Result<LoanDto>> UpdateLoanAsync(int id, UpdateLoanDataRequest request, CancellationToken ct);
}
