using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using MicroLendingSystem.Backend.Authorization;



namespace microlending_API.Features.Loans;



[ApiController]

[Route("api/loans")]

[Authorize]

public class LoanController : ControllerBase

{

    private readonly ILoanService _service;



    public LoanController(ILoanService service) => _service = service;



    [HttpGet]

    [HasPermission(PermissionNames.LoanRequest_List)]

    public async Task<IActionResult> GetLoan([FromQuery] GetLoansRequest request, CancellationToken ct)

    {

        var result = await _service.GetLoanAsync(request, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }



    [HttpPost("create")]

    [HasPermission(PermissionNames.Loan_Create)]

    public async Task<IActionResult> CreateLoan([FromBody] CreateLoanRequest request, CancellationToken ct)

    {

        var result = await _service.CreateLoanAsync(request, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }



    [HttpPatch("{id}/approve-reject")]

    [HasPermission(PermissionNames.Loan_Approve)]

    public async Task<IActionResult> ApproveRejectLoan(int id, [FromQuery] int statusId, CancellationToken ct)

    {

        var result = await _service.ApproveRejectLoanAsync(id, statusId, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }



    [HttpPost("{id}/repay")]

    [HasPermission(PermissionNames.Loan_Repay)]

    public async Task<IActionResult> RepayLoan(int id, [FromBody] RepayLoanRequest request, CancellationToken ct)

    {

        var result = await _service.RepayLoanAsync(id, request, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }



    [HttpGet("{id}/details")]

    [HasPermission(PermissionNames.Loan_Read)]

    public async Task<IActionResult> GetLoansById(int id, CancellationToken ct)

    {

        var result = await _service.GetLoanByIdAsync(id, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }



    [HttpPut("{id}/update_status")]

    [HasPermission(PermissionNames.Loan_Update)]

    public async Task<IActionResult> UpdateLoanStatus(int id, [FromBody] UpdateLoanStatusRequest request, CancellationToken ct)

    {

        var result = await _service.UpdateLoanStatusAsync(id, request, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }



    [HttpPut("{id}/update_details")]

    [HasPermission(PermissionNames.Loan_Update)]

    public async Task<IActionResult> UpdateLoanDetails(int id, [FromBody] UpdateLoanDataRequest request, CancellationToken ct)

    {

        var result = await _service.UpdateLoanAsync(id, request, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }



    [HttpDelete("{id}/delete")]

    [HasPermission(PermissionNames.Loan_Delete)]

    public async Task<IActionResult> DeleteLoan(int id, CancellationToken ct)

    {

        var result = await _service.DeleteLoanAsync(id, ct);

        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);

    }

}

