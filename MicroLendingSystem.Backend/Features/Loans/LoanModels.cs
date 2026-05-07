using microlending_API.Features.Loans.Constants;

namespace microlending_API.Features.Loans;

public class CreateLoanRequest
{
    public int BorrowerId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
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
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
}

public class UpdateLoanStatusRequest
{
    public int StatusId { get; set; }
}
