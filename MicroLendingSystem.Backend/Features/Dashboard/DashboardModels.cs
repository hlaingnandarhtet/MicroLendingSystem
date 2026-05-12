namespace MicroLendingSystem.Backend.Features.Dashboard;

public sealed class DashboardSummaryDto
{
    public bool ShowBorrowerDashboard { get; set; }
    public int ActiveLoansCount { get; set; }
    /// <summary>Non-deleted user accounts (system-wide).</summary>
    public int ActiveUsersCount { get; set; }
    public int TotalBorrowersCount { get; set; }
    public decimal TotalBorrowedAmountAll { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal BorrowedAmount { get; set; }
    public decimal RepaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public decimal AccruedInterest { get; set; }
    public List<DashboardRecentLoanDto> RecentLoans { get; set; } = new();
    public List<DashboardChartPointDto> MonthlyData { get; set; } = new();
}

public sealed class DashboardRecentLoanDto
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class DashboardChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
