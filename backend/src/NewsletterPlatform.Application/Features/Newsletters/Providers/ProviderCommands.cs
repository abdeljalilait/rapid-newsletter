using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;
using FluentValidation.Results;

namespace NewsletterPlatform.Application.Features.Newsletters.Providers;

public sealed record GetProviderAccountsQuery(Guid WorkspaceId) : IRequest<IReadOnlyCollection<ProviderAccountDto>>;
public sealed record CreateProviderAccountCommand(Guid WorkspaceId, ProviderAccountRequest Request) : IRequest<ProviderAccountDto>;
public sealed record ValidateProviderAccountCommand(Guid WorkspaceId, Guid ProviderAccountId) : IRequest<ProviderAccountDto?>;

internal sealed class GetProviderAccountsHandler : IRequestHandler<GetProviderAccountsQuery, IReadOnlyCollection<ProviderAccountDto>>
{
    private readonly IProviderAccountRepository _providers;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetProviderAccountsHandler(IProviderAccountRepository providers, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _providers = providers;
        _auth = auth;
        _current = current;
    }

    public async Task<IReadOnlyCollection<ProviderAccountDto>> Handle(GetProviderAccountsQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to view provider accounts.");

        return await _providers.GetProviderAccountsAsync(request.WorkspaceId, ct);
    }
}

internal sealed class CreateProviderAccountHandler : IRequestHandler<CreateProviderAccountCommand, ProviderAccountDto>
{
    private readonly IProviderAccountRepository _providers;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public CreateProviderAccountHandler(IProviderAccountRepository providers, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _providers = providers;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<ProviderAccountDto> Handle(CreateProviderAccountCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage provider accounts.");

        if (!IsSupportedProvider(request.Request.Provider))
            throw new Exceptions.ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Request.Provider), "This email provider is not supported.")
            });

        var result = await _providers.CreateProviderAccountAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }

    private static bool IsSupportedProvider(EmailProvider provider) =>
        provider is EmailProvider.Resend
            or EmailProvider.Mailtrap
            or EmailProvider.MailerSend
            or EmailProvider.Plunk
            or EmailProvider.AmazonSes;
}

internal sealed class ValidateProviderAccountHandler : IRequestHandler<ValidateProviderAccountCommand, ProviderAccountDto?>
{
    private readonly IProviderAccountRepository _providers;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public ValidateProviderAccountHandler(IProviderAccountRepository providers, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _providers = providers;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<ProviderAccountDto?> Handle(ValidateProviderAccountCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage provider accounts.");

        var result = await _providers.ValidateProviderAccountAsync(request.WorkspaceId, request.ProviderAccountId, ct);
        if (result is not null)
            await _uow.SaveChangesAsync(ct);
        return result;
    }
}
