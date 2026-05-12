using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Frontend.Models;
using MicroLendingSystem.Frontend.ViewModels;

namespace MicroLendingSystem.Frontend.Controllers;

[Authorize]
public class BorrowerController : Controller
{
    private readonly HttpClient _http;

    public BorrowerController(IHttpClientFactory factory)
        => _http = factory.CreateClient("BackendApi");

    private void AttachBearerFromSession()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<IActionResult> Index(
        int page = 1, int pageSize = 10,
        string? fullName = null, string? userName = null, string? phoneNo = null, string? nrcNo = null,
        CancellationToken ct = default)
    {
        ViewData["Title"] = "Borrowers";
        ViewData["ActivePage"] = "Borrowers";
        AttachBearerFromSession();

        var url = $"api/borrowers?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(fullName)) url += $"&fullName={Uri.EscapeDataString(fullName)}";
        if (!string.IsNullOrWhiteSpace(userName)) url += $"&userName={Uri.EscapeDataString(userName)}";
        if (!string.IsNullOrWhiteSpace(phoneNo)) url += $"&phoneNo={Uri.EscapeDataString(phoneNo)}";
        if (!string.IsNullOrWhiteSpace(nrcNo)) url += $"&nrcNo={Uri.EscapeDataString(nrcNo)}";

        var result = await _http.GetFromJsonAsync<PagedPayload<BorrowerDto>>(url, ct);

        var vm = new PagedViewModel<BorrowerDto>();
        if (result != null)
        {
            vm.Items = result.Items;
            vm.TotalCount = result.TotalCount;
            vm.CurrentPage = result.CurrentPage;
            vm.PageSize = result.PageSize;
        }
        ViewBag.FullName = fullName;
        ViewBag.UserName = userName;
        ViewBag.PhoneNo = phoneNo;
        ViewBag.NrcNo = nrcNo;
        return View(vm);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Add Borrower";
        ViewData["ActivePage"] = "Borrowers";
        return View(new BorrowerFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(BorrowerFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Add Borrower";
        ViewData["ActivePage"] = "Borrowers";
        AttachBearerFromSession();

        var response = await _http.PostAsJsonAsync("api/borrowers/create",
            new CreateBorrowerRequest
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Nrcno = model.Nrcno,
                PhoneNo = model.PhoneNo,
                Address = model.Address,
                DocumentId = model.DocumentId,
                CreateLoginAccount = model.CreateLoginAccount,
                LoginEmail = model.LoginEmail,
                LoginPassword = model.LoginPassword
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Borrower created successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Borrower";
        ViewData["ActivePage"] = "Borrowers";
        AttachBearerFromSession();

        var response = await _http.GetAsync($"api/borrowers/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Borrower not found.";
            return RedirectToAction("Index");
        }
        var d = await response.Content.ReadFromJsonAsync<BorrowerDto>(cancellationToken: ct);
        if (d is null) return RedirectToAction("Index");

        return View(new BorrowerFormViewModel
        {
            Id = d.Id,
            FullName = d.FullName,
            UserName = d.UserName,
            Nrcno = d.Nrcno,
            PhoneNo = d.PhoneNo,
            Address = d.Address,
            DocumentId = d.DocumentId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, BorrowerFormViewModel model, CancellationToken ct)
    {
        ViewData["Title"] = "Edit Borrower";
        ViewData["ActivePage"] = "Borrowers";
        AttachBearerFromSession();

        var response = await _http.PutAsJsonAsync($"api/borrowers/{id}/update",
            new UpdateBorrowerRequest
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Nrcno = model.Nrcno,
                PhoneNo = model.PhoneNo,
                Address = model.Address,
                DocumentId = model.DocumentId
            }, ct);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Borrower updated successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Borrower Details";
        ViewData["ActivePage"] = "Borrowers";
        AttachBearerFromSession();

        var response = await _http.GetAsync($"api/borrowers/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Borrower not found.";
            return RedirectToAction("Index");
        }
        var d = await response.Content.ReadFromJsonAsync<BorrowerDto>(cancellationToken: ct);
        return View(d);
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Delete Borrower";
        ViewData["ActivePage"] = "Borrowers";
        AttachBearerFromSession();

        var response = await _http.GetAsync($"api/borrowers/{id}/details", ct);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Borrower not found.";
            return RedirectToAction("Index");
        }
        var d = await response.Content.ReadFromJsonAsync<BorrowerDto>(cancellationToken: ct);
        return View(d);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        AttachBearerFromSession();
        var response = await _http.DeleteAsync($"api/borrowers/{id}/delete", ct);
        if (response.IsSuccessStatusCode)
            TempData["SuccessMessage"] = "Borrower deleted.";
        else
            TempData["ErrorMessage"] = await response.Content.ReadAsStringAsync(ct);

        return RedirectToAction("Index");
    }
}