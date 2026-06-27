using MediatR;
using NewsletterPlatform.Application.Interfaces;

namespace NewsletterPlatform.Application.Features.Newsletters.Public;

public sealed record UnsubscribePublicCommand(string WorkspaceSlug, PublicUnsubscribeRequest Request) : IRequest<PublicUnsubscribeResult>;
public sealed record PublicUnsubscribeResult(PublicUnsubscribeDto? Result);

internal sealed class UnsubscribePublicHandler : IRequestHandler<UnsubscribePublicCommand, PublicUnsubscribeResult>
{
    private readonly ISubscriberRepository _subscribers;
    private readonly IUnitOfWork _uow;

    public UnsubscribePublicHandler(ISubscriberRepository subscribers, IUnitOfWork uow)
    {
        _subscribers = subscribers;
        _uow = uow;
    }

    public async Task<PublicUnsubscribeResult> Handle(UnsubscribePublicCommand request, CancellationToken ct)
    {
        var result = await _subscribers.UnsubscribePublicAsync(request.WorkspaceSlug, request.Request, ct);
        if (result is not null)
            await _uow.SaveChangesAsync(ct);

        return new PublicUnsubscribeResult(result);
    }
}
