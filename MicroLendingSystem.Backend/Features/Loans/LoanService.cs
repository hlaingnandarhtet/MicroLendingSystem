using Microsoft.EntityFrameworkCore;
using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using microlending_API.Features.Borrowers;
using microlending_API.Features.LoanSettings;

namespace microlending_API.Features.Loans;

public class LoanService : ILoanService
{
    private const int DisbursementTransactionType = 1;
    private const int RepaymentTransactionType = 2;

    private readonly AppDbContext _context;

    public LoanService(AppDbContext context) => _context = context;

    public async Task<Result<LoanDto>> CreateLoanAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var borrowerExists = await _context.Borrowers.AnyAsync(b => b.Id == request.BorrowerId && b.IsDeleted != true, ct);
        if (!borrowerExists)
        {
            return Result<LoanDto>.Failure($"Borrower with ID {request.BorrowerId} was not found.", 400);
        }

        var setting = await _context.LoanSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.LoanSettingId && s.IsDeleted != true, ct);

        if (setting is null)
        {
            return Result<LoanDto>.Failure($"Loan setting with ID {request.LoanSettingId} was not found.", 400);
        }

        if (!Enum.IsDefined(typeof(CalculationType), setting.CalculationType))
        {
            return Result<LoanDto>.Failure("The selected loan setting has an invalid calculation type.", 400);
        }

        var datePart = DateTime.UtcNow.ToString("yyyyMM");
        var count = await _context.Loans.CountAsync(ct) + 1;

        var loan = new Loan
        {
            BorrowerId = request.BorrowerId,
            LoanSettingId = setting.Id,
            LoanCode = $"LN-{datePart}-{count:D3}",
            LoanAmount = request.LoanAmount,
            InterestRate = setting.InterestRate,
            LoanTerm = setting.LoanTerm,
            CalculationType = setting.CalculationType,
            Status = (int)LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Borrower)
            .Include(l => l.LoanSetting)
            .FirstAsync(l => l.Id == loan.Id, ct);

        return Result<LoanDto>.Success(MapToLoanDto(created));
    }

    public async Task<Result<bool>> ApproveRejectLoanAsync(int id, int statusId, CancellationToken ct)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan is null)
        {
            return Result<bool>.Failure("Loan not found.", 404);
        }

        if (statusId == (int)LoanStatus.Active)
        {
            if (loan.Status == (int)LoanStatus.Active)
            {
                return Result<bool>.Failure("Loan is already active.", 400);
            }

            if (loan.Status != (int)LoanStatus.Pending)
            {
                return Result<bool>.Failure("Only pending loans can be approved.", 400);
            }

            if (!Enum.IsDefined(typeof(CalculationType), loan.CalculationType))
            {
                return Result<bool>.Failure("Loan has an invalid calculation type.", 400);
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var calcType = (CalculationType)loan.CalculationType;
            var totalRepayable = CalculateInterest(loan.LoanAmount, loan.InterestRate, loan.LoanTerm, calcType);

            loan.Status = (int)LoanStatus.Active;
            loan.StartDate = today;
            loan.EndDate = today.AddMonths(loan.LoanTerm);
            loan.TotalRepayableAmount = totalRepayable;
            loan.RemainingBalance = await GetRemainingBalanceAsync(loan.Id, loan.LoanAmount, ct);

            var alreadyDisbursed = await _context.Transactions.AnyAsync(
                t => t.LoanId == loan.Id && t.IsDeleted != true && t.TransactionType == DisbursementTransactionType,
                ct);

            if (!alreadyDisbursed)
            {
                _context.Transactions.Add(new Transaction
                {
                    LoanId = loan.Id,
                    TransactionType = DisbursementTransactionType,
                    Amount = loan.LoanAmount,
                    TransactionDate = DateTime.UtcNow,
                    Description = "Loan disbursement on approval.",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
        }
        else if (statusId == (int)LoanStatus.Rejected)
        {
            if (loan.Status != (int)LoanStatus.Pending)
            {
                return Result<bool>.Failure("Only pending loans can be rejected.", 400);
            }

            loan.Status = (int)LoanStatus.Rejected;
        }
        else
        {
            return Result<bool>.Failure("Invalid status action. Only Active (2) or Rejected (5) are allowed.", 400);
        }

        loan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<PagedResult<PagedPayload<LoanDto>>> GetLoanAsync(GetLoansRequest request, CancellationToken ct)
    {
        if (request.Page < 1 || request.PageSize < 1)
        {
            return PagedResult<PagedPayload<LoanDto>>.Failure("Invalid pagination parameters.", 400);
        }

        var query = _context.Loans.AsNoTracking().Where(l => l.IsDeleted != true);

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == (int)request.Status.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(l => l.LoanCode.Contains(request.SearchTerm));
        }

        var total = await query.CountAsync(ct);
        var entities = await query
            .Include(l => l.Borrower)
            .Include(l => l.LoanSetting)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = entities.Select(MapToLoanDto).ToList();

        var payload = new PagedPayload<LoanDto>
        {
            Items = items,
            TotalCount = total,
            CurrentPage = request.Page,
            PageSize = request.PageSize
        };

        return PagedResult<PagedPayload<LoanDto>>.Success(payload);
    }

    public async Task<Result<LoanDto>> GetLoanByIdAsync(int id, CancellationToken ct)
    {
        var loan = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Borrower)
            .Include(l => l.LoanSetting)
            .FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);

        if (loan is null)
        {
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        return Result<LoanDto>.Success(MapToLoanDto(loan));
    }

    public async Task<Result<LoanDto>> UpdateLoanStatusAsync(int id, UpdateLoanStatusRequest request, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(LoanStatus), request.StatusId))
        {
            return Result<bool>.Failure("Invalid loan status.", 400);
        }

        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan is null)
        {
            return Result<bool>.Failure("Loan not found.", 404);
        }

        loan.Status = request.StatusId;
        loan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteLoanAsync(int id, CancellationToken ct)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan is null)
        {
            return Result<bool>.Failure("Loan not found.", 404);
        }

        loan.IsDeleted = true;
        loan.UpdatedAt = DateTime.UtcNow;
        loan.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<LoanDto>> UpdateLoanAsync(int id, UpdateLoanDataRequest request, CancellationToken ct)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);

        if (loan is null)
        {
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        if (loan.Status != (int)LoanStatus.Pending)
        {
            return Result<LoanDto>.Failure("Cannot update loan data because it is not pending.", 400);
        }

        var setting = await _context.LoanSettings
            .FirstOrDefaultAsync(s => s.Id == request.LoanSettingId && s.IsDeleted != true, ct);

        if (setting is null)
        {
            return Result<LoanDto>.Failure($"Loan setting with ID {request.LoanSettingId} was not found.", 400);
        }

        if (!Enum.IsDefined(typeof(CalculationType), setting.CalculationType))
        {
            return Result<LoanDto>.Failure("The selected loan setting has an invalid calculation type.", 400);
        }

        loan.LoanAmount = request.LoanAmount;
        loan.LoanSettingId = setting.Id;
        loan.InterestRate = setting.InterestRate;
        loan.LoanTerm = setting.LoanTerm;
        loan.CalculationType = setting.CalculationType;
        loan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var updated = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Borrower)
            .Include(l => l.LoanSetting)
            .FirstAsync(l => l.Id == id, ct);

        return Result<LoanDto>.Success(MapToLoanDto(updated));
    }

    /// Total repayable (principal + interest). <paramref name="rate"/> is annual nominal %; <paramref name="term"/> is months.
    /// Monthly: P×(R/100/12)×T interest. Daily: P×(R/100/365)×(T×30) interest (30-day month approximation).

    private static decimal CalculateInterest(decimal principal, decimal rate, int term, CalculationType type)
    {
        if (principal <= 0 || term <= 0)
        {
            return decimal.Round(principal, 2, MidpointRounding.AwayFromZero);
        }

        var interest = type switch
        {
            CalculationType.Monthly => principal * (rate / 100m / 12m) * term,  
            CalculationType.Daily => principal * (rate / 100m / 365m) * (term * 30m),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var total = principal + interest;
        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    private async Task<decimal> GetRemainingBalanceAsync(int loanId, decimal loanAmount, CancellationToken ct)
    {
        var totalRepayments = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.LoanId == loanId && t.IsDeleted != true && t.TransactionType == RepaymentTransactionType)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        var remaining = loanAmount - totalRepayments;
        return decimal.Round(Math.Max(remaining, 0m), 2, MidpointRounding.AwayFromZero);
    }

    private static LoanDto MapToLoanDto(Loan loan) =>
        new()
        {
            Id = loan.Id,
            LoanCode = loan.LoanCode,
            BorrowerId = loan.BorrowerId,
            LoanSettingId = loan.LoanSettingId,
            PlanName = loan.LoanSetting?.PlanName ?? string.Empty,
            LoanAmount = loan.LoanAmount,
            InterestRate = loan.InterestRate,
            LoanTerm = loan.LoanTerm,
            CalculationType = loan.CalculationType,
            TotalRepayableAmount = loan.TotalRepayableAmount,
            RemainingBalance = loan.RemainingBalance,
            Status = loan.Status ?? (int)LoanStatus.Pending,
            StartDate = loan.StartDate,
            EndDate = loan.EndDate,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt,
            Borrower = loan.Borrower is null
                ? null
                : new BorrowerDto
                {
                    Id = loan.Borrower.Id,
                    FullName = loan.Borrower.FullName,
                    UserName = loan.Borrower.UserName,
                    Nrcno = loan.Borrower.Nrcno,
                    PhoneNo = loan.Borrower.PhoneNo,
                    Address = loan.Borrower.Address,
                    DocumentId = loan.Borrower.DocumentId ?? 0,
                    CreatedAt = loan.Borrower.CreatedAt,
                    UpdatedAt = loan.Borrower.UpdatedAt
                }
        };
}
