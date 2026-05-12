using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Backend.Authorization;
using MicroLendingSystem.Backend.Features.Permissions;

namespace MicroLendingSystem.Backend.Features.Permissions;

[ApiController]
[Route("api/permissions")]
[Authorize]
public sealed class PermissionController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionController(IPermissionService service) => _service = service;

    [HasPermission(PermissionNames.Permission_Read)]
    [HttpGet]
    public async Task<IActionResult> GetPermissions([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _service.GetPermissionsAsync(page, pageSize, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Permission_Read)]
    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Permission_Create)]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePermissionRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Permission_Update)]
    [HttpPut("{id:int}/update")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Permission_Delete)]
    [HttpDelete("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }
}
