using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Frontend.Models;
using MicroLendingSystem.Frontend.ViewModels;

namespace MicroLendingSystem.Frontend.Controllers;

[Authorize]
public class LoanController : Controller
{
    private readonly HttpClient _http;

    public LoanController(IHttpClientFactory factory)
        => _http = factory.CreateClient("BackendApi");

    public async Task<IActionResult> Index(
        int page = 1, int pageSize = 10,
        string? loanCode = null, string? planName = null, string? borrowerName = null,
        LoanStatus? status = null,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Loans";
        ViewData["ActivePage"] = "Loans";

        var url = $"api/loans?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(loanCode)) url += $"&loanCode={Uri.EscapeDataString(loanCode)}";
        if (!string.IsNullOrWhiteSpace(planName)) url += $"&planName={Uri.EscapeDataString(planName)}";
        if (!string.IsNullOrWhiteSpace(borrowerName)) url += $"&borrowerName={Uri.EscapeDataString(borrowerName)}";
        if (status.HasValue) url += $"&status={(int)status.Value}";

        var result = await _http.GetFromJsonAsync<PagedPayload<LoanDto>>(url, ct);

        var vm = new LoanIndexViewModel { StatusFilter = status };
        if (result != null)
        {
            vm.Items = result.Items;
            vm.TotalCount = result.TotalCount;
            vm.CurrentPage = result.CurrentPage;
            vm.PageSize = result.PageSize;
        }
        ViewBag.LoanCode = loanCode;
        ViewBag.PlanName = planName;
        ViewBag.BorrowerName = borrowerName;
        return View(vm);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewData["Title"] = "Create Loan";
        ViewData["ActivePage"] = "Loans";
        return View(await BuildLoanFormAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(LoanFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Create Loan";
        ViewData["ActivePage"] = "Loans";

        var response = await _http.PostAsJsonAsync("api/loans/create",
            new CreateLoanRequest
            {
                BorrowerId = model.BorrowerId,
                LoanAmount = model.LoanAmount,
                LoanSettingId = model.LoanSettingId
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            var vm = await BuildLoanFormAsync(ct);
            vm.BorrowerId = model.BorrowerId;
            vm.LoanAmount = model.LoanAmount;
            vm.LoanSettingId = model.LoanSettingId;
            return View(vm);
        }
        TempData["SuccessMessage"] = "Loan created successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Loan Details";
        ViewData["ActivePage"] = "Loans";

        var response = await _http.GetAsync($"api/loans/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Loan not found.";
            return RedirectToAction("Index");
        }
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>(cancellationToken: ct);
        return View(loan);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Loan";
        ViewData["ActivePage"] = "Loans";

        var response = await _http.GetAsync($"api/loans/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Loan not found.";
            return RedirectToAction("Index");
        }
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>(cancellationToken: ct);
        var settings = await _http.GetFromJsonAsync<PagedPayload<LoanSettingDto>>(
            "api/loan-settings?page=1&pageSize=100", ct);

        return View(new LoanEditViewModel
        {
            Id = loan!.Id,
            LoanAmount = loan.LoanAmount,
            LoanSettingId = loan.LoanSettingId,
            LoanSettings = settings?.Items ?? new List<LoanSettingDto>()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, LoanEditViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Loan";
        ViewData["ActivePage"] = "Loans";

        var response = await _http.PutAsJsonAsync($"api/loans/{id}/update_details",
            new UpdateLoanDataRequest
            {
                LoanAmount = model.LoanAmount,
                LoanSettingId = model.LoanSettingId
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Loan updated successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Delete Loan";
        ViewData["ActivePage"] = "Loans";

        var response = await _http.GetAsync($"api/loans/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Loan not found.";
            return RedirectToAction("Index");
        }
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>(cancellationToken: ct);
        return View(loan);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var response = await _http.DeleteAsync($"api/loans/{id}/delete", ct);
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Loan deleted.";
        else
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ApproveReject(int id, int statusId, CancellationToken ct)
    {
        var response = await _http.PatchAsync(
            $"api/loans/{id}/approve-reject?statusId={statusId}", null, ct);

        // Return JSON for AJAX requests (inline approve/reject from Index page)
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (response.IsSuccessStatusCode)
                return Json(new { success = true, message = "Loan status updated." });
            var error = await response.Content.ReadAsStringAsync(ct);
            return Json(new { success = false, message = error });
        }

        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Loan status updated.";
        else
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);

        return RedirectToAction("Details", new { id });
    }

    public async Task<IActionResult> Transactions(
        int page = 1, int pageSize = 10,
        string? loanCode = null, string? planName = null,
        string? borrowerName = null, string? status = null,
        DateTime? startDate = null, DateTime? endDate = null,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Transactions";
        ViewData["ActivePage"] = "Transactions";

        var url = $"api/transactions?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(loanCode)) url += $"&loanCode={Uri.EscapeDataString(loanCode)}";
        if (!string.IsNullOrWhiteSpace(planName)) url += $"&planName={Uri.EscapeDataString(planName)}";
        if (!string.IsNullOrWhiteSpace(borrowerName)) url += $"&borrowerName={Uri.EscapeDataString(borrowerName)}";
        if (!string.IsNullOrWhiteSpace(status)) url += $"&status={Uri.EscapeDataString(status)}";
        if (startDate.HasValue) url += $"&startDate={startDate.Value:yyyy-MM-dd}";
        if (endDate.HasValue) url += $"&endDate={endDate.Value:yyyy-MM-dd}";

        var result = await _http.GetFromJsonAsync<PagedPayload<TransactionListRowDto>>(url, ct);

        var vm = new TransactionListViewModel();
        if (result != null)
        {
            vm.Items = result.Items;
            vm.TotalCount = result.TotalCount;
            vm.CurrentPage = result.CurrentPage;
            vm.PageSize = result.PageSize;
        }
        ViewBag.LoanCode = loanCode;
        ViewBag.PlanName = planName;
        ViewBag.BorrowerName = borrowerName;
        ViewBag.Status = status;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

        return View(vm);
    }

    public async Task<IActionResult> Repay(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Loan Repayment";
        ViewData["ActivePage"] = "Loans";

        var response = await _http.GetAsync($"api/loans/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Loan not found.";
            return RedirectToAction("Index");
        }
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>(cancellationToken: ct);
        return View(new LoanRepayViewModel
        {
            LoanId = loan!.Id,
            LoanCode = loan.LoanCode,
            RemainingBalance = loan.RemainingBalance ?? 0
        });
    }

    [HttpPost]
    public async Task<IActionResult> Repay(LoanRepayViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Loan Repayment";
        ViewData["ActivePage"] = "Loans";

        var response = await _http.PostAsJsonAsync($"api/loans/{model.LoanId}/repay",
            new RepayLoanRequest
            {
                AmountPaid = model.AmountPaid,
                PrincipalAmount = model.PrincipalAmount,
                InterestAmount = model.InterestAmount,
                Note = model.Note
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Repayment recorded successfully.";
        return RedirectToAction("Details", new { id = model.LoanId });
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private async Task<LoanFormViewModel> BuildLoanFormAsync(CancellationToken ct)
    {
        var borrowers = await _http.GetFromJsonAsync<PagedPayload<BorrowerDto>>(
            "api/borrowers?page=1&pageSize=200", ct);
        var settings = await _http.GetFromJsonAsync<PagedPayload<LoanSettingDto>>(
            "api/loans/plan-options?page=1&pageSize=100", ct);
        return new LoanFormViewModel
        {
            Borrowers  = borrowers?.Items  ?? new List<BorrowerDto>(),
            LoanSettings = settings?.Items ?? new List<LoanSettingDto>()
        };
    }
}
