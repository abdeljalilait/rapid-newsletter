using MediatR;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Application.Features.Newsletters.Analytics;
using NewsletterPlatform.Application.Features.Newsletters.Campaigns;
using NewsletterPlatform.Application.Features.Newsletters.Lists;
using NewsletterPlatform.Application.Features.Newsletters.Payments;
using NewsletterPlatform.Application.Features.Newsletters.Plans;
using NewsletterPlatform.Application.Features.Newsletters.Posts;
using NewsletterPlatform.Application.Features.Newsletters.Providers;
using NewsletterPlatform.Application.Features.Newsletters.Public;
using NewsletterPlatform.Application.Features.Newsletters.Subscribers;
using NewsletterPlatform.Application.Features.Newsletters.Tags;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Api.Endpoints;

public static class NewsletterEndpoints
{
    public static IEndpointRouteBuilder MapNewsletterEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/public").WithTags("Public");

        publicGroup.MapGet("/{workspaceSlug}", async (IMediator mediator, string workspaceSlug, CancellationToken ct) =>
        {
            var workspace = await mediator.Send(new GetPublicWorkspaceQuery(workspaceSlug), ct);
            return workspace is null ? Results.NotFound() : Results.Ok(workspace);
        });

        publicGroup.MapGet("/{workspaceSlug}/plans", async (IMediator mediator, string workspaceSlug, CancellationToken ct) =>
        {
            var plans = await mediator.Send(new GetPublicPlansQuery(workspaceSlug), ct);
            return plans is null ? Results.NotFound() : Results.Ok(plans);
        });

        publicGroup.MapGet("/{workspaceSlug}/posts", async (IMediator mediator, string workspaceSlug, CancellationToken ct) =>
        {
            var posts = await mediator.Send(new GetPublicPostsQuery(workspaceSlug), ct);
            return posts is null ? Results.NotFound() : Results.Ok(posts);
        });

        publicGroup.MapGet("/{workspaceSlug}/posts/{postId:guid}", async (IMediator mediator, string workspaceSlug, Guid postId, CancellationToken ct) =>
        {
            var post = await mediator.Send(new GetPublicPostQuery(workspaceSlug, postId), ct);
            return post is null ? Results.NotFound() : Results.Ok(post);
        });

