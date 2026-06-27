namespace NewsletterPlatform.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? WorkspaceId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = null!;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Metadata { get; set; }
    public string? IpAddress { get; set; }
}
