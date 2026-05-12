namespace microlending_API.Features.Transactions;

public sealed class GetTransactionsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public string? LoanCode { get; set; }
    public string? PlanName { get; set; }
    public string? BorrowerName { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>Row for the transaction list grid (Date, Borrower, Loan amount, Paid, Status).</summary>
public sealed class TransactionListRowDto
{
    public int Id { get; init; }
    public int LoanId { get; init; }

    public string LoanCode { get; init; } = string.Empty;

    public DateTime TransactionDate { get; init; }
    public string BorrowerName { get; init; } = string.Empty;
    public decimal LoanAmount { get; init; }
    public string LoanPlanName { get; init; } = string.Empty;

    public decimal Paid { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal? TotalRepayableAmount { get; init; }
    public decimal? RemainingBalance { get; init; }
}
