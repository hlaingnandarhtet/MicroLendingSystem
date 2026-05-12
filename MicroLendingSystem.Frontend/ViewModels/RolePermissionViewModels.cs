using MicroLendingSystem.Frontend.Models;

namespace MicroLendingSystem.Frontend.ViewModels;

public class RoleFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AssignPermissionsViewModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionCheckItem> Permissions { get; set; } = new();
}

public class PermissionCheckItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
