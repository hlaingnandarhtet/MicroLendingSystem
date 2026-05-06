using Microlending.Shared.Models;
using MicroLendingSystem.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace microlending_API.Features.Borrowers;

[ApiController]
[Route("api/systems/borrowers")]
public class BorrowerController : ControllerBase
{
    private readonly IBorrowerService _service;

    public BorrowerController(IBorrowerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<BorrowersPagedResponse>> GetBorrowers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1 || pageSize < 1) return BadRequest("Invalid pagination parameters.");
        return Ok(await _service.GetBorrowersAsync(page, pageSize, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Borrower>> GetBorrowersById(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("create")]
    public async Task<ActionResult<Borrower>> BorrowersCreate([FromBody] Borrower model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _service.CreateAsync(model, ct);
        return CreatedAtAction(nameof(GetBorrowersById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}/update")]
    public async Task<IActionResult> BorrowersUpdate(int id, [FromBody] Borrower model, CancellationToken ct = default)
    {
        if (id != model.Id) return BadRequest("ID mismatch");

        var result = await _service.UpdateAsync(id, model, ct);
        return result is not null ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}/delete")]
    public async Task<IActionResult> BorrowersDelete(int id, CancellationToken ct = default)
    {
        var success = await _service.DeleteAsync(id, ct);
        return success ? NoContent() : NotFound();
    }
}

public class BorrowersPagedResponse
{
    public IReadOnlyList<Borrower> Items { get; set; } = Array.Empty<Borrower>();
    public PaginationMetadata Pagination { get; set; } = null!;
}