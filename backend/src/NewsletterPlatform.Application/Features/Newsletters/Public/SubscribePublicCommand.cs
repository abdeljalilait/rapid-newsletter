using MediatR;
using NewsletterPlatform.Application.Interfaces;

namespace NewsletterPlatform.Application.Features.Newsletters.Public;

public sealed record SubscribePublicCommand(string WorkspaceSlug, PublicSubscribeRequest Request) : IRequest<SubscribePublicResult>;
public sealed record SubscribePublicResult(CreatedDto<SubscriberDto>? Result);

internal sealed class SubscribePublicHandler : IRequestHandler<SubscribePublicCommand, SubscribePublicResult>
{
    private readonly ISubscriberRepository _subscribers;
    private readonly IUnitOfWork _uow;

    public SubscribePublicHandler(ISubscriberRepository subscribers, IUnitOfWork uow)
    {
        _subscribers = subscribers;
        _uow = uow;
    }

    public async Task<SubscribePublicResult> Handle(SubscribePublicCommand request, CancellationToken ct)
    {
        var result = await _subscribers.SubscribePublicAsync(request.WorkspaceSlug, request.Request, ct);
        if (result is not null)
            await _uow.SaveChangesAsync(ct);

        return new SubscribePublicResult(result);
    }
}
