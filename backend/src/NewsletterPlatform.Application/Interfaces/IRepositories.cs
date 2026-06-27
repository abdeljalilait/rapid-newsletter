using NewsletterPlatform.Domain.Entities;

namespace NewsletterPlatform.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailWithTokensAsync(string normalizedEmail, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeAllActiveTokensForUserAsync(Guid userId, string reason, CancellationToken ct = default);
    Task<User?> GetByPasswordResetTokenAsync(string tokenHash, CancellationToken ct = default);
    Task<User?> GetByEmailConfirmationTokenAsync(string tokenHash, CancellationToken ct = default);
}

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Workspace workspace, CancellationToken ct = default);
    void Update(Workspace workspace);
    Task<IReadOnlyCollection<Workspace>> GetWorkspacesForUserAsync(Guid userId, CancellationToken ct = default);
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}