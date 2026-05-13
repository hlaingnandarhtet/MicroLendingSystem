namespace MicroLendingSystem.Frontend.ViewModels;

public class DashboardViewModel
{
    public bool ShowBorrowerDashboard { get; set; }
    public int ActiveLoansCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public int TotalBorrowersCount { get; set; }
    public decimal TotalBorrowedAmountAll { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal BorrowedAmount { get; set; }
    public decimal RepaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public decimal AccruedInterest { get; set; }

    public List<ChartDataPoint> MonthlyData { get; set; } = new();
    public List<RecentLoanItem> RecentLoans { get; set; } = new();
    public Dictionary<string, int> StatusDistribution { get; set; } = new();

    public List<Models.LoanDto> PendingLoans { get; set; } = new();
    public int PendingLoansTotalCount { get; set; }
    public int PendingLoansCurrentPage { get; set; }
    public int PendingLoansPageSize { get; set; }
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class RecentLoanItem
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}
