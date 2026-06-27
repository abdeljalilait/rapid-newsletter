namespace NewsletterPlatform.Application.Features.Workspaces;

public sealed record WorkspaceDto(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string? Description,
    string DefaultSenderName,
    string DefaultSenderEmail,
    string Timezone,
    string DefaultCurrency,
    string Status,
    string CurrentUserRole,
    DateTime CreatedAt);

public sealed record CreateWorkspaceRequest(
    string Name,
    string? Slug,
    string? Description,
    string DefaultSenderName,
    string DefaultSenderEmail,
    string? LogoUrl,
    string Timezone = "UTC",
    string DefaultCurrency = "USD");

public sealed record UpdateWorkspaceRequest(
    string Name,
    string DefaultSenderName,
    string DefaultSenderEmail,
    string? Description,
    string? LogoUrl,
    string Timezone,
    string DefaultCurrency);