using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Payments;

public sealed record GetPaymentConfigurationQuery(Guid WorkspaceId) : IRequest<PaymentConfigurationDto?>;
public sealed record UpsertPaymentConfigurationCommand(Guid WorkspaceId, PaymentConfigurationRequest Request) : IRequest<PaymentConfigurationDto>;
public sealed record StorePaymentWebhookCommand(Guid WorkspaceId, PaymentWebhookRequest Request) : IRequest<CreatedDto<Guid>>;

internal sealed class GetPaymentConfigurationHandler : IRequestHandler<GetPaymentConfigurationQuery, PaymentConfigurationDto?>
{
    private readonly IPaymentRepository _payments;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetPaymentConfigurationHandler(IPaymentRepository payments, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _payments = payments;
        _auth = auth;
        _current = current;
    }

    public async Task<PaymentConfigurationDto?> Handle(GetPaymentConfigurationQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Owner, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to view payment configuration.");

        return await _payments.GetPaymentConfigurationAsync(request.WorkspaceId, ct);
    }
}

internal sealed class UpsertPaymentConfigurationHandler : IRequestHandler<UpsertPaymentConfigurationCommand, PaymentConfigurationDto>
{
    private readonly IPaymentRepository _payments;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public UpsertPaymentConfigurationHandler(IPaymentRepository payments, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _payments = payments;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<PaymentConfigurationDto> Handle(UpsertPaymentConfigurationCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Owner, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage payment configuration.");

        var result = await _payments.UpsertPaymentConfigurationAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }
}

internal sealed class StorePaymentWebhookHandler : IRequestHandler<StorePaymentWebhookCommand, CreatedDto<Guid>>
{
    private readonly IPaymentRepository _payments;
    private readonly IUnitOfWork _uow;

    public StorePaymentWebhookHandler(IPaymentRepository payments, IUnitOfWork uow)
    {
        _payments = payments;
        _uow = uow;
    }

    public async Task<CreatedDto<Guid>> Handle(StorePaymentWebhookCommand request, CancellationToken ct)
    {
        var result = await _payments.StorePaymentWebhookAsync(request.WorkspaceId, request.Request, ct);
        if (result.Created)
            await _uow.SaveChangesAsync(ct);
        return result;
    }
}
