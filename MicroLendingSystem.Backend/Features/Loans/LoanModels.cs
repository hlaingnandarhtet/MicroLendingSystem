using microlending_API.Features.Borrowers;

namespace microlending_API.Features.Loans;

public enum LoanStatus
{
    Pending = 1,
    Active = 2,
    Complete = 3,
    Overdue = 4,
    Rejected = 5
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
    public string? SearchTerm { get; set; }
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

public class LoanDto
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int BorrowerId { get; set; }
    public int LoanSettingId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public int CalculationType { get; set; }
    public decimal? TotalRepayableAmount { get; set; }
    public decimal? RemainingBalance { get; set; }
    public int Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public BorrowerDto? Borrower { get; set; }
}
