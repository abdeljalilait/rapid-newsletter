namespace NewsletterPlatform.Domain.Entities;

public class WorkspaceMember : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public WorkspaceRole Role { get; set; }
    public string? InvitedByEmail { get; set; }
    public DateTime? JoinedAt { get; set; }
}
