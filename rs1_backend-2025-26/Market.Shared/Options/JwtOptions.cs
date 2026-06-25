using System.ComponentModel.DataAnnotations;

namespace Market.Shared.Options;

/// <summary>Typed JWT settings from the "Jwt" section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; init; } = default!;
    [Required] public string Audience { get; init; } = default!;
    [Required, MinLength(32)] public string Key { get; init; } = default!;

    [Range(1, 1440)] public int AccessTokenMinutes { get; init; } = 20;
    [Range(1, 90)] public int RefreshTokenDays { get; init; } = 14;
}