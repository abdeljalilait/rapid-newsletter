using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public sealed partial class NewsletterRepository
{
    public async Task<IReadOnlyCollection<ProviderAccountDto>> GetProviderAccountsAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.EmailProviderAccounts.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId && !x.IsDeleted)
            .OrderBy(x => x.AccountName)
            .Select(x => ProviderAccountDto.From(x))
            .ToArrayAsync(ct);
    }

    public Task<ProviderAccountDto> CreateProviderAccountAsync(Guid workspaceId, ProviderAccountRequest request, CancellationToken ct = default)
    {
        var account = new EmailProviderAccount
        {
            WorkspaceId = workspaceId,
            Provider = request.Provider,
            AccountName = request.AccountName.Trim(),
            EncryptedApiKey = _protector.Protect(request.ApiKey),
            EncryptedApiSecret = string.IsNullOrEmpty(request.ApiSecret) ? string.Empty : _protector.Protect(request.ApiSecret),
            FromName = request.FromName.Trim(),
            FromEmail = NormalizeEmail(request.FromEmail),
            SendingDomain = request.SendingDomain,
            DailyLimit = request.DailyLimit,
            MonthlyLimit = request.MonthlyLimit,
            RatePerMinute = request.RatePerMinute,
            Enabled = request.Enabled
        };

        _db.EmailProviderAccounts.Add(account);
        return Task.FromResult(ProviderAccountDto.From(account));
    }

    public async Task<ProviderAccountDto?> ValidateProviderAccountAsync(Guid workspaceId, Guid providerAccountId, CancellationToken ct = default)
    {
        var account = await _db.EmailProviderAccounts.FirstOrDefaultAsync(x =>
            x.WorkspaceId == workspaceId &&
            x.Id == providerAccountId &&
            !x.IsDeleted, ct);

        if (account is null) return null;

        account.HealthStatus = account.Enabled ? ProviderHealthStatus.Active : ProviderHealthStatus.Disabled;
        account.LastValidatedAt = DateTime.UtcNow;
        return ProviderAccountDto.From(account);
    }
}
