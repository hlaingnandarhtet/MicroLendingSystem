using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Frontend.Models;
using MicroLendingSystem.Frontend.ViewModels;

namespace MicroLendingSystem.Frontend.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly HttpClient _http;

    public DashboardController(IHttpClientFactory factory)
        => _http = factory.CreateClient("BackendApi");

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";
        ViewData["ActivePage"] = "Dashboard";

        var vm = new DashboardViewModel();
        try
        {
            var summary = await _http.GetFromJsonAsync<DashboardSummaryDto>("api/dashboard/summary", ct);
            if (summary != null)
            {
                vm.ShowBorrowerDashboard = summary.ShowBorrowerDashboard;
                vm.ActiveLoansCount = summary.ActiveLoansCount;
                vm.ActiveUsersCount = summary.ActiveUsersCount;
                vm.TotalBorrowersCount = summary.TotalBorrowersCount;
                vm.TotalBorrowedAmountAll = summary.TotalBorrowedAmountAll;
                vm.RevenueThisMonth = summary.RevenueThisMonth;
                vm.BorrowedAmount = summary.BorrowedAmount;
                vm.RepaidAmount = summary.RepaidAmount;
                vm.RemainingBalance = summary.RemainingBalance;
                vm.AccruedInterest = summary.AccruedInterest;
                vm.RecentLoans = summary.RecentLoans.Select(l => new RecentLoanItem
                {
                    Id = l.Id,
                    LoanCode = l.LoanCode,
                    BorrowerName = l.BorrowerName,
                    Amount = l.Amount,
                    Status = l.Status,
                    CreatedAt = l.CreatedAt
                }).ToList();
                vm.MonthlyData = summary.MonthlyData.Select(m => new ChartDataPoint
                {
                    Label = m.Label,
                    Value = m.Value
                }).ToList();
            }
        }
        catch
        {
            // Dashboard degrades gracefully if Backend is unavailable
        }

        return View(vm);
    }
}
