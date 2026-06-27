namespace NewsletterPlatform.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public Guid? ReplacesTokenId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? CreatedByIp { get; set; }
    public RefreshTokenStatus Status { get; set; }
}
