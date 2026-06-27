using MediatR;
using NewsletterPlatform.Application.Features.Authentication;
using NewsletterPlatform.Application.Features.Authentication.Commands;

namespace NewsletterPlatform.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (IMediator mediator, RegisterRequest req, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RegisterCommand(req.Email, req.Password, req.FirstName, req.LastName), ct);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (IMediator mediator, LoginRequest req, CancellationToken ct) =>
        {
            var result = await mediator.Send(new LoginCommand(req.Email, req.Password), ct);
            return Results.Ok(result);
        });

        group.MapPost("/refresh", async (IMediator mediator, RefreshRequest req, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RefreshCommand(req.RefreshToken), ct);
            return Results.Ok(result);
        });

        group.MapPost("/logout", async (IMediator mediator, LogoutRequest req, CancellationToken ct) =>
        {
            await mediator.Send(new LogoutCommand(req.RefreshToken), ct);
            return Results.NoContent();
        }).RequireAuthorization();

        group.MapPost("/confirm-email", async (IMediator mediator, ConfirmEmailRequest req, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ConfirmEmailCommand(req.Token), ct);
            return Results.Ok(result);
        });

        group.MapPost("/request-password-reset", async (IMediator mediator, RequestPasswordResetRequest req, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RequestPasswordResetCommand(req.Email), ct);
            return Results.Ok(result);
        });

        group.MapPost("/reset-password", async (IMediator mediator, ResetPasswordRequest req, CancellationToken ct) =>
        {
            await mediator.Send(new ResetPasswordCommand(req.Token, req.NewPassword), ct);
            return Results.NoContent();
        });

        group.MapGet("/me", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery(), ct);
            return Results.Ok(result);
        }).RequireAuthorization();

        return app;
    }
}