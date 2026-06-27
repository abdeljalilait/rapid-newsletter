using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NewsletterPlatformDbContext _db;

    public UserRepository(NewsletterPlatformDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken ct = default) =>
        _db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<User?> GetByEmailWithTokensAsync(string normalizedEmail, CancellationToken ct = default) =>
        _db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
    }

    public void Update(User user)
    {
        if (_db.Entry(user).State == EntityState.Detached)
            _db.Users.Update(user);
    }

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default) =>
        _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task RevokeAllActiveTokensForUserAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.Status == RefreshTokenStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, RefreshTokenStatus.Revoked)
                .SetProperty(t => t.RevokedAt, now)
                .SetProperty(t => t.RevokedReason, reason)
                .SetProperty(t => t.UpdatedAt, now), ct);
    }

    public Task<User?> GetByPasswordResetTokenAsync(string tokenHash, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash, ct);

    public Task<User?> GetByEmailConfirmationTokenAsync(string tokenHash, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.EmailConfirmationTokenHash == tokenHash, ct);
}

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly NewsletterPlatformDbContext _db;

    public WorkspaceRepository(NewsletterPlatformDbContext db) => _db = db;

    public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Workspaces.Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _db.Workspaces.Include(w => w.Members).FirstOrDefaultAsync(w => w.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        _db.Workspaces.AnyAsync(w => w.Slug == slug, ct);

    public async Task AddAsync(Workspace workspace, CancellationToken ct = default) =>
        await _db.Workspaces.AddAsync(workspace, ct);

    public void Update(Workspace workspace)
    {
        if (_db.Entry(workspace).State == EntityState.Detached)
            _db.Workspaces.Update(workspace);
    }

    public async Task<IReadOnlyCollection<Workspace>> GetWorkspacesForUserAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Workspaces
            .Include(w => w.Members)
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .OrderByDescending(w => w.CreatedAt)
            .ToArrayAsync(ct);
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly NewsletterPlatformDbContext _db;
    public AuditLogRepository(NewsletterPlatformDbContext db) => _db = db;
    public Task AddAsync(AuditLog log, CancellationToken ct = default) => _db.AuditLogs.AddAsync(log, ct).AsTask();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly NewsletterPlatformDbContext _db;
    public UnitOfWork(NewsletterPlatformDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}