using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Authenticates a user with email and password.
/// </summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        => identityService.LoginAsync(request.Email, request.Password, cancellationToken);
}
