namespace Microlending.Shared.Models;

public class PaginationMetadata
{
    public int TotalCount { get; set; }

    public int PageSize { get; set; }

    public int CurrentPage { get; set; }
}
