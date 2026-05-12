using MicroLendingSystem.Frontend.Models;

namespace MicroLendingSystem.Frontend.ViewModels;

public class LoanSettingFormViewModel
{
    public int Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public bool IsActive { get; set; } = true;
    public CalculationType CalculationType { get; set; } = CalculationType.Monthly;
}
