using MicroLendingSystem.Database.Models;

namespace microlending_API.Features.Loans
{

    public interface ILoanService
    {
        Task<Result<LoanDto>> CreateLoansAsync(CreateLoanRequest request, CancellationToken ct);
        Task<Result<bool>> ApproveLoansAsync(int id, CancellationToken ct);
        Task<object> GetLoansPagedAsync(GetLoansRequest request, CancellationToken ct);
        Task<LoanDetails?> GetLoansByIdAsync(int id, CancellationToken ct);
        Task<Loan?> UpdateLoansStatusAsync(int id, string newStatus, CancellationToken ct);
        Task<bool> DeleteLoansAsync(int id, CancellationToken ct);
        Task<Result<Loan?>> UpdateLoansAsync(int id, UpdateLoanDataRequest request, CancellationToken ct);

    }

}
