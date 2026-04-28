using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

/// <summary>
/// Admin creates a new user with role assignment and auto-confirmed email.
/// </summary>
public sealed record AdminCreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role = "User") : IRequest<Result<UserDto>>;

public sealed class AdminCreateUserCommandValidator : AbstractValidator<AdminCreateUserCommand>
{
    public AdminCreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
    }
}

public sealed class AdminCreateUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<AdminCreateUserCommand, Result<UserDto>>
{
    public Task<Result<UserDto>> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
    {
        return identityService.AdminCreateUserAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Role,
            cancellationToken);
    }
}
