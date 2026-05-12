namespace microlending_API.Features.LoanSettings;

public enum CalculationType
{
    Monthly = 1,
    Daily = 2
}

public enum DisbursementTransactionType
{
    Disbursement = 1
}

public class LoanSettingDto
{
    public int Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public CalculationType CalculationType { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }
    
}

public class CreateLoanSettingRequest
{
    public string PlanName { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public bool IsActive { get; set; }
    public CalculationType CalculationType { get; set; }
}

public class UpdateLoanSettingRequest
{
    public string PlanName { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public bool IsActive { get; set; }
    public int LoanTerm { get; set; }
    public CalculationType CalculationType { get; set; }
}

public class GetLoanSettingsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? PlanName { get; set; }
}
