namespace MicroLendingSystem.Backend.Options;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = "";

    public string Issuer { get; init; } = "";

    public string Audience { get; init; } = "";

    public int AccessTokenExpirationMinutes { get; init; } = 60;
}
