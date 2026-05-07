namespace microlending_API.Features.Loans.Models;

public class BorrowerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Nrcno { get; set; }
    public string? PhoneNo { get; set; }
    public string? Address { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int BorrowerId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public int Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public BorrowerDto? Borrower { get; set; }
}
