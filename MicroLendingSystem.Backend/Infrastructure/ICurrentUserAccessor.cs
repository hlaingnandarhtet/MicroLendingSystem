namespace MicroLendingSystem.Backend.Infrastructure;

public interface ICurrentUserAccessor
{
    /// <summary>Authenticated user id from JWT, or null if missing.</summary>
    int? UserId { get; }

    bool IsAdmin { get; }
    bool IsBorrower { get; }
    bool TryGetUserId(out int userId);
}
