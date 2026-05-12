using MicroLendingSystem.Database.Models;

namespace MicroLendingSystem.Backend.Infrastructure;

public interface IJwtTokenService
{
    string CreateAccessToken(User user, IEnumerable<string> permissions);
}
