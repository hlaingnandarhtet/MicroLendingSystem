using microlending_API.Features.Loans;
using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using MicroLendingSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using microlending_API.Features.Borrowers;

namespace microlending_API.Features.LoanSettings
{
    public class LoanSettingService:ILoanSettingService
    {
        private readonly AppDbContext _context;

        public LoanSettingService(AppDbContext context) => _context = context;

        public async Task<PagedResult<PagedPayload<LoanSettingDto>>> GetLoanSettingsAsync(GetLoanSettingsRequest request, CancellationToken ct)
        {
            if (request.Page < 1 || request.PageSize < 1)
            {
                return PagedResult<PagedPayload<LoanSettingDto>>.Failure("Invalid pagination parameters.", 400);
            }

            // Only surface active (non-versioned-out) settings for new loan applications.
            var query = _context.LoanSettings
                .AsNoTracking()
                .Where(s => s.IsDeleted != true && s.IsActive == true);

            if (!string.IsNullOrWhiteSpace(request.PlanName))
            {
                query = query.Where(s => s.PlanName.Contains(request.PlanName));
            }

            var total = await query.CountAsync(ct);
            var entities = await query
                .OrderByDescending(s => s.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var items = entities.Select(MapLoanSettingToDto).ToList();

            var payload = new PagedPayload<LoanSettingDto>
            {
                Items = items,
                TotalCount = total,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };

            return PagedResult<PagedPayload<LoanSettingDto>>.Success(payload);
        }

        public async Task<Result<LoanSettingDto>> GetLoanSettingByIdAsync(int id, CancellationToken ct)
        {
            var entity = await _context.LoanSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted != true, ct);

            return entity is null
                ? Result<LoanSettingDto>.Failure("Loan setting not found.", 404)
                : Result<LoanSettingDto>.Success(MapLoanSettingToDto(entity));
        }

        public async Task<Result<LoanSettingDto>> CreateLoanSettingAsync(CreateLoanSettingRequest request, CancellationToken ct)
        {
            if (!Enum.IsDefined(typeof(CalculationType), request.CalculationType))
            {
                return Result<LoanSettingDto>.Failure("Invalid calculation type.", 400);
            }

            var now = DateTime.UtcNow;
            var entity = new LoanSetting
            {
                PlanName = request.PlanName,
                InterestRate = request.InterestRate,
                LoanTerm = request.LoanTerm,
                CalculationType = (int)request.CalculationType,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false,
                IsActive = request.IsActive,
            };

            _context.LoanSettings.Add(entity);
            await _context.SaveChangesAsync(ct);

            return Result<LoanSettingDto>.Success(MapLoanSettingToDto(entity));
        }

        public async Task<Result<LoanSettingDto>> UpdateLoanSettingAsync(int id, UpdateLoanSettingRequest request, CancellationToken ct)
        {
            if (!Enum.IsDefined(typeof(CalculationType), request.CalculationType))
            {
                return Result<LoanSettingDto>.Failure("Invalid calculation type.", 400);
            }

            var existing = await _context.LoanSettings
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted != true, ct);

            if (existing is null)
            {
                return Result<LoanSettingDto>.Failure("Loan setting not found.", 404);
            }

            var now = DateTime.UtcNow;

            // --- Versioning: deactivate the current record instead of mutating it ---
            existing.IsActive = false;
            existing.UpdatedAt = now;
            // IsDeleted stays false so existing loans still resolve their FK.

            // Create the new version as the active record.
            var newVersion = new LoanSetting
            {
                PlanName = request.PlanName,
                InterestRate = request.InterestRate,
                LoanTerm = request.LoanTerm,
                CalculationType = (int)request.CalculationType,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false,
                IsActive = true
            };

            _context.LoanSettings.Add(newVersion);
            await _context.SaveChangesAsync(ct);

            return Result<LoanSettingDto>.Success(MapLoanSettingToDto(newVersion));
        }

        public async Task<Result<bool>> DeleteLoanSettingAsync(int id, CancellationToken ct)
        {
            var entity = await _context.LoanSettings.FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted != true, ct);
            if (entity is null)
            {
                return Result<bool>.Failure("Loan setting not found.", 404);
            }

            var inUse = await _context.Loans.AnyAsync(l => l.LoanSettingId == id && l.IsDeleted != true, ct);
            if (inUse)
            {
                return Result<bool>.Failure("Cannot delete a loan setting that is assigned to one or more loans.", 409);
            }

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return Result<bool>.Success(true);
        }

        private static LoanSettingDto MapLoanSettingToDto(LoanSetting s) =>
        new()
        {
            Id = s.Id,
            PlanName = s.PlanName,
            InterestRate = s.InterestRate,
            LoanTerm = s.LoanTerm,
            CalculationType = (CalculationType)s.CalculationType,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            IsActive = s.IsActive ?? false
        };
    }
}
