using NewsletterPlatform.Application.Interfaces;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;
public sealed record CurrentUserDto(Guid UserId, string Email, string FirstName, string? LastName, bool EmailConfirmed);

internal sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _current;

    public GetCurrentUserHandler(IUserRepository users, ICurrentUserService current)
    {
        _users = users;
        _current = current;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");

        var user = await _users.GetByIdAsync(_current.UserId.Value, cancellationToken)
            ?? throw new Exceptions.UnauthorizedException("User not found.");

        return new CurrentUserDto(user.Id, user.Email, user.FirstName, user.LastName, user.EmailConfirmedAt.HasValue);
    }
}
