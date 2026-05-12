using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Backend.Authorization;
using MicroLendingSystem.Backend.Features.Permissions;

namespace MicroLendingSystem.Backend.Features.Roles;

[ApiController]
[Route("api/roles")]
[Authorize]
public sealed class RoleController : ControllerBase
{
    private readonly IRoleService _service;

    public RoleController(IRoleService service) => _service = service;

    [HasPermission(PermissionNames.Role_Read)]
    [HttpGet]
    public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _service.GetRolesAsync(page, pageSize, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Role_Read)]
    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Role_Create)]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Role_Update)]
    [HttpPut("{id:int}/update")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Role_Delete)]
    [HttpDelete("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HasPermission(PermissionNames.Role_AssignPermissions)]
    [HttpPut("{id:int}/permissions")]
    public async Task<IActionResult> AssignPermissions(int id, [FromBody] AssignPermissionsToRoleRequest request, CancellationToken ct)
    {
        var result = await _service.AssignPermissionsAsync(id, request, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }
}
