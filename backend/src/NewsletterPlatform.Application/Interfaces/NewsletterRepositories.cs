using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Interfaces;

public interface IWorkspaceAuthorization
{
    Task<bool> HasWorkspaceRoleAsync(Guid userId, Guid workspaceId, WorkspaceRole requiredRole, CancellationToken ct = default);
}

public interface IPublicWorkspaceReader
{
    Task<PublicWorkspaceDto?> GetPublicWorkspaceAsync(string workspaceSlug, CancellationToken ct = default);
    Task<IReadOnlyCollection<PlanDto>?> GetPublicPlansAsync(string workspaceSlug, CancellationToken ct = default);
    Task<IReadOnlyCollection<PostDto>?> GetPublicPostsAsync(string workspaceSlug, CancellationToken ct = default);
    Task<PublicPostDetailDto?> GetPublicPostAsync(string workspaceSlug, Guid postId, CancellationToken ct = default);
}

public interface ISubscriberRepository
{
    Task<IReadOnlyCollection<string>> GetSubscriberEmailsAsync(Guid workspaceId, CancellationToken ct = default);
    Task<IReadOnlyCollection<SubscriberDto>> GetSubscribersAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CreatedDto<SubscriberDto>?> CreateSubscriberAsync(Guid workspaceId, UpsertSubscriberRequest request, CancellationToken ct = default);
    Task<ImportSummaryDto> ImportSubscribersAsync(Guid workspaceId, ImportSubscribersRequest request, CancellationToken ct = default);
    Task<CreatedDto<SubscriberDto>?> SubscribePublicAsync(string workspaceSlug, PublicSubscribeRequest request, CancellationToken ct = default);
    Task<PublicUnsubscribeDto?> UnsubscribePublicAsync(string workspaceSlug, PublicUnsubscribeRequest request, CancellationToken ct = default);
}

public interface ITagRepository
{
    Task<IReadOnlyCollection<TagDto>> GetTagsAsync(Guid workspaceId, CancellationToken ct = default);
    Task<TagDto> CreateTagAsync(Guid workspaceId, NameRequest request, CancellationToken ct = default);
}

public interface IListRepository
{
    Task<IReadOnlyCollection<ListDto>> GetListsAsync(Guid workspaceId, CancellationToken ct = default);
    Task<ListDto> CreateListAsync(Guid workspaceId, ListRequest request, CancellationToken ct = default);
    Task<bool> AddListMemberAsync(Guid workspaceId, Guid listId, Guid subscriberId, CancellationToken ct = default);
}

public interface IPlanRepository
{
    Task<IReadOnlyCollection<PlanDto>> GetPlansAsync(Guid workspaceId, CancellationToken ct = default);
    Task<PlanDto> CreatePlanAsync(Guid workspaceId, PlanRequest request, CancellationToken ct = default);
}

public interface IPaymentRepository
{
    Task<PaymentConfigurationDto?> GetPaymentConfigurationAsync(Guid workspaceId, CancellationToken ct = default);
    Task<PaymentConfigurationDto> UpsertPaymentConfigurationAsync(Guid workspaceId, PaymentConfigurationRequest request, CancellationToken ct = default);
    Task<CreatedDto<Guid>> StorePaymentWebhookAsync(Guid workspaceId, PaymentWebhookRequest request, CancellationToken ct = default);
}

public interface IProviderAccountRepository
{
    Task<IReadOnlyCollection<ProviderAccountDto>> GetProviderAccountsAsync(Guid workspaceId, CancellationToken ct = default);
    Task<ProviderAccountDto> CreateProviderAccountAsync(Guid workspaceId, ProviderAccountRequest request, CancellationToken ct = default);
    Task<ProviderAccountDto?> ValidateProviderAccountAsync(Guid workspaceId, Guid providerAccountId, CancellationToken ct = default);
}

public interface IPostRepository
{
    Task<IReadOnlyCollection<PostDto>> GetPostsAsync(Guid workspaceId, CancellationToken ct = default);
    Task<PostDto> CreatePostAsync(Guid workspaceId, PostRequest request, CancellationToken ct = default);
}

public interface ICampaignRepository
{
    Task<IReadOnlyCollection<CampaignDto>> GetCampaignsAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CampaignDto> CreateCampaignAsync(Guid workspaceId, CampaignRequest request, CancellationToken ct = default);
    Task<CampaignCapacityDto?> EstimateCampaignCapacityAsync(Guid workspaceId, Guid campaignId, CampaignLaunchRequest request, CancellationToken ct = default);
    Task<LaunchCampaignResult?> LaunchCampaignAsync(Guid workspaceId, Guid campaignId, CampaignLaunchRequest request, CancellationToken ct = default);
    Task<CampaignDispatchBatchDto?> ClaimCampaignDispatchBatchAsync(Guid workspaceId, Guid campaignId, CampaignDispatchClaimRequest request, CancellationToken ct = default);
    Task<CampaignDto?> SetCampaignStatusAsync(Guid workspaceId, Guid campaignId, CampaignStatus status, CancellationToken ct = default);
}

public interface IAnalyticsRepository
{
    Task<OverviewAnalyticsDto> GetOverviewAnalyticsAsync(Guid workspaceId, CancellationToken ct = default);
}
