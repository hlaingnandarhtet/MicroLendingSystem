using Microsoft.AspNetCore.Mvc;

namespace microlending_API.Features.Borrowers;

[ApiController]
[Route("api/borrowers")]
public class BorrowerController : ControllerBase
{
    private readonly IBorrowerService _service;

    public BorrowerController(IBorrowerService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetBorrowers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _service.GetBorrowersAsync(page, pageSize, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> GetBorrowersById(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost("create")]
    public async Task<IActionResult> BorrowersCreate([FromBody] CreateBorrowerRequest request, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("{id}/update")]
    public async Task<IActionResult> BorrowersUpdate(int id, [FromBody] UpdateBorrowerRequest request, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpDelete("{id:int}/delete")]
    public async Task<IActionResult> BorrowersDelete(int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }
}
