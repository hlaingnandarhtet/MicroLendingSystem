using MicroLendingSystem.Frontend.Models;

namespace MicroLendingSystem.Frontend.ViewModels;

public class LoanFormViewModel
{
    public int BorrowerId { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanSettingId { get; set; }

    public List<BorrowerDto> Borrowers { get; set; } = new();
    public List<LoanSettingDto> LoanSettings { get; set; } = new();
}

public class LoanEditViewModel
{
    public int Id { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanSettingId { get; set; }
    public List<LoanSettingDto> LoanSettings { get; set; } = new();
}

public class LoanRepayViewModel
{
    public int LoanId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public decimal RemainingBalance { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal? PrincipalAmount { get; set; }
    public decimal? InterestAmount { get; set; }
    public string? Note { get; set; }
}

public class LoanIndexViewModel : PagedViewModel<LoanDto>
{
    public LoanStatus? StatusFilter { get; set; }
    public new string? SearchTerm { get; set; }
}
