namespace MicroLendingSystem.Database.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    /// <summary>1 = Disbursement, 2 = Repayment.</summary>
    public int TransactionType { get; set; }

    /// <summary>Total amount paid / disbursed.</summary>
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    /// <summary>Principal portion (e.g. disbursed principal or principal paid in a repayment).</summary>
    public decimal? PrincipalAmount { get; set; }

    /// <summary>Interest portion (e.g. on repayments); optional for disbursements.</summary>
    public decimal? InterestAmount { get; set; }

    /// <summary>1 = Completed, 2 = Pending, 3 = Failed.</summary>
    public int? PaymentStatus { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Loan Loan { get; set; } = null!;
}
