using System.Text.Json;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace NewsletterPlatform.Infrastructure.Services;

public class AuditLogger : IAuditLogger
{
    private readonly NewsletterPlatformDbContext _db;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(NewsletterPlatformDbContext db, ILogger<AuditLogger> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(
        Guid? workspaceId,
        Guid? actorUserId,
        string action,
        string? entityType = null,
        Guid? entityId = null,
        object? metadata = null,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        try
        {
            var json = metadata is null ? null : JsonSerializer.Serialize(metadata);
            var log = new AuditLog
            {
                WorkspaceId = workspaceId,
                ActorUserId = actorUserId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Metadata = json,
                IpAddress = ipAddress
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist audit log {Action}", action);
        }
    }
}
