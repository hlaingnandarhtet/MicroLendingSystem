using MicroLendingSystem.Backend.Infrastructure;
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
    private readonly ICurrentUserAccessor _currentUser;

    public LoanService(AppDbContext context, ICurrentUserAccessor currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<LoanDto>> CreateLoanAsync(CreateLoanRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is not int creatorId)
        {
            return Result<LoanDto>.Failure("Unauthorized.", 401);
        }

        var borrower = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == request.BorrowerId && b.IsDeleted != true, ct);
        if (borrower is null)
        {
            return Result<LoanDto>.Failure($"Borrower with ID {request.BorrowerId} was not found.", 400);
        }

        //if (!_currentUser.IsAdmin && borrower.CreatedById != creatorId)
        //{
        //    return Result<LoanDto>.Failure("You can only create loans for borrowers you registered.", 403);
        //}

        if (_currentUser.IsBorrower)
        {
            if (_currentUser.UserId is not int bid || borrower.UserId != bid)
                return Result<LoanDto>.Failure("You can only request loans for your own profile.", 403);
        }
        else if (!_currentUser.IsAdmin && borrower.CreatedById != creatorId)
            return Result<LoanDto>.Failure("You can only create loans for borrowers you registered.", 403);


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
            CalculationType = setting.CalculationType,
            Status = (int)LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            CreatedById = creatorId
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
        if (!_currentUser.IsAdmin)
        {
            return Result<bool>.Failure("Only administrators can approve or reject loans.", 403);
        }

        var loan = await _context.Loans
            .Include(l => l.LoanSetting)
            .FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
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

            if (loan.LoanSetting is null)
            {
                return Result<bool>.Failure("Loan has no associated setting.", 400);
            }

            if (!Enum.IsDefined(typeof(CalculationType), loan.LoanSetting.CalculationType))
            {
                return Result<bool>.Failure("Loan has an invalid calculation type.", 400);
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var calcType = (CalculationType)loan.LoanSetting.CalculationType;
            var totalRepayable = CalculateInterest(
                loan.LoanAmount,
                loan.LoanSetting.InterestRate,
                loan.LoanSetting.LoanTerm,
                calcType);

            loan.Status = (int)LoanStatus.Active;
            loan.StartDate = today;
            loan.EndDate = today.AddMonths(loan.LoanSetting.LoanTerm);
            loan.TotalRepayableAmount = totalRepayable;
            loan.RemainingBalance = await ComputeRemainingBalanceAsync(loan.Id, totalRepayable, ct);

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
                    PrincipalAmount = loan.LoanAmount,
                    InterestAmount = null,
                    PaymentStatus = (int)PaymentStatus.Completed,
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

        try
        {
            var query = _context.Loans.AsNoTracking().Where(l => l.IsDeleted != true);

            if (_currentUser != null && !_currentUser.IsAdmin)
            {
                if (_currentUser.IsBorrower && _currentUser.UserId is int borrowerUid)
                {
                    query = query.Where(l => _context.Borrowers.Any(b =>
                        b.Id == l.BorrowerId && b.IsDeleted != true && b.UserId == borrowerUid));
                }
                else if (_currentUser.UserId is int ownerId)
                {
                    query = query.Where(l => l.CreatedById == ownerId);
                }
            }

            if (request.Status.HasValue)
                query = query.Where(l => l.Status == (int)request.Status.Value);

            if (!string.IsNullOrEmpty(request.LoanCode))
                query = query.Where(l => l.LoanCode.Contains(request.LoanCode));

            if (!string.IsNullOrEmpty(request.PlanName))
                query = query.Where(l => l.LoanSetting.PlanName.Contains(request.PlanName));

            if (!string.IsNullOrEmpty(request.BorrowerName))
                query = query.Where(l => l.Borrower.FullName.Contains(request.BorrowerName));

            var total = await query.CountAsync(ct);
            var entities = await query
                .Include(l => l.Borrower)
                .Include(l => l.LoanSetting)
                .OrderByDescending(l => l.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var items = entities?.Select(x => MapToLoanDto(x, false)).ToList() ?? new List<LoanDto>();

            return PagedResult<PagedPayload<LoanDto>>.Success(new PagedPayload<LoanDto>
            {
                Items = items,
                TotalCount = total,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            return PagedResult<PagedPayload<LoanDto>>.Success(new PagedPayload<LoanDto>
            {
                Items = new List<LoanDto>(),
                TotalCount = 0,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            });
        }
    }

    public async Task<Result<LoanDto>> GetLoanByIdAsync(int id, CancellationToken ct)
    {
        var loan = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Borrower)
            .Include(l => l.LoanSetting)
            .Include(l => l.Transactions.Where(t => t.IsDeleted != true).OrderBy(t => t.TransactionDate).ThenBy(t => t.Id))
            .FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);

        if (loan is null)
        {
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        if (!CanAccessLoan(loan))
        {
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        return Result<LoanDto>.Success(MapToLoanDto(loan, includeTransactions: true));
    }

    public async Task<Result<LoanDto>> RepayLoanAsync(int loanId, RepayLoanRequest request, CancellationToken ct)
    {
        if (request.AmountPaid <= 0)
        {
            return Result<LoanDto>.Failure("Repayment amount must be greater than zero.", 400);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        var loan = await _context.Loans
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == loanId && l.IsDeleted != true, ct);

        if (loan is null)
        {
            await transaction.RollbackAsync(ct);
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        if (!CanAccessLoan(loan))
        {
            await transaction.RollbackAsync(ct);
            return Result<LoanDto>.Failure($"You do not have access to this loan. (UID: {_currentUser.UserId}, IsBorrower: {_currentUser.IsBorrower}, BorrowerUID: {loan.Borrower?.UserId})", 403);
        }

        if (loan.Status != (int)LoanStatus.Active)
        {
            await transaction.RollbackAsync(ct);
            return Result<LoanDto>.Failure("Only active loans can receive repayments.", 400);
        }

        if (loan.TotalRepayableAmount is null)
        {
            await transaction.RollbackAsync(ct);
            return Result<LoanDto>.Failure("Loan has no total repayable amount set; approve the loan before repaying.", 400);
        }

        var paymentDate = request.PaymentDate ?? DateTime.UtcNow;
        var note = string.IsNullOrWhiteSpace(request.Note)
            ? "Manual repayment."
            : request.Note!.Trim();

        var amountRounded = decimal.Round(request.AmountPaid, 2, MidpointRounding.AwayFromZero);
        decimal principalApplied;
        decimal interestApplied;

        switch (request.PrincipalAmount, request.InterestAmount)
        {
            case (not null, not null):
                principalApplied = request.PrincipalAmount.Value;
                interestApplied = request.InterestAmount.Value;
                break;
            case (not null, null):
                principalApplied = request.PrincipalAmount.Value;
                interestApplied = amountRounded - decimal.Round(principalApplied, 2, MidpointRounding.AwayFromZero);
                break;
            case (null, not null):
                interestApplied = request.InterestAmount.Value;
                principalApplied = amountRounded - decimal.Round(interestApplied, 2, MidpointRounding.AwayFromZero);
                break;
            default:
                principalApplied = amountRounded;
                interestApplied = 0m;
                break;
        }

        principalApplied = decimal.Round(principalApplied, 2, MidpointRounding.AwayFromZero);
        interestApplied = decimal.Round(interestApplied, 2, MidpointRounding.AwayFromZero);

        if (principalApplied < 0 || interestApplied < 0)
        {
            await transaction.RollbackAsync(ct);
            return Result<LoanDto>.Failure("Principal and interest portions cannot be negative.", 400);
        }

        if (decimal.Round(principalApplied + interestApplied, 2, MidpointRounding.AwayFromZero) != amountRounded)
        {
            await transaction.RollbackAsync(ct);
            return Result<LoanDto>.Failure("Principal and interest portions must add up to AmountPaid.", 400);
        }

        var repayment = new Transaction
        {
            LoanId = loanId,
            TransactionType = RepaymentTransactionType,
            Amount = amountRounded,
            PrincipalAmount = principalApplied,
            InterestAmount = interestApplied,
            PaymentStatus = (int)PaymentStatus.Completed,
            TransactionDate = paymentDate,
            Description = note,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Transactions.Add(repayment);
        await _context.SaveChangesAsync(ct);

        var remaining = await ComputeRemainingBalanceAsync(loanId, loan.TotalRepayableAmount.Value, ct);
        loan.RemainingBalance = remaining;
        loan.UpdatedAt = DateTime.UtcNow;

        if (remaining <= 0)
        {
            loan.Status = (int)LoanStatus.Complete;
        }

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var updated = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Borrower)
            .Include(l => l.LoanSetting)
            .Include(l => l.Transactions.Where(t => t.IsDeleted != true).OrderBy(t => t.TransactionDate).ThenBy(t => t.Id))
            .FirstAsync(l => l.Id == loanId, ct);

        return Result<LoanDto>.Success(MapToLoanDto(updated, includeTransactions: true));
    }

    public async Task<Result<LoanDto>> UpdateLoanStatusAsync(int id, UpdateLoanStatusRequest request, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(LoanStatus), request.StatusId))
        {
            return Result<LoanDto>.Failure("Invalid loan status.", 400);
        }

        var loan = await _context.Loans
            .Include(l => l.LoanSetting)
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan is null)
        {
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        if (!CanAccessLoan(loan))
        {
            return Result<LoanDto>.Failure("You do not have access to this loan.", 403);
        }

        loan.Status = request.StatusId;
        loan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<LoanDto>.Success(MapToLoanDto(loan));
    }

    public async Task<Result<bool>> DeleteLoanAsync(int id, CancellationToken ct)
    {
        var loan = await _context.Loans
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan is null)
        {
            return Result<bool>.Failure("Loan not found.", 404);
        }

        if (!CanAccessLoan(loan))
        {
            return Result<bool>.Failure("You do not have access to this loan.", 403);
        }

        loan.IsDeleted = true;
        loan.UpdatedAt = DateTime.UtcNow;
        loan.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<LoanDto>> UpdateLoanAsync(int id, UpdateLoanDataRequest request, CancellationToken ct)
    {
        var loan = await _context.Loans
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);

        if (loan is null)
        {
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        if (loan.Status != (int)LoanStatus.Pending)
        {
            return Result<LoanDto>.Failure("Cannot update loan data because it is not pending.", 400);
        }

        if (!CanAccessLoan(loan))
        {
            return Result<LoanDto>.Failure("You do not have access to this loan.", 403);
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

    /// <summary>
    /// Remaining balance = TotalRepayableAmount minus sum of repayment transaction amounts (type 2), floored at zero.
    /// </summary>
    private async Task<decimal> ComputeRemainingBalanceAsync(int loanId, decimal totalRepayableAmount, CancellationToken ct)
    {
        var totalRepayments = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.LoanId == loanId && t.IsDeleted != true && t.TransactionType == RepaymentTransactionType)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        var remaining = totalRepayableAmount - totalRepayments;
        return decimal.Round(Math.Max(remaining, 0m), 2, MidpointRounding.AwayFromZero);
    }

    private bool CanAccessLoan(Loan loan)
    {
        if (_currentUser.IsAdmin) return true;
        if (_currentUser.IsBorrower && _currentUser.UserId is int buid)
            return loan.Borrower?.UserId == buid; // ensure Borrower loaded or query again
        return _currentUser.UserId is int uid && loan.CreatedById == uid;
    }

    private static LoanDto MapToLoanDto(Loan loan, bool includeTransactions = false) =>
        new()
        {
            Id = loan.Id,
            LoanCode = loan.LoanCode,
            BorrowerId = loan.BorrowerId,
            LoanSettingId = loan.LoanSettingId,
            PlanName = loan.LoanSetting?.PlanName ?? string.Empty,
            LoanAmount = loan.LoanAmount,
            // Sourced from the linked LoanSetting (not stored on the Loan row)
            InterestRate = loan.LoanSetting?.InterestRate ?? 0m,
            LoanTerm = loan.LoanSetting?.LoanTerm ?? 0,
            CalculationType = loan.LoanSetting?.CalculationType ?? loan.CalculationType ?? 0,
            IsActive = loan.LoanSetting?.IsActive ?? false,
            TotalRepayableAmount = loan.TotalRepayableAmount,
            RemainingBalance = loan.RemainingBalance,
            Status = loan.Status ?? (int)LoanStatus.Pending,
            StartDate = loan.StartDate,
            EndDate = loan.EndDate,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt,
            CreatedById = loan.CreatedById,
            Borrower = loan.Borrower is null
                ? null
                : new BorrowerDto
                {
                    Id = loan.Borrower.Id,
                    FullName = loan.Borrower.FullName,
                    //UserName = loan.Borrower!.UserName,
                    //Nrcno = loan.Borrower.Nrcno,
                    //PhoneNo = loan.Borrower.PhoneNo,
                    //Address = loan.Borrower.Address,
                    UserName = loan.Borrower?.UserName ?? string.Empty,
                    Nrcno = loan.Borrower?.Nrcno ?? string.Empty,
                    PhoneNo = loan.Borrower?.PhoneNo ?? string.Empty,
                    Address = loan.Borrower?.Address ?? string.Empty,
                    //DocumentId = loan.Borrower?.DocumentId ?? 0,
                    CreatedById = loan.Borrower?.CreatedById,
                    CreatedAt = loan.Borrower?.CreatedAt,
                    UpdatedAt = loan.Borrower?.UpdatedAt
                },
            Transactions = includeTransactions && loan.Transactions is not null
                ? loan.Transactions.Select(MapToTransactionDto).ToList()
                : null
        };

    private static TransactionDto MapToTransactionDto(Transaction t) =>
        new()
        {
            Id = t.Id,
            LoanId = t.LoanId,
            TransactionType = t.TransactionType,
            Amount = t.Amount,
            PrincipalAmount = t.PrincipalAmount,
            InterestAmount = t.InterestAmount,
            PaymentStatus = t.PaymentStatus,
            TransactionDate = t.TransactionDate,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        };
}
