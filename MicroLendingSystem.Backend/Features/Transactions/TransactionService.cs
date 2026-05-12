using MicroLendingSystem.Backend.Infrastructure;
using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using microlending_API.Features.Loans;

namespace microlending_API.Features.Transactions;

public sealed class TransactionService(AppDbContext context, ICurrentUserAccessor currentUser) : ITransactionService
{
    private readonly AppDbContext _context = context;
    private readonly ICurrentUserAccessor _currentUser = currentUser;

    public async Task<PagedResult<PagedPayload<TransactionListRowDto>>> GetTransactionsAsync(
        GetTransactionsRequest request,
        CancellationToken ct)
    {
        if (request.Page < 1 || request.PageSize < 1)
        {
            return PagedResult<PagedPayload<TransactionListRowDto>>.Failure("Invalid pagination parameters.", 400);
        }

        //var baseQuery =
        //    from t in _context.Transactions.AsNoTracking()
        //    join l in _context.Loans.AsNoTracking() on t.LoanId equals l.Id
        //    join b in _context.Borrowers.AsNoTracking() on l.BorrowerId equals b.Id
        //    where t.IsDeleted != true && l.IsDeleted != true && b.IsDeleted != true
        //    select new { t, l, b };

        //if (!_currentUser.IsAdmin && _currentUser.UserId is int ownerId)
        //{
        //    baseQuery = baseQuery.Where(x => x.l.CreatedById == ownerId);
        //}

        //if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //{
        //    var term = request.SearchTerm.Trim();
        //    baseQuery = baseQuery.Where(x =>
        //        x.l.LoanCode.Contains(term) ||
        //        x.b.FullName.Contains(term));
        //}

        //var projected = baseQuery.Select(x => new TransactionListRowDto
        //{
        //    Id = x.t.Id,
        //    LoanId = x.l.Id,
        //    TransactionDate = x.t.TransactionDate,
        //    BorrowerName = x.b.FullName,
        //    LoanAmount = x.l.LoanAmount,
        //    Paid = x.t.Amount,
        //    Status = x.t.PaymentStatus == (int)PaymentStatus.Completed ? "Completed"
        //        : x.t.PaymentStatus == (int)PaymentStatus.Pending ? "Pending"
        //        : x.t.PaymentStatus == (int)PaymentStatus.Failed ? "Failed"
        //        : "—"
        //});

        var baseQuery =
        from t in _context.Transactions.AsNoTracking()
        join l in _context.Loans.AsNoTracking() on t.LoanId equals l.Id
        join b in _context.Borrowers.AsNoTracking() on l.BorrowerId equals b.Id
        join s in _context.LoanSettings.AsNoTracking() on l.LoanSettingId equals s.Id
        where t.IsDeleted != true && l.IsDeleted != true && b.IsDeleted != true && s.IsDeleted != true
        select new { t, l, b, s };
        if (!_currentUser.IsAdmin && _currentUser.UserId is int uid)
        {
            if (_currentUser.IsBorrower)
                baseQuery = baseQuery.Where(x => x.b.UserId == uid);
            else
                baseQuery = baseQuery.Where(x => x.l.CreatedById == uid);
        }

        if (!string.IsNullOrWhiteSpace(request.LoanCode))
            baseQuery = baseQuery.Where(x => x.l.LoanCode.Contains(request.LoanCode.Trim()));

        if (!string.IsNullOrWhiteSpace(request.PlanName))
            baseQuery = baseQuery.Where(x => x.s.PlanName.Contains(request.PlanName.Trim()));

        if (!string.IsNullOrWhiteSpace(request.BorrowerName))
            baseQuery = baseQuery.Where(x => x.b.FullName.Contains(request.BorrowerName));

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statusTerm = request.Status.Trim();
            baseQuery = baseQuery.Where(x =>
                (x.t.PaymentStatus == (int)PaymentStatus.Completed && "Completed".Contains(statusTerm)) ||
                (x.t.PaymentStatus == (int)PaymentStatus.Pending   && "Pending".Contains(statusTerm))   ||
                (x.t.PaymentStatus == (int)PaymentStatus.Failed    && "Failed".Contains(statusTerm)));
        }

        if (request.StartDate.HasValue)
            baseQuery = baseQuery.Where(x => x.t.TransactionDate >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            baseQuery = baseQuery.Where(x => x.t.TransactionDate <= request.EndDate.Value.AddDays(1));

        var projected = baseQuery.Select(x => new TransactionListRowDto
        {
            Id = x.t.Id,
            LoanId = x.l.Id,
            LoanCode = x.l.LoanCode,
            TransactionDate = x.t.TransactionDate,
            BorrowerName = x.b.FullName,
            LoanPlanName = x.s.PlanName,
            LoanAmount = x.l.LoanAmount,
            Paid = x.t.Amount,
            Status = x.t.PaymentStatus == (int)PaymentStatus.Completed ? "Completed"
                : x.t.PaymentStatus == (int)PaymentStatus.Pending ? "Pending"
                : x.t.PaymentStatus == (int)PaymentStatus.Failed ? "Failed"
                : "—",
            TotalRepayableAmount = x.l.TotalRepayableAmount,
            RemainingBalance = x.l.RemainingBalance
        });

        var total = await projected.CountAsync(ct);
        var items = await projected
            .OrderByDescending(r => r.Id)
            .ThenByDescending(r => r.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var payload = new PagedPayload<TransactionListRowDto>
        {
            Items = items,
            TotalCount = total,
            CurrentPage = request.Page,
            PageSize = request.PageSize
        };

        return PagedResult<PagedPayload<TransactionListRowDto>>.Success(payload);
    }
}
