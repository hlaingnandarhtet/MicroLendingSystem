using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Frontend.Models;
using MicroLendingSystem.Frontend.ViewModels;

namespace MicroLendingSystem.Frontend.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly HttpClient _http;

    public UserController(IHttpClientFactory factory)
        => _http = factory.CreateClient("BackendApi");

    public async Task<IActionResult> Index(
        int page = 1, int pageSize = 10,
        string? name = null, string? email = null, string? role = null,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Users";
        ViewData["ActivePage"] = "Users";

        var url = $"api/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(name)) url += $"&name={Uri.EscapeDataString(name)}";
        if (!string.IsNullOrWhiteSpace(email)) url += $"&email={Uri.EscapeDataString(email)}";
        if (!string.IsNullOrWhiteSpace(role)) url += $"&role={Uri.EscapeDataString(role)}";

        var result = await _http.GetFromJsonAsync<PagedPayload<UserDto>>(url, ct);

        var vm = new PagedViewModel<UserDto>();
        if (result != null)
        {
            vm.Items = result.Items;
            vm.TotalCount = result.TotalCount;
            vm.CurrentPage = result.CurrentPage;
            vm.PageSize = result.PageSize;
        }
        ViewBag.Name = name;
        ViewBag.Email = email;
        ViewBag.Role = role;
        return View(vm);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "Add User";
        ViewData["ActivePage"] = "Users";
        return View(await BuildUserFormAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Add User";
        ViewData["ActivePage"] = "Users";

        var response = await _http.PostAsJsonAsync("api/users/create",
            new CreateUserRequest
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password ?? string.Empty,
                RoleId = model.RoleId
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            model.Roles = await GetRolesAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "User created successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit User";
        ViewData["ActivePage"] = "Users";

        var response = await _http.GetAsync($"api/users/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }
        var u = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct);
        if (u is null) return RedirectToAction("Index");

        return View(new UserFormViewModel
        {
            Id = u.Id, Name = u.Name, Email = u.Email, RoleId = u.RoleId,
            Roles = await GetRolesAsync(ct)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit User";
        ViewData["ActivePage"] = "Users";

        // Update user details
        var updateResponse = await _http.PutAsJsonAsync($"api/users/{id}/update",
            new UpdateUserRequest
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password
            }, ct);

        if (!updateResponse.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await updateResponse.Content.ReadAsStringAsync(ct);
            model.Roles = await GetRolesAsync(ct);
            return View(model);
        }

        // Assign role if changed
        if (model.RoleId.HasValue)
        {
            await _http.PutAsJsonAsync($"api/users/{id}/assign-role",
                new AssignUserRoleRequest { RoleId = model.RoleId.Value }, ct);
        }

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Delete User";
        ViewData["ActivePage"] = "Users";

        var response = await _http.GetAsync($"api/users/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }
        var u = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct);
        return View(u);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var response = await _http.DeleteAsync($"api/users/{id}/delete", ct);
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "User deleted.";
        else
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);

        return RedirectToAction("Index");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private async Task<UserFormViewModel> BuildUserFormAsync(CancellationToken ct) =>
        new() { Roles = await GetRolesAsync(ct) };

    private async Task<List<RoleSummaryDto>> GetRolesAsync(CancellationToken ct)
    {
        var result = await _http.GetFromJsonAsync<PagedPayload<RoleSummaryDto>>(
            "api/roles?page=1&pageSize=100", ct);
        return result?.Items ?? new List<RoleSummaryDto>();
    }
}
