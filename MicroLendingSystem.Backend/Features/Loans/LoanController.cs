using Microsoft.AspNetCore.Mvc;

namespace microlending_API.Features.Loans;

[ApiController]
[Route("api/systems/loans")]
public class LoanController : ControllerBase
{
    private readonly ILoanService _service;

    public LoanController(ILoanService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetLoan([FromQuery] GetLoansRequest request, CancellationToken ct)
    {
        var result = await _service.GetLoanAsync(request, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Data);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateLoan([FromBody] CreateLoanRequest request, CancellationToken ct)
    {
        var result = await _service.CreateLoanAsync(request, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Data);
    }

    [HttpPatch("{id}/approve-reject")]
    public async Task<IActionResult> ApproveRejectLoan(int id, [FromQuery] int statusId, CancellationToken ct)
    {
        var result = await _service.ApproveRejectLoanAsync(id, statusId, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetLoansById(int id, CancellationToken ct)
    {
        var result = await _service.GetLoanByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Data);
    }

    [HttpPut("{id}/update_status")]
    public async Task<IActionResult> UpdateLoanStatus(int id, [FromBody] UpdateLoanStatusRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateLoanStatusAsync(id, request, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Data);
    }

    [HttpPut("{id}/update_details")]
    public async Task<IActionResult> UpdateLoanDetails(int id, [FromBody] UpdateLoanDataRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateLoanAsync(id, request, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}/delete")]
    public async Task<IActionResult> DeleteLoan(int id, CancellationToken ct)
    {
        var result = await _service.DeleteLoanAsync(id, ct);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Data);
    }
}
