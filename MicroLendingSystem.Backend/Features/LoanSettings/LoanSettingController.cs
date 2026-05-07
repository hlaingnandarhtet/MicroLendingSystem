using microlending_API.Features.Loans;
using Microsoft.AspNetCore.Mvc;

namespace microlending_API.Features.LoanSettings;

[ApiController]
[Route("api/loan-settings")]
public class LoanSettingController : ControllerBase
{
    private readonly ILoanSettingService _service;

    public LoanSettingController(ILoanSettingService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetLoanSettings([FromQuery] GetLoanSettingsRequest request, CancellationToken ct)
    {
        var result = await _service.GetLoanSettingsAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> GetLoanSettingById(int id, CancellationToken ct)
    {
        var result = await _service.GetLoanSettingByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateLoanSetting([FromBody] CreateLoanSettingRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateLoanSettingAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("{id:int}/update")]
    public async Task<IActionResult> UpdateLoanSetting(int id, [FromBody] UpdateLoanSettingRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateLoanSettingAsync(id, request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpDelete("{id:int}/delete")]
    public async Task<IActionResult> DeleteLoanSetting(int id, CancellationToken ct)
    {
        var result = await _service.DeleteLoanSettingAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }
}
