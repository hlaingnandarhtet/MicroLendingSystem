namespace MicroLendingSystem.Database.Models;

public partial class Document
{
    public int Id { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Borrower> Borrowers { get; set; } = new List<Borrower>();
}
