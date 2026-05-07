namespace MicroLendingSystem.Database.Models;

public partial class LoanSetting
{
    public int Id { get; set; }

    public string PlanName { get; set; } = null!;

    public decimal InterestRate { get; set; }

    public int LoanTerm { get; set; }

    /// <summary>1 = Monthly, 2 = Daily (matches API <c>CalculationType</c>).</summary>
    public int CalculationType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
