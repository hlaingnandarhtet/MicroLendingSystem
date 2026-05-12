using System.Security.Claims;

namespace MicroLendingSystem.Frontend.Infrastructure;

/// <summary>
/// Extension methods for checking granular permissions stored as Claims
/// in the authentication cookie. Permission claims use the type "Permission"
/// and values matching PermissionNames constants (e.g. "Loan Read").
/// </summary>
public static class PermissionExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
        => user.Claims.Any(c => c.Type == "Permission" && c.Value == permission);

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole("Admin") ||
           user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

    public static bool HasPermissionOrAdmin(this ClaimsPrincipal user, string permission)
        => user.IsAdmin() || user.HasPermission(permission);

    /// <summary>Show sidebar link if user can list or create borrowers.</summary>
    public static bool CanSeeBorrowerMenu(this ClaimsPrincipal user) =>
        user.IsAdmin()
        || user.HasPermission("Borrower Read")
        || user.HasPermission("Borrower Create")
        || user.HasPermission("Borrower Update")
        || user.HasPermission("Borrower Delete");

    public static bool CanSeeLoanRequestMenu(this ClaimsPrincipal user) =>
        user.IsAdmin()
        || user.HasPermission("Loan Request List")
        || user.HasPermission("Loan Read")
        || user.HasPermission("Loan Create");

    public static bool CanSeeTransactionsMenu(this ClaimsPrincipal user) =>
        user.IsAdmin() || user.HasPermission("Transaction List") || user.HasPermission("Transaction Export");

    public static bool CanSeeLoanSettingMenu(this ClaimsPrincipal user) =>
        user.IsAdmin()
        || user.HasPermission("Loan Setting Read")
        || user.HasPermission("Loan Setting Create")
        || user.HasPermission("Loan Setting Update")
        || user.HasPermission("Loan Setting Delete");
}
