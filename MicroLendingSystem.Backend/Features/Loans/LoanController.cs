using Microsoft.AspNetCore.Mvc;

namespace microlending_API.Features.Loans;

[ApiController]
[Route("api/systems/loans")]
public class LoanController : ControllerBase
{
    private readonly ILoanService _service;
    public LoanController(ILoanService service) => _service = service;

    // Get Loans with pagination, filtering, and search
    [HttpGet]
    public async Task<IActionResult> GetLoans([FromQuery] GetLoansRequest request, CancellationToken ct)
    {
        return Ok(await _service.GetLoansPagedAsync(request, ct));
    }

    // Apply for a new loan
    [HttpPost("create")]
    public async Task<IActionResult> CreateLoans([FromBody] CreateLoanRequest request, CancellationToken ct)
    {
        var result = await _service.CreateLoansAsync(request, ct);
        if (!result.IsSuccess)
        {
            // borrower exit or not 
            return BadRequest(new { Message = result.Error });
        }

        return Ok(result.Value);
        //return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // Approve a loan application
    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> ApproveLoans(int id, CancellationToken ct)
    {
        var result = await _service.ApproveLoansAsync(id, ct);
        return result.IsSuccess ? Ok(new { Message = "Loan activated" }) : BadRequest(result.Error);
    }

    // Get loan details by ID
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetLoansById(int id, CancellationToken ct)
    {
        var loan = await _service.GetLoansByIdAsync(id, ct);
        return loan != null ? Ok(loan) : NotFound();
    }

    // Update loan Status
    [HttpPut("{id}/update_status")]
    public async Task<IActionResult> UpdateLoanStatus(int id, [FromBody] UpdateLoanStatusRequest request, CancellationToken ct)
    {
        var updatedLoan = await _service.UpdateLoansStatusAsync(id, request.NewStatus, ct);
        return updatedLoan != null ? Ok(updatedLoan) : NotFound();
    }

    [HttpPut("{id}/update_details")]
    public async Task<IActionResult> UpdateDetails(int id, [FromBody] UpdateLoanDataRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateLoansAsync(id, request, ct);
        return result.IsSuccess ? Ok(new { Message = "Loan updated successfully" }) : BadRequest(result.Error);
    }

    // Delete a loan by ID
    [HttpDelete("{id}/delete")]
    public async Task<IActionResult> DeleteLoans(int id, CancellationToken ct)
    {
        var success = await _service.DeleteLoansAsync(id, ct);
        return success ? Ok(new { Message = "Loan deleted" }) : NotFound();
    }
}

public class UpdateLoanStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
}