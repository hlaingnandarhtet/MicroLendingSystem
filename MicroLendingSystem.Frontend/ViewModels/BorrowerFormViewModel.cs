namespace MicroLendingSystem.Frontend.ViewModels;

public class BorrowerFormViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Nrcno { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public bool CreateLoginAccount { get; set; }
    public string? LoginEmail { get; set; }
    public string? LoginPassword { get; set; }
}
