using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Domain.Common;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public sealed partial class NewsletterRepository
{
    public async Task<IReadOnlyCollection<TagDto>> GetTagsAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.Tags.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderBy(x => x.Name)
            .Select(x => TagDto.From(x))
            .ToArrayAsync(ct);
    }

    public Task<TagDto> CreateTagAsync(Guid workspaceId, NameRequest request, CancellationToken ct = default)
    {
        var tag = new Tag { WorkspaceId = workspaceId, Name = request.Name.Trim(), Slug = Slug.Normalize(request.Name) };
        _db.Tags.Add(tag);
        return Task.FromResult(TagDto.From(tag));
    }

    public async Task<IReadOnlyCollection<ListDto>> GetListsAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.AudienceLists.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderBy(x => x.Name)
            .Select(x => ListDto.From(x))
            .ToArrayAsync(ct);
    }

    public Task<ListDto> CreateListAsync(Guid workspaceId, ListRequest request, CancellationToken ct = default)
    {
        var list = new AudienceList { WorkspaceId = workspaceId, Name = request.Name.Trim(), Description = request.Description };
        _db.AudienceLists.Add(list);
        return Task.FromResult(ListDto.From(list));
    }

    public async Task<bool> AddListMemberAsync(Guid workspaceId, Guid listId, Guid subscriberId, CancellationToken ct = default)
    {
        if (!await _db.AudienceLists.AnyAsync(x => x.WorkspaceId == workspaceId && x.Id == listId, ct)
            || !await _db.Subscribers.AnyAsync(x => x.WorkspaceId == workspaceId && x.Id == subscriberId, ct))
            return false;

        var exists = await _db.AudienceListMembers.AnyAsync(x =>
            x.WorkspaceId == workspaceId &&
            x.AudienceListId == listId &&
            x.SubscriberId == subscriberId, ct);

        if (!exists)
            _db.AudienceListMembers.Add(new AudienceListMember { WorkspaceId = workspaceId, AudienceListId = listId, SubscriberId = subscriberId });

        return true;
    }

    public async Task<IReadOnlyCollection<PlanDto>> GetPlansAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.SubscriptionPlans.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderBy(x => x.SortOrder)
            .Select(x => PlanDto.From(x))
            .ToArrayAsync(ct);
    }

    public Task<PlanDto> CreatePlanAsync(Guid workspaceId, PlanRequest request, CancellationToken ct = default)
    {
        var plan = new SubscriptionPlan
        {
            WorkspaceId = workspaceId,
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            BillingInterval = request.BillingInterval,
            DodoProductId = request.DodoProductId,
            BenefitsJson = JsonSerializer.Serialize(request.Benefits ?? []),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        };

        _db.SubscriptionPlans.Add(plan);
        return Task.FromResult(PlanDto.From(plan));
    }

    public async Task<PaymentConfigurationDto?> GetPaymentConfigurationAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var config = await _db.WorkspacePaymentConfigurations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId, ct);
        return config is null ? null : PaymentConfigurationDto.From(config);
    }

    public async Task<PaymentConfigurationDto> UpsertPaymentConfigurationAsync(Guid workspaceId, PaymentConfigurationRequest request, CancellationToken ct = default)
    {
        var config = await _db.WorkspacePaymentConfigurations.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId, ct);
        if (config is null)
        {
            config = new WorkspacePaymentConfiguration
            {
                WorkspaceId = workspaceId,
                EncryptedApiKey = _protector.Protect(request.ApiKey),
                EncryptedWebhookSecret = _protector.Protect(request.WebhookSecret)
            };
            _db.WorkspacePaymentConfigurations.Add(config);
        }
        else
        {
            config.EncryptedApiKey = _protector.Protect(request.ApiKey);
            config.EncryptedWebhookSecret = _protector.Protect(request.WebhookSecret);
        }

        config.Environment = request.Environment;
        config.ConnectionStatus = ConnectionStatus.PendingValidation;
        return PaymentConfigurationDto.From(config);
    }

    public async Task<CreatedDto<Guid>> StorePaymentWebhookAsync(Guid workspaceId, PaymentWebhookRequest request, CancellationToken ct = default)
    {
        var existing = await _db.PaymentWebhookEvents
            .Where(x => x.WorkspaceId == workspaceId && x.ProviderEventId == request.ProviderEventId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (existing != Guid.Empty)
            return new CreatedDto<Guid>(existing, false);

        var webhook = new PaymentWebhookEvent
        {
            WorkspaceId = workspaceId,
            ProviderEventId = request.ProviderEventId,
            EventType = request.EventType,
            RawPayload = request.RawPayload.GetRawText(),
            ProcessingStatus = WebhookProcessingStatus.Pending
        };

        _db.PaymentWebhookEvents.Add(webhook);
        return new CreatedDto<Guid>(webhook.Id, true);
    }
}
