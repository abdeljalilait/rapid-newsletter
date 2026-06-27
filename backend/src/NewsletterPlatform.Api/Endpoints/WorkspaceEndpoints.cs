using MediatR;
using NewsletterPlatform.Application.Features.Workspaces;
using NewsletterPlatform.Application.Features.Workspaces.Commands;
using NewsletterPlatform.Application.Features.Workspaces.Queries;

namespace NewsletterPlatform.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static IEndpointRouteBuilder MapWorkspaceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workspaces").WithTags("Workspaces").RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetMyWorkspacesQuery(), ct);
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetWorkspaceByIdQuery(id), ct);
            return Results.Ok(result);
        });

        group.MapPost("/", async (IMediator mediator, CreateWorkspaceRequest req, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateWorkspaceCommand(
                req.Name, req.Slug, req.Description, req.DefaultSenderName,
                req.DefaultSenderEmail, req.LogoUrl, req.Timezone, req.DefaultCurrency), ct);
            return Results.Created($"/api/workspaces/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdateWorkspaceRequest req, CancellationToken ct) =>
        {
            var result = await mediator.Send(new UpdateWorkspaceCommand(
                id, req.Name, req.DefaultSenderName, req.DefaultSenderEmail,
                req.Description, req.LogoUrl, req.Timezone, req.DefaultCurrency), ct);
            return Results.Ok(result);
        });

        return app;
    }
}