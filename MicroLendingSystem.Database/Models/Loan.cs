namespace MicroLendingSystem.Database.Models;

public partial class Loan
{
    public int Id { get; set; }

    public int BorrowerId { get; set; }

    public int? LoanSettingId { get; set; }

    public string LoanCode { get; set; } = null!;

    public decimal LoanAmount { get; set; }

    public decimal InterestRate { get; set; }

    public int LoanTerm { get; set; }

    /// <summary>1 = Monthly, 2 = Daily (snapshot from loan setting at creation).</summary>
    public int? CalculationType { get; set; }

    public decimal? TotalRepayableAmount { get; set; }

    public decimal? RemainingBalance { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Borrower Borrower { get; set; } = null!;

    public virtual LoanSetting LoanSetting { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
