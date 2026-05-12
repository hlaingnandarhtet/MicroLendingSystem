using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Frontend.Models;
using MicroLendingSystem.Frontend.ViewModels;

namespace MicroLendingSystem.Frontend.Controllers;

[Authorize]
public class RolePermissionController : Controller
{
    private readonly HttpClient _http;

    public RolePermissionController(IHttpClientFactory factory)
        => _http = factory.CreateClient("BackendApi");

    public async Task<IActionResult> Index(
        int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        ViewData["Title"] = "Role Permissions";
        ViewData["ActivePage"] = "Roles";

        var result = await _http.GetFromJsonAsync<PagedPayload<RoleSummaryDto>>(
            $"api/roles?page={page}&pageSize={pageSize}", ct);

        var vm = new PagedViewModel<RoleSummaryDto>();
        if (result != null)
        {
            vm.Items = result.Items;
            vm.TotalCount = result.TotalCount;
            vm.CurrentPage = result.CurrentPage;
            vm.PageSize = result.PageSize;
        }
        return View(vm);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Role";
        ViewData["ActivePage"] = "Roles";
        return View(new RoleFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Role";
        ViewData["ActivePage"] = "Roles";

        var response = await _http.PostAsJsonAsync("api/roles/create",
            new CreateRoleRequest { Name = model.Name }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Role created.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Role";
        ViewData["ActivePage"] = "Roles";

        var response = await _http.GetAsync($"api/roles/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Index");
        }
        var role = await response.Content.ReadFromJsonAsync<RoleDetailDto>(cancellationToken: ct);
        if (role is null) return RedirectToAction("Index");

        return View(new RoleFormViewModel { Id = role.Id, Name = role.Name });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, RoleFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Role";
        ViewData["ActivePage"] = "Roles";

        var response = await _http.PutAsJsonAsync($"api/roles/{id}/update",
            new UpdateRoleRequest { Name = model.Name }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Role updated.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Delete Role";
        ViewData["ActivePage"] = "Roles";

        var response = await _http.GetAsync($"api/roles/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Index");
        }
        var role = await response.Content.ReadFromJsonAsync<RoleDetailDto>(cancellationToken: ct);
        return View(role);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var response = await _http.DeleteAsync($"api/roles/{id}/delete", ct);
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Role deleted.";
        else
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> AssignPermissions(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Assign Permissions";
        ViewData["ActivePage"] = "Roles";

        var roleResponse = await _http.GetAsync($"api/roles/{id}/details", ct);
        if (!roleResponse.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Index");
        }
        var role = await roleResponse.Content.ReadFromJsonAsync<RoleDetailDto>(cancellationToken: ct);

        var permsResult = await _http.GetFromJsonAsync<PagedPayload<PermissionDto>>(
            "api/permissions?page=1&pageSize=500", ct);
        var allPerms = permsResult?.Items ?? new List<PermissionDto>();

        return View(new AssignPermissionsViewModel
        {
            RoleId = id,
            RoleName = role!.Name,
            Permissions = allPerms.Select(p => new PermissionCheckItem
            {
                Id = p.Id,
                Name = p.Name,
                IsSelected = role.Permissions.Contains(p.Name)
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> AssignPermissions(
        int id, AssignPermissionsViewModel model, CancellationToken ct)
    {
        var selectedIds = model.Permissions
            .Where(p => p.IsSelected)
            .Select(p => p.Id)
            .ToList();

        var response = await _http.PutAsJsonAsync($"api/roles/{id}/permissions",
            new AssignPermissionsToRoleRequest { PermissionIds = selectedIds }, ct);

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Permissions assigned.";
        else
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);

        return RedirectToAction("Index");
    }
}