        publicGroup.MapPost("/{workspaceSlug}/subscribe", async (IMediator mediator, string workspaceSlug, PublicSubscribeRequest request, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SubscribePublicCommand(workspaceSlug, request), ct);
            if (result.Result is null) return Results.NotFound();
            return result.Result.Created
                ? Results.Created($"/api/public/{workspaceSlug}/subscribe", result.Result.Value)
                : Results.Ok(result.Result.Value);
        });

        publicGroup.MapPost("/{workspaceSlug}/unsubscribe", async (IMediator mediator, string workspaceSlug, PublicUnsubscribeRequest request, CancellationToken ct) =>
        {
            var result = await mediator.Send(new UnsubscribePublicCommand(workspaceSlug, request), ct);
            return result.Result is null ? Results.NotFound() : Results.Ok(result.Result);
        });

        var group = app.MapGroup("/api/workspaces/{workspaceId:guid}")
            .WithTags("Newsletter Platform")
            .RequireAuthorization();

        group.MapGet("/subscribers", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetSubscribersQuery(workspaceId), ct));
        });

        group.MapPost("/subscribers", async (IMediator mediator, Guid workspaceId, UpsertSubscriberRequest request, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateSubscriberCommand(workspaceId, request), ct);
            return result is null
                ? Results.Conflict(new { error = "Subscriber already exists." })
                : Results.Created($"/api/workspaces/{workspaceId}/subscribers/{result.Value.Id}", result.Value);
        });

        group.MapPost("/subscribers/import-json", async (IMediator mediator, Guid workspaceId, ImportSubscribersRequest request, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new ImportSubscribersCommand(workspaceId, request), ct));
        });

        group.MapGet("/tags", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetTagsQuery(workspaceId), ct));
        });

        group.MapPost("/tags", async (IMediator mediator, Guid workspaceId, NameRequest request, CancellationToken ct) =>
        {
            var tag = await mediator.Send(new CreateTagCommand(workspaceId, request), ct);
            return Results.Created($"/api/workspaces/{workspaceId}/tags/{tag.Id}", tag);
        });

        group.MapGet("/lists", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetListsQuery(workspaceId), ct));
        });

        group.MapPost("/lists", async (IMediator mediator, Guid workspaceId, ListRequest request, CancellationToken ct) =>
        {
            var list = await mediator.Send(new CreateListCommand(workspaceId, request), ct);
            return Results.Created($"/api/workspaces/{workspaceId}/lists/{list.Id}", list);
        });

        group.MapPost("/lists/{listId:guid}/members/{subscriberId:guid}", async (IMediator mediator, Guid workspaceId, Guid listId, Guid subscriberId, CancellationToken ct) =>
        {
            return await mediator.Send(new AddListMemberCommand(workspaceId, listId, subscriberId), ct)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapGet("/plans", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetPlansQuery(workspaceId), ct));
        });

        group.MapPost("/plans", async (IMediator mediator, Guid workspaceId, PlanRequest request, CancellationToken ct) =>
        {
            var plan = await mediator.Send(new CreatePlanCommand(workspaceId, request), ct);
            return Results.Created($"/api/workspaces/{workspaceId}/plans/{plan.Id}", plan);
        });

        group.MapGet("/payments/dodo/configuration", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            var config = await mediator.Send(new GetPaymentConfigurationQuery(workspaceId), ct);
            return Results.Ok(config);
        });

        group.MapPut("/payments/dodo/configuration", async (IMediator mediator, Guid workspaceId, PaymentConfigurationRequest request, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new UpsertPaymentConfigurationCommand(workspaceId, request), ct));
        });

        group.MapPost("/payments/dodo/webhooks", async (IMediator mediator, Guid workspaceId, PaymentWebhookRequest request, CancellationToken ct) =>
        {
            var result = await mediator.Send(new StorePaymentWebhookCommand(workspaceId, request), ct);
            return result.Created
                ? Results.Accepted($"/api/workspaces/{workspaceId}/payments/dodo/webhooks/{result.Value}", new { Id = result.Value })
                : Results.Ok(new { duplicate = true, Id = result.Value });
        });

        group.MapGet("/provider-accounts", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetProviderAccountsQuery(workspaceId), ct));
        });

        group.MapPost("/provider-accounts", async (IMediator mediator, Guid workspaceId, ProviderAccountRequest request, CancellationToken ct) =>
        {
            var account = await mediator.Send(new CreateProviderAccountCommand(workspaceId, request), ct);
            return Results.Created($"/api/workspaces/{workspaceId}/provider-accounts/{account.Id}", account);
        });

        group.MapPost("/provider-accounts/{providerAccountId:guid}/validate", async (IMediator mediator, Guid workspaceId, Guid providerAccountId, CancellationToken ct) =>
        {
            var account = await mediator.Send(new ValidateProviderAccountCommand(workspaceId, providerAccountId), ct);
            return account is null ? Results.NotFound() : Results.Ok(account);
        });

        group.MapGet("/posts", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetPostsQuery(workspaceId), ct));
        });

        group.MapPost("/posts", async (IMediator mediator, Guid workspaceId, PostRequest request, CancellationToken ct) =>
        {
            var post = await mediator.Send(new CreatePostCommand(workspaceId, request), ct);
            return Results.Created($"/api/workspaces/{workspaceId}/posts/{post.Id}", post);
        });

        group.MapGet("/campaigns", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetCampaignsQuery(workspaceId), ct));
        });

        group.MapPost("/campaigns", async (IMediator mediator, Guid workspaceId, CampaignRequest request, CancellationToken ct) =>
        {
            var campaign = await mediator.Send(new CreateCampaignCommand(workspaceId, request), ct);
            return Results.Created($"/api/workspaces/{workspaceId}/campaigns/{campaign.Id}", campaign);
        });

        group.MapPost("/campaigns/{campaignId:guid}/estimate", async (IMediator mediator, Guid workspaceId, Guid campaignId, CampaignLaunchRequest request, CancellationToken ct) =>
        {
            var estimate = await mediator.Send(new EstimateCampaignCapacityQuery(workspaceId, campaignId, request), ct);
            return estimate is null ? Results.NotFound() : Results.Ok(estimate);
        });

        group.MapPost("/campaigns/{campaignId:guid}/launch", async (IMediator mediator, Guid workspaceId, Guid campaignId, CampaignLaunchRequest request, CancellationToken ct) =>
        {
            var result = await mediator.Send(new LaunchCampaignCommand(workspaceId, campaignId, request), ct);
            if (result is null) return Results.NotFound();
            return result.Error is null
                ? Results.Ok(result.Campaign)
                : Results.BadRequest(new { error = result.Error, recipients = result.Recipients, capacity = result.Capacity });
        });

        group.MapPost("/campaigns/{campaignId:guid}/dispatch/claim", async (IMediator mediator, Guid workspaceId, Guid campaignId, CampaignDispatchClaimRequest request, CancellationToken ct) =>
        {
            var batch = await mediator.Send(new ClaimCampaignDispatchBatchCommand(workspaceId, campaignId, request), ct);
            return batch is null ? Results.NotFound() : Results.Ok(batch);
        });

        group.MapPost("/campaigns/{campaignId:guid}/pause", async (IMediator mediator, Guid workspaceId, Guid campaignId, CancellationToken ct) =>
        {
            var campaign = await mediator.Send(new SetCampaignStatusCommand(workspaceId, campaignId, CampaignStatus.Paused), ct);
            return campaign is null ? Results.NotFound() : Results.Ok(campaign);
        });

        group.MapPost("/campaigns/{campaignId:guid}/resume", async (IMediator mediator, Guid workspaceId, Guid campaignId, CancellationToken ct) =>
        {
            var campaign = await mediator.Send(new SetCampaignStatusCommand(workspaceId, campaignId, CampaignStatus.Sending), ct);
            return campaign is null ? Results.NotFound() : Results.Ok(campaign);
        });

        group.MapPost("/campaigns/{campaignId:guid}/cancel", async (IMediator mediator, Guid workspaceId, Guid campaignId, CancellationToken ct) =>
        {
            var campaign = await mediator.Send(new SetCampaignStatusCommand(workspaceId, campaignId, CampaignStatus.Cancelled), ct);
            return campaign is null ? Results.NotFound() : Results.Ok(campaign);
        });

        group.MapGet("/analytics/overview", async (IMediator mediator, Guid workspaceId, CancellationToken ct) =>
        {
            return Results.Ok(await mediator.Send(new GetOverviewAnalyticsQuery(workspaceId), ct));
        });

        return app;
    }
}
