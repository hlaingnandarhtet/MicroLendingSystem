using MicroLendingSystem.Database.Models;
using Microlending.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppDb = MicroLendingSystem.Database.AppDbContext.AppDbContext;

namespace microlending_API.Features.Borrowers;

[ApiController]
[Route("api/borrowers/")]
public class BorrowerController : ControllerBase
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private readonly AppDb _context;

    public BorrowerController(AppDb context)
    {
        _context = context;
    }

    /// GET DATA FROM DB WITH PAGINATION
    
    [HttpGet]
    [ProducesResponseType(typeof(BorrowersPagedResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BorrowersPagedResponse>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,

        // ERROR VALIDATION

        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            ModelState.AddModelError(nameof(page), "Page must be at least 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            ModelState.AddModelError(nameof(pageSize), $"Page size must be between 1 and {MaxPageSize}.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var query = _context.Borrowers.AsNoTracking().Where(b => b.IsDeleted != true);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var response = new BorrowersPagedResponse
        {
            Items = items, // real data from db
            Pagination = new PaginationMetadata
            {
                TotalCount = total,
                PageSize = pageSize,
                CurrentPage = page
            }
        };

        return Ok(response);
    }

    /// GET BY ID
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Borrower), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Borrower>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var borrower = await _context.Borrowers.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, cancellationToken);

        if (borrower is null)
        {
            return NotFound();
        }

        return Ok(borrower);
    }

    /// Edit existing borrower
    [HttpGet("{id:int}/edit")]
    [ProducesResponseType(typeof(Borrower), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Borrower>> Edit(int id, CancellationToken cancellationToken = default)
    {
        var borrower = await _context.Borrowers.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, cancellationToken);
        if (borrower is null)
        {
            return NotFound();
        }
        return Ok(borrower);
    }

    /// CREATE NEW BORROWER
    [HttpPost("create")]
    [ProducesResponseType(typeof(Borrower), StatusCodes.Status201Created)] // FOR SWAGGER 
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Borrower>> Create([FromBody] Borrower model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {   
            return BadRequest(ModelState);
        }

        var now = DateTime.UtcNow;
        var entity = new Borrower
        {
            FullName = model.FullName,
            UserName = model.UserName,
            Nrcno = model.Nrcno,
            PhoneNo = model.PhoneNo,
            Address = model.Address,
            DocumentId = model.DocumentId,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        _context.Borrowers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    /// Update existing borrower
    [HttpPut("{id}/update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Borrower model, CancellationToken cancellationToken = default)
    {
        if (id != model.Id)
        {
            return BadRequest("Route id does not match payload id.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        entity.FullName = model.FullName;
        entity.UserName = model.UserName;
        entity.Nrcno = model.Nrcno;
        entity.PhoneNo = model.PhoneNo;
        entity.Address = model.Address;
        entity.DocumentId = model.DocumentId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// DELETE BORROWER
    [HttpDelete("{id:int}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Borrowers
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted != true, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public class BorrowersPagedResponse
{
    public IReadOnlyList<Borrower> Items { get; set; } = Array.Empty<Borrower>();

    public PaginationMetadata Pagination { get; set; } = null!;
}
