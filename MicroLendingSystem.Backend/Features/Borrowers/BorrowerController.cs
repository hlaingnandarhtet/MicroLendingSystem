using MicroLendingSystem.Backend.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace microlending_API.Features.Borrowers;

[ApiController]
[Route("api/borrowers")]
[Authorize]
public class BorrowerController : ControllerBase
{
    private readonly IBorrowerService _service;

    public BorrowerController(IBorrowerService service) => _service = service;

    [HttpGet]
    [HasPermission(PermissionNames.Borrower_Read)]
    public async Task<IActionResult> GetBorrowers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? fullName = null,
        [FromQuery] string? userName = null,
        [FromQuery] string? phoneNo = null,
        [FromQuery] string? nrcNo = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetBorrowersAsync(page, pageSize, fullName, userName, phoneNo, nrcNo, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("{id:int}/details")]
    [HasPermission(PermissionNames.Borrower_Read)]
    public async Task<IActionResult> GetBorrowersById(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost("create")]
    [HasPermission(PermissionNames.Borrower_Create)]
    public async Task<IActionResult> BorrowersCreate([FromBody] CreateBorrowerRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("{id}/update")]
    [HasPermission(PermissionNames.Borrower_Update)]
    public async Task<IActionResult> BorrowersUpdate(int id, [FromBody] UpdateBorrowerRequest request, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpDelete("{id:int}/delete")]
    [HasPermission(PermissionNames.Borrower_Delete)]
    public async Task<IActionResult> BorrowersDelete(int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }
}
