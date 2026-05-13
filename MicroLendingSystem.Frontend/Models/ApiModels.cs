// ============================================================
//  MicroLendingSystem.Frontend.Models.ApiModels
//  Local DTO copies matching the Backend API contracts.
//  These are kept in sync with MicroLendingSystem.Backend DTOs.
// ============================================================

namespace MicroLendingSystem.Frontend.Models;

// ── Shared pagination ────────────────────────────────────────
public sealed class PagedPayload<TItem>
{
    public List<TItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}

// ── Borrower ─────────────────────────────────────────────────
public class BorrowerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Nrcno { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public int? CreatedById { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

}

public class CreateBorrowerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Nrcno { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public bool CreateLoginAccount { get; set; }
    public string? LoginEmail { get; set; }
    public string? LoginPassword { get; set; }
}

public class UpdateBorrowerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Nrcno { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public bool CreateLoginAccount { get; set; }
    public string? LoginEmail { get; set; }
    public string? LoginPassword { get; set; }
}

// ── Loan Settings ─────────────────────────────────────────────
public enum CalculationType { Monthly = 1, Daily = 2 }

public class LoanSettingDto
{
    public int Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public CalculationType CalculationType { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
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

// ── Loans ─────────────────────────────────────────────────────
public enum LoanStatus { Pending = 1, Active = 2, Complete = 3, Overdue = 4, Rejected = 5 }

public class TransactionDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public int TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal? PrincipalAmount { get; set; }
    public decimal? InterestAmount { get; set; }
    public int? PaymentStatus { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int BorrowerId { get; set; }
    public int LoanSettingId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTerm { get; set; }
    public int CalculationType { get; set; }
    public bool IsActive { get; set; }
    public decimal? TotalRepayableAmount { get; set; }
    public decimal? RemainingBalance { get; set; }
    public int Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedById { get; set; }
    public BorrowerDto? Borrower { get; set; }
    public List<TransactionDto>? Transactions { get; set; }
}

public sealed class TransactionListRowDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string BorrowerName { get; set; } = string.Empty;

    public string LoanCode { get; set; } = string.Empty;
    public string LoanPlanName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal Paid { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? TotalRepayableAmount { get; set; }
    public decimal? RemainingBalance { get; set; }
}

public class CreateLoanRequest
{
    public int BorrowerId { get; set; }
    public decimal LoanAmount { get; set; }
    public int LoanSettingId { get; set; }
}

public class UpdateLoanDataRequest
{
    public decimal LoanAmount { get; set; }
    public int LoanSettingId { get; set; }
}

public class RepayLoanRequest
{
    public decimal AmountPaid { get; set; }
    public decimal? PrincipalAmount { get; set; }
    public decimal? InterestAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Note { get; set; }
}

// ── Dashboard ─────────────────────────────────────────────────
public sealed class DashboardSummaryDto
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

// ── Users ─────────────────────────────────────────────────────
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
    public UserDto User { get; set; } = null!;
    public List<string> Permissions { get; set; } = new();
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? RoleId { get; set; }
}

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class AssignUserRoleRequest
{
    public int RoleId { get; set; }
}

// ── Roles ─────────────────────────────────────────────────────
public class RoleSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RoleDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;
}

public class AssignPermissionsToRoleRequest
{
    public List<int> PermissionIds { get; set; } = new();
}

// ── Permissions ───────────────────────────────────────────────
public class PermissionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
