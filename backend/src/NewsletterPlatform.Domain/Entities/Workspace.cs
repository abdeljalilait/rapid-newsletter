namespace NewsletterPlatform.Domain.Entities;

public class Workspace : BaseEntity, IAggregateRoot
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string DefaultSenderName { get; set; } = null!;
    public string DefaultSenderEmail { get; set; } = null!;
    public string Timezone { get; set; } = "UTC";
    public string DefaultCurrency { get; set; } = "USD";
    public WorkspaceStatus Status { get; set; }
    public List<WorkspaceMember> Members { get; set; } = [];
}
