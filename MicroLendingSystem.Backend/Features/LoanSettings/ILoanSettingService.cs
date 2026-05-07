using MicroLendingSystem.Shared.Models;

namespace microlending_API.Features.LoanSettings
{
    public interface ILoanSettingService
    {
        Task<PagedResult<PagedPayload<LoanSettingDto>>> GetLoanSettingsAsync(GetLoanSettingsRequest request, CancellationToken ct);
        Task<Result<LoanSettingDto>> GetLoanSettingByIdAsync(int id, CancellationToken ct);
        Task<Result<LoanSettingDto>> CreateLoanSettingAsync(CreateLoanSettingRequest request, CancellationToken ct);
        Task<Result<LoanSettingDto>> UpdateLoanSettingAsync(int id, UpdateLoanSettingRequest request, CancellationToken ct);
        Task<Result<bool>> DeleteLoanSettingAsync(int id, CancellationToken ct);
    }
}
