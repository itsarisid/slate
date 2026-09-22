using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using FluentValidation;
using MediatR;

namespace Alphabet.Application.Features.Identity.Commands;

public sealed record UpdateUserProfileCommand(Guid UserId, UpdateUserProfileRequest Request) : IRequest<Result<UserProfileDto>>;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.PhoneNumber).MaximumLength(32);
        RuleFor(x => x.Request.Bio).MaximumLength(2000);
        RuleFor(x => x.Request.Department).MaximumLength(256);
        RuleFor(x => x.Request.Location).MaximumLength(256);
    }
}

public sealed class UpdateUserProfileCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    public Task<Result<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        => identityService.UpdateProfileAsync(request.UserId, request.Request, cancellationToken);
}

public sealed record UpdateUserAvatarCommand(Guid UserId, string? AvatarUrl) : IRequest<Result<UserProfileDto>>;

public sealed class UpdateUserAvatarCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateUserAvatarCommand, Result<UserProfileDto>>
{
    public Task<Result<UserProfileDto>> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
        => identityService.UpdateAvatarAsync(request.UserId, request.AvatarUrl, cancellationToken);
}

public sealed record UpdateUserPreferencesCommand(Guid UserId, UpdateUserPreferencesRequest Request) : IRequest<Result<UserPreferencesDto>>;

public sealed class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
{
    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.Request.Theme).Must(x => x is null || x is "light" or "dark" or "system");
        RuleFor(x => x.Request.Timezone).MaximumLength(128);
    }
}

public sealed class UpdateUserPreferencesCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateUserPreferencesCommand, Result<UserPreferencesDto>>
{
    public Task<Result<UserPreferencesDto>> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
        => identityService.UpdatePreferencesAsync(request.UserId, request.Request, cancellationToken);
}

public sealed record RevokeUserSessionCommand(Guid UserId, Guid SessionId) : IRequest<Result>;

public sealed class RevokeUserSessionCommandHandler(IIdentityService identityService) : IRequestHandler<RevokeUserSessionCommand, Result>
{
    public Task<Result> Handle(RevokeUserSessionCommand request, CancellationToken cancellationToken)
        => identityService.RevokeSessionAsync(request.UserId, request.SessionId, cancellationToken);
}

public sealed record RevokeAllUserSessionsCommand(Guid UserId) : IRequest<Result>;

public sealed class RevokeAllUserSessionsCommandHandler(IIdentityService identityService) : IRequestHandler<RevokeAllUserSessionsCommand, Result>
{
    public Task<Result> Handle(RevokeAllUserSessionsCommand request, CancellationToken cancellationToken)
        => identityService.RevokeAllSessionsAsync(request.UserId, cancellationToken);
}
