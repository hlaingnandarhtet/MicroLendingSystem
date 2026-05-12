using MicroLendingSystem.Backend.Infrastructure;
using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using Microsoft.EntityFrameworkCore;
using microlending_API.Features.Loans;

namespace MicroLendingSystem.Backend.Features.Dashboard;

public sealed class DashboardService(AppDbContext context, ICurrentUserAccessor currentUser) : IDashboardService
{
    private readonly AppDbContext _context = context;
    private readonly ICurrentUserAccessor _currentUser = currentUser;
    private const int TxRepayment = 2;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct)
    {
        var loans = VisibleLoans();
        var activeStatus = (int)LoanStatus.Active;
        var rejected = (int)LoanStatus.Rejected;

        var activeLoansCount = await loans
            .Where(l => l.Status == activeStatus)
            .CountAsync(ct);

        var borrowerQuery = _context.Borrowers.AsNoTracking().Where(b => b.IsDeleted != true);
        if (!_currentUser.IsAdmin)
        {
            if (!_currentUser.TryGetUserId(out var uid))
            {
                return new DashboardSummaryDto();
            }

            if (_currentUser.IsBorrower)
                borrowerQuery = borrowerQuery.Where(b => b.UserId == uid);
            else
                borrowerQuery = borrowerQuery.Where(b => b.CreatedById == uid);
        }

        var totalBorrowersCount = await borrowerQuery.CountAsync(ct);

        var recentLoans = await loans
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(l => new DashboardRecentLoanDto
            {
                Id = l.Id,
                LoanCode = l.LoanCode,
                BorrowerName = l.Borrower != null ? l.Borrower.FullName : string.Empty,
                Amount = l.LoanAmount,
                Status = l.Status ?? 0,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(ct);

        var chartLoans = await loans
            .OrderByDescending(l => l.CreatedAt)
            .Take(200)
            .Select(l => new { l.LoanAmount, l.CreatedAt })
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var monthlyData = new List<DashboardChartPointDto>();
        for (int i = 5; i >= 0; i--)
        {
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            var end = start.AddMonths(1);
            var total = chartLoans
                .Where(l => l.CreatedAt >= start && l.CreatedAt < end)
                .Sum(l => l.LoanAmount);
            monthlyData.Add(new DashboardChartPointDto
            {
                Label = start.ToString("MMM yyyy"),
                Value = total
            });
        }

        var dto = new DashboardSummaryDto
        {
            ActiveLoansCount = activeLoansCount,
            TotalBorrowersCount = totalBorrowersCount,
            RecentLoans = recentLoans,
            MonthlyData = monthlyData
        };

        if (_currentUser.IsBorrower && _currentUser.UserId is int borrowerUid)
        {
            dto.ShowBorrowerDashboard = true;
            var myLoans = _context.Loans.AsNoTracking()
                .Where(l => l.IsDeleted != true
                            && l.Borrower != null
                            && l.Borrower.IsDeleted != true
                            && l.Borrower.UserId == borrowerUid);

            dto.BorrowedAmount = await myLoans
                .Where(l => l.Status != rejected)
                .SumAsync(l => l.LoanAmount, ct);

            dto.RepaidAmount = await (
                from t in _context.Transactions.AsNoTracking()
                join l in myLoans on t.LoanId equals l.Id
                where t.IsDeleted != true
                      && t.TransactionType == TxRepayment
                      && t.PaymentStatus == (int)PaymentStatus.Completed
                select t.Amount).SumAsync(ct);

            dto.RemainingBalance = await myLoans
                .Where(l => l.RemainingBalance.HasValue)
                .SumAsync(l => l.RemainingBalance!.Value, ct);

            dto.AccruedInterest = await myLoans
                .Where(l => l.Status != rejected && l.TotalRepayableAmount.HasValue)
                .SumAsync(l => l.TotalRepayableAmount!.Value - l.LoanAmount, ct);

            return dto;
        }

        dto.ShowBorrowerDashboard = false;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        IQueryable<Loan> financeLoans = _context.Loans.AsNoTracking().Where(l => l.IsDeleted != true);
        if (!_currentUser.IsAdmin)
        {
            if (!_currentUser.TryGetUserId(out var staffId))
                return dto;
            financeLoans = financeLoans.Where(l => l.CreatedById == staffId);
        }

        dto.TotalBorrowedAmountAll = await financeLoans
            .Where(l => l.Status != rejected)
            .SumAsync(l => l.LoanAmount, ct);

        dto.RevenueThisMonth = await (
            from t in _context.Transactions.AsNoTracking()
            join l in financeLoans on t.LoanId equals l.Id
            where t.IsDeleted != true
                  && t.TransactionType == TxRepayment
                  && t.PaymentStatus == (int)PaymentStatus.Completed
                  && t.TransactionDate >= monthStart
            select t.Amount).SumAsync(ct);

        dto.ActiveUsersCount = await _context.Users.AsNoTracking()
            .Where(u => u.IsDeleted != true)
            .CountAsync(ct);

        return dto;
    }

    private IQueryable<Loan> VisibleLoans()
    {
        var baseQuery = _context.Loans.AsNoTracking().Where(l => l.IsDeleted != true);

        if (_currentUser.IsAdmin)
            return baseQuery.Include(l => l.Borrower);

        if (_currentUser.IsBorrower && _currentUser.UserId is int borrowerUid)
        {
            return baseQuery
                .Include(l => l.Borrower)
                .Where(l => l.Borrower != null
                            && l.Borrower.IsDeleted != true
                            && l.Borrower.UserId == borrowerUid);
        }

        if (_currentUser.UserId is int staffId)
            return baseQuery.Include(l => l.Borrower).Where(l => l.CreatedById == staffId);

        return baseQuery.Where(_ => false);
    }
}
