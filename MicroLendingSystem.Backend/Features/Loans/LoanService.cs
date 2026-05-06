using Microsoft.EntityFrameworkCore;
using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using microlending_API.Features.Loans.Constants;

namespace microlending_API.Features.Loans;

public class LoanService : ILoanService
{
    private readonly AppDbContext _context;
    public LoanService(AppDbContext context) => _context = context;

    // Create Loan
    public async Task<Result<LoanDto>> CreateLoansAsync(CreateLoanRequest request, CancellationToken ct)
    {
        // borrower exist or not
        var borrowerExists = await _context.Borrowers.AnyAsync(b => b.Id == request.BorrowerId, ct);
        if (!borrowerExists)
        {
            return Result<LoanDto>.Failure($"Borrowers with ID {request.BorrowerId} was not found.");
        }

        // Auto Generate Loan Code (LN-202605-001)
        var datePart = DateTime.UtcNow.ToString("yyyyMM");
        var count = await _context.Loans.CountAsync(ct) + 1;

        // create new loan
        var loan = new Loan
        {
            BorrowerId = request.BorrowerId,
            LoanCode = $"LN-{datePart}-{count:D3}",
            LoanAmount = request.LoanAmount,
            InterestRate = request.InterestRate,
            LoanTerm = request.LoanTerm,
            Status = LoanStatus.Pending.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(ct);

        return Result<LoanDto>.Success(new LoanDto
        {
            Id = loan.Id,
            LoanCode = loan.LoanCode,
            Status = loan.Status
        });
    }

    // Approve Loan
    public async Task<Result<bool>> ApproveLoansAsync(int id, CancellationToken ct)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan == null) return Result<bool>.Failure("Loan not found.");

        loan.Status = LoanStatus.Active.ToString();
        loan.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        loan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    // Get Paged Loans
    public async Task<object> GetLoansPagedAsync(GetLoansRequest request, CancellationToken ct)
    {
        var query = _context.Loans.AsNoTracking().Where(l => l.IsDeleted != true);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value.ToString());

        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.Where(l => l.LoanCode.Contains(request.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                LoanCode = l.LoanCode,
                LoanAmount = l.LoanAmount,
                Status = l.Status ?? "Pending",
                CreatedAt = l.CreatedAt
            }).ToListAsync(ct);

        return new { Items = items, TotalCount = total, CurrentPage = request.Page };
    }

    // Get Loan Details
    public async Task<LoanDetails?> GetLoansByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Loans
            .AsNoTracking()
            .Where(l => l.Id == id && l.IsDeleted != true)
            .Select(l => new LoanDetails
            {
                Id = l.Id,
                LoanCode = l.LoanCode,
                LoanAmount = l.LoanAmount,
                Status = l.Status ?? "Pending",
                BorrowerId = l.BorrowerId,
                InterestRate = l.InterestRate,
                LoanTerm = l.LoanTerm
            }).FirstOrDefaultAsync(ct);
    }

    // Update Status
    public async Task<Loan?> UpdateLoansStatusAsync(int id, string newStatus, CancellationToken ct)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan == null)
            return null;

        loan.Status = newStatus;
        loan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return loan;
    }

    // Delete Loan
    public async Task<bool> DeleteLoansAsync(int id, CancellationToken ct)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);
        if (loan == null)
            return false;

        loan.IsDeleted = true;
        loan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // Update Loan
    public async Task<Result<Loan?>> UpdateLoansAsync(int id, UpdateLoanDataRequest request, CancellationToken ct)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == id && l.IsDeleted != true, ct);

        if (loan == null) return Result<Loan?>.Failure("Loan not found.");
        if (loan.Status != "Pending")
        {
            return Result<Loan?>.Failure("Cannot update loan data because it is already approved or active.");
        }

        loan.LoanAmount = request.LoanAmount;
        loan.InterestRate = request.InterestRate;
        loan.LoanTerm = request.LoanTerm;
        loan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<Loan?>.Success(loan);
    }

}