using microlending_API.Features.Borrowers;
using microlending_API.Features.LoanSettings;

namespace microlending_API.Features.Loans;

public enum LoanStatus
{
    Pending = 1,
    Active = 2,
    Complete = 3,
    Overdue = 4,
    Rejected = 5
}

public enum PaymentStatus
{
    Completed = 1,
    Pending = 2,
    Failed = 3
}

public class CreateLoanRequest
{
    public int BorrowerId { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanSettingId { get; set; }
}

public class GetLoansRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? LoanCode { get; set; }
    public string? PlanName { get; set; }
    public string? BorrowerName { get; set; }
    public LoanStatus? Status { get; set; }
}

public class UpdateLoanDataRequest
{
    public decimal LoanAmount { get; set; }
    public int LoanSettingId { get; set; }
}

public class UpdateLoanStatusRequest
{
    public int StatusId { get; set; }
}

public class RepayLoanRequest
{
    /// <summary>Amount being paid in this transaction.</summary>
    public decimal AmountPaid { get; set; }

    /// <summary>Optional breakdown: portion applied to principal.</summary>
    public decimal? PrincipalAmount { get; set; }

    /// <summary>Optional breakdown: portion applied to interest.</summary>
    public decimal? InterestAmount { get; set; }

    /// <summary>When the payment was made (defaults to UTC now if omitted).</summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>Free-text note, e.g. "Partial repayment #1".</summary>
    public string? Note { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    /// <summary>1 = Disbursement, 2 = Repayment.</summary>
    public int TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal? PrincipalAmount { get; set; }
    public decimal? InterestAmount { get; set; }
    /// <summary>1 = Completed, 2 = Pending, 3 = Failed.</summary>
    public int? PaymentStatus { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int BorrowerId { get; set; }
    public int LoanSettingId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    /// <summary>Sourced from the linked LoanSetting record.</summary>
    public decimal InterestRate { get; set; }
    /// <summary>Sourced from the linked LoanSetting record.</summary>
    public int LoanTerm { get; set; }
    public int CalculationType { get; set; }
    public bool IsActive { get; set; }
    public decimal? TotalRepayableAmount { get; set; }
    public decimal? RemainingBalance { get; set; }
    public int Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedById { get; set; }
    public BorrowerDto? Borrower { get; set; }
    /// <summary>Full transaction history (disbursements + repayments). Populated by GetLoanById.</summary>
    public List<TransactionDto>? Transactions { get; set; }
}
