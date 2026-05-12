using MicroLendingSystem.Frontend.Models;

namespace MicroLendingSystem.Frontend.ViewModels;

public class UserFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int? RoleId { get; set; }

    public List<RoleSummaryDto> Roles { get; set; } = new();
}
