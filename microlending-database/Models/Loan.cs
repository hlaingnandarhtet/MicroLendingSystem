namespace MicroLendingSystem.Database.Models;

public partial class Loan
{
    public int Id { get; set; }

    public int BorrowerId { get; set; }

    public string LoanCode { get; set; } = null!;

    public decimal LoanAmount { get; set; }

    public decimal InterestRate { get; set; }

    public int LoanTerm { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Borrower Borrower { get; set; } = null!;

    public virtual ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
}
