using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroLendingSystem.Frontend.Models;
using MicroLendingSystem.Frontend.ViewModels;

namespace MicroLendingSystem.Frontend.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly HttpClient _http;

    public AccountController(IHttpClientFactory factory)
        => _http = factory.CreateClient("BackendApi");

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["IsLoginPage"] = true;
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        ViewData["IsLoginPage"] = true;

        if (!ModelState.IsValid)
            return View(model);

        var response = await _http.PostAsJsonAsync(
            "api/users/login",
            new LoginRequest { Email = model.Email, Password = model.Password },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            model.ErrorMessage = string.IsNullOrWhiteSpace(error)
                ? "Invalid credentials."
                : error.Trim('"');
            return View(model);
        }

        var loginDto = await response.Content.ReadFromJsonAsync<LoginDto>(cancellationToken: ct);

        if (loginDto is null)
        {
            model.ErrorMessage = "Unexpected response from the server.";
            return View(model);
        }

        HttpContext.Session.SetString("jwt_token", loginDto.Token);

        var user = loginDto.User;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
            new("RoleId",                  user.RoleId.ToString()),
            new(ClaimTypes.Role,           user.RoleName)
        };

        try
        {
            var roleResponse = await _http.GetAsync($"api/roles/{user.RoleId}/details", ct);
            if (roleResponse.IsSuccessStatusCode)
            {
                var role = await roleResponse.Content.ReadFromJsonAsync<RoleDetailDto>(cancellationToken: ct);
                if (role?.Permissions != null)
                {
                    foreach (var perm in role.Permissions)
                        claims.Add(new Claim("Permission", perm));
                }
            }
        }
        catch
        {
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        return (user.RoleName ?? "").Trim().ToUpperInvariant() switch
        {
            "ADMIN" => RedirectToAction("Index", "Dashboard"),
            "STAFF" => RedirectToAction("Index", "Dashboard"),
            "BORROWER" => RedirectToAction("Index", "Dashboard"),
            _ => RedirectToAction("Index", "Dashboard")
        };
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("jwt_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
