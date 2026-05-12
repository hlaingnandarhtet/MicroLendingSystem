namespace microlending_API.Features.Borrowers
{
    public class CreateBorrowerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Nrcno { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int? DocumentId { get; set; }
        public bool CreateLoginAccount { get; set; }
        public string? LoginEmail { get; set; }
        public string? LoginPassword { get; set; }
    }

    public class UpdateBorrowerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Nrcno { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int? DocumentId { get; set; }
    }

    public class GetBorrowersRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? FullName { get; set; }
        public string? PhoneNo { get; set; }
        public string? Address { get; set; }
    }

    public class DeleteBorrowerRequest
    {
        public int Id { get; set; }
    }

    public class BorrowerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Nrcno { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public int? UserId { get; set; }

        public int? CreatedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

