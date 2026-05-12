using System.ComponentModel.DataAnnotations;

namespace MicroLendingSystem.Database.Models;

public partial class Borrower
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [MaxLength(100)]
    public string? UserName { get; set; }

    [MaxLength(50)]
    public string? Nrcno { get; set; }

    [MaxLength(50)]
    public string? PhoneNo { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public int? DocumentId { get; set; }

    public int? UserId { get; set; }
    public virtual User? User { get; set; }

    /// <summary>User who registered this borrower (staff isolation).</summary>
    public int? CreatedById { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool? IsDeleted { get; set; }
}
