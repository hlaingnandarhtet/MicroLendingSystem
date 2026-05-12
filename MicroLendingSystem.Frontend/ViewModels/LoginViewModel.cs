using System.ComponentModel.DataAnnotations;

namespace MicroLendingSystem.Frontend.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "admin@gmail.com";

    [Required]
    public string Password { get; set; } = "admin123!";

    public string? ErrorMessage { get; set; }
}
