using Microsoft.EntityFrameworkCore;
using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using microlending_API.Features.Loans.Constants;
using microlending_API.Features.Loans.Models;

namespace microlending_API.Features.Loans;

public class LoanService : ILoanService
{
    private readonly AppDbContext _context;

    public LoanService(AppDbContext context) => _context = context;

    public async Task<Result<LoanDto>> CreateLoanAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var borrowerExists = await _context.Borrowers.AnyAsync(b => b.Id == request.BorrowerId && b.IsDeleted != true, ct);
        if (!borrowerExists)
        {
            return Result<LoanDto>.Failure($"Borrower with ID {request.BorrowerId} was not found.", 400);
        }

        var datePart = DateTime.UtcNow.ToString("yyyyMM");
        var count = await _context.Loans.CountAsync(ct) + 1;

        var loan = new Loan
        {
            BorrowerId = request.BorrowerId,
            LoanCode = $"LN-{datePart}-{count:D3}",
            LoanAmount = request.LoanAmount,
            InterestRate = request.InterestRate,
            LoanTerm = request.LoanTerm,
            Status = (int)LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Borrower)
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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            loan.Status = (int)LoanStatus.Active;
            loan.StartDate = today;
            loan.EndDate = today.AddMonths(loan.LoanTerm);
        }
        else if (statusId == (int)LoanStatus.Rejected)
        {
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
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                LoanCode = l.LoanCode,
                BorrowerId = l.BorrowerId,
                LoanAmount = l.LoanAmount,
                InterestRate = l.InterestRate,
                LoanTerm = l.LoanTerm,
                Status = l.Status ?? (int)LoanStatus.Pending,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Borrower = l.Borrower == null
                    ? null
                    : new BorrowerDto
                    {
                        Id = l.Borrower.Id,
                        FullName = l.Borrower.FullName,
                        UserName = l.Borrower.UserName,
                        Nrcno = l.Borrower.Nrcno,
                        PhoneNo = l.Borrower.PhoneNo,
                        Address = l.Borrower.Address
                    }
            })
            .ToListAsync(ct);

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
        var dto = await _context.Loans
            .AsNoTracking()
            .Where(l => l.Id == id && l.IsDeleted != true)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                LoanCode = l.LoanCode,
                BorrowerId = l.BorrowerId,
                LoanAmount = l.LoanAmount,
                InterestRate = l.InterestRate,
                LoanTerm = l.LoanTerm,
                Status = l.Status ?? (int)LoanStatus.Pending,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Borrower = l.Borrower == null
                    ? null
                    : new BorrowerDto
                    {
                        Id = l.Borrower.Id,
                        FullName = l.Borrower.FullName,
                        UserName = l.Borrower.UserName,
                        Nrcno = l.Borrower.Nrcno,
                        PhoneNo = l.Borrower.PhoneNo,
                        Address = l.Borrower.Address
                    }
            })
            .FirstOrDefaultAsync(ct);

        if (dto is null)
        {
            return Result<LoanDto>.Failure("Loan not found.", 404);
        }

        return Result<LoanDto>.Success(dto);
    }

    public async Task<Result<bool>> UpdateLoanStatusAsync(int id, UpdateLoanStatusRequest request, CancellationToken ct)
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

        loan.LoanAmount = request.LoanAmount;
        loan.InterestRate = request.InterestRate;
        loan.LoanTerm = request.LoanTerm;
        loan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        var updated = await _context.Loans
            .AsNoTracking()
            .Include(l => l.Borrower)
            .FirstAsync(l => l.Id == id, ct);

        return Result<LoanDto>.Success(MapToLoanDto(updated));
    }

    private static LoanDto MapToLoanDto(Loan loan) =>
        new()
        {
            Id = loan.Id,
            LoanCode = loan.LoanCode,
            BorrowerId = loan.BorrowerId,
            LoanAmount = loan.LoanAmount,
            InterestRate = loan.InterestRate,
            LoanTerm = loan.LoanTerm,
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
                    Address = loan.Borrower.Address
                }
        };
}
