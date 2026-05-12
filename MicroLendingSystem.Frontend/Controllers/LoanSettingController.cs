using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Frontend.Models;
using MicroLendingSystem.Frontend.ViewModels;

namespace MicroLendingSystem.Frontend.Controllers;

[Authorize]
public class LoanSettingController : Controller
{
    private readonly HttpClient _http;

    public LoanSettingController(IHttpClientFactory factory)
        => _http = factory.CreateClient("BackendApi");

    public async Task<IActionResult> Index(
        int page = 1, int pageSize = 10, string? planName = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "Loan Settings";
        ViewData["ActivePage"] = "LoanSettings";

        var url = $"api/loan-settings?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(planName))
            url += $"&planName={Uri.EscapeDataString(planName)}";

        var result = await _http.GetFromJsonAsync<PagedPayload<LoanSettingDto>>(url, ct);

        var vm = new PagedViewModel<LoanSettingDto>();
        if (result != null)
        {
            vm.Items = result.Items;
            vm.TotalCount = result.TotalCount;
            vm.CurrentPage = result.CurrentPage;
            vm.PageSize = result.PageSize;
        }
        ViewBag.PlanName = planName;
        return View(vm);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Add Loan Setting";
        ViewData["ActivePage"] = "LoanSettings";
        return View(new LoanSettingFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(LoanSettingFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Add Loan Setting";
        ViewData["ActivePage"] = "LoanSettings";

        var response = await _http.PostAsJsonAsync("api/loan-settings/create",
            new CreateLoanSettingRequest
            {
                PlanName = model.PlanName,
                InterestRate = model.InterestRate,
                LoanTerm = model.LoanTerm,
                IsActive = model.IsActive,
                CalculationType = model.CalculationType
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Loan setting created.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Loan Setting";
        ViewData["ActivePage"] = "LoanSettings";

        var response = await _http.GetAsync($"api/loan-settings/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Loan setting not found.";
            return RedirectToAction("Index");
        }
        var d = await response.Content.ReadFromJsonAsync<LoanSettingDto>(cancellationToken: ct);
        if (d is null) return RedirectToAction("Index");

        return View(new LoanSettingFormViewModel
        {
            Id = d.Id, PlanName = d.PlanName, InterestRate = d.InterestRate,
            LoanTerm = d.LoanTerm, IsActive = d.IsActive, CalculationType = d.CalculationType
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, LoanSettingFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Loan Setting";
        ViewData["ActivePage"] = "LoanSettings";

        var response = await _http.PutAsJsonAsync($"api/loan-settings/{id}/update",
            new UpdateLoanSettingRequest
            {
                PlanName = model.PlanName,
                InterestRate = model.InterestRate,
                LoanTerm = model.LoanTerm,
                IsActive = model.IsActive,
                CalculationType = model.CalculationType
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Loan setting updated.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Delete Loan Setting";
        ViewData["ActivePage"] = "LoanSettings";

        var response = await _http.GetAsync($"api/loan-settings/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Loan setting not found.";
            return RedirectToAction("Index");
        }
        var d = await response.Content.ReadFromJsonAsync<LoanSettingDto>(cancellationToken: ct);
        return View(d);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var response = await _http.DeleteAsync($"api/loan-settings/{id}/delete", ct);
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Loan setting deleted.";
        else
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);

        return RedirectToAction("Index");
    }
}
