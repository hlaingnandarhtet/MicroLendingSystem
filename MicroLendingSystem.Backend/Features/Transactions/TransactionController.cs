using MicroLendingSystem.Backend.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace microlending_API.Features.Transactions;

[ApiController]
[Route("api/transactions")]
[Authorize]
public sealed class TransactionController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionController(ITransactionService service) => _service = service;

    [HttpGet]
    [HasPermission(PermissionNames.Transaction_List)]
    public async Task<IActionResult> GetTransactions([FromQuery] GetTransactionsRequest request, CancellationToken ct)
    {
        var result = await _service.GetTransactionsAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }
}
