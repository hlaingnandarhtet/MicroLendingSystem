using MicroLendingSystem.Shared.Models;

namespace microlending_API.Features.Transactions;

public interface ITransactionService
{
    Task<PagedResult<PagedPayload<TransactionListRowDto>>> GetTransactionsAsync(GetTransactionsRequest request, CancellationToken ct);
}
