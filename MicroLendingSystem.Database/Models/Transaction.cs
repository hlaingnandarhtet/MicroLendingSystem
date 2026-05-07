namespace MicroLendingSystem.Database.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    /// <summary>1 = Disbursement, 2 = Repayment.</summary>
    public int TransactionType { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Loan Loan { get; set; } = null!;
}
