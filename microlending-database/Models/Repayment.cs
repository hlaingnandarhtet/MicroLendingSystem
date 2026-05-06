namespace MicroLendingSystem.Database.Models;

public partial class Repayment
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    public decimal? AmountPaid { get; set; }

    public decimal? InterestPaid { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public decimal? RemainingBalance { get; set; }

    public string? Status { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Loan Loan { get; set; } = null!;
}
