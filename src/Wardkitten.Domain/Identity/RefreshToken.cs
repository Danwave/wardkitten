using Wardkitten.Domain.Common;

namespace Wardkitten.Domain.Identity;

/// <summary>Refresh token rotatorio y revocable. Se persiste <b>hasheado</b> (ver SECURITY.md).</summary>
public sealed class RefreshToken : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive(DateTime nowUtc) => RevokedAtUtc is null && nowUtc < ExpiresAtUtc;
}
