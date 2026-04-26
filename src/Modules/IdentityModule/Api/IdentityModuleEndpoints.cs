using Alphabet.Application.Features.Identity.Commands;
using Alphabet.Application.Features.Identity.Commands.Mfa;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Features.Identity.Queries;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.IdentityModule.Api;

/// <summary>
/// Maps authentication and identity administration endpoints.
/// </summary>
public static class IdentityModuleEndpoints
{
    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        MapAuth(endpoints, versionSet);
        MapAdmin(endpoints, versionSet);
        return endpoints;
    }

    private static void MapAuth(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Identity Module");

        group.MapPost("/register", async Task<Results<Created<UserDto>, BadRequest<ProblemDetails>>> (RegisterCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Registration failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/admin/users/{result.Value.UserId}", result.Value);
        });

        group.MapPost("/confirm-email", async Task<Results<Ok, BadRequest<ProblemDetails>>> (ConfirmEmailCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Email confirmation failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        group.MapPost("/login", async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (LoginCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Login failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        });

        group.MapPost("/forgot-password", async Task<Ok> (ForgotPasswordCommand command, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return TypedResults.Ok();
        });

        group.MapPost("/reset-password", async Task<Results<Ok, BadRequest<ProblemDetails>>> (ResetPasswordCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Reset password failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        group.MapPost("/refresh-token", async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (RefreshTokenCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Refresh token failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        });

        group.MapPost("/logout", async Task<Results<Ok, BadRequest<ProblemDetails>>> (LogoutCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Logout failed", Detail = result.Error })
                : TypedResults.Ok();
        }).RequireAuthorization();

        group.MapPost("/change-password", async Task<Results<Ok, BadRequest<ProblemDetails>>> (ChangePasswordCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Change password failed", Detail = result.Error })
                : TypedResults.Ok();
        }).RequireAuthorization();

        group.MapPost("/mfa/enable-authenticator", async Task<Results<Ok<AuthenticatorSetupDto>, BadRequest<ProblemDetails>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new EnableAuthenticatorCommand(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Enable authenticator failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).RequireAuthorization();

        group.MapPost("/mfa/verify-authenticator", async Task<Results<Ok<RecoveryCodesDto>, BadRequest<ProblemDetails>>> (VerifyAuthenticatorCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Verify authenticator failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).RequireAuthorization();

        group.MapPost("/mfa/enable-otp", async Task<Results<Ok, BadRequest<ProblemDetails>>> (EnableOtpCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Enable OTP failed", Detail = result.Error })
                : TypedResults.Ok();
        }).RequireAuthorization();

        group.MapPost("/mfa/verify-otp", async Task<Results<Ok, BadRequest<ProblemDetails>>> (VerifyOtpCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Verify OTP failed", Detail = result.Error })
                : TypedResults.Ok();
        }).RequireAuthorization();

        group.MapPost("/mfa/login", async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (MfaLoginCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "MFA login failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        });

        group.MapGet("/mfa/recovery-codes", async Task<Results<Ok<RecoveryCodesDto>, BadRequest<ProblemDetails>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRecoveryCodesQuery(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Get recovery codes failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).RequireAuthorization();

        group.MapPost("/mfa/recovery-codes/regenerate", async Task<Results<Ok<RecoveryCodesDto>, BadRequest<ProblemDetails>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RegenerateRecoveryCodesCommand(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Regenerate recovery codes failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        }).RequireAuthorization();
    }

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Identity Module")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/users/{userId:guid}/unlock", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UnlockUserCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Unlock user failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        group.MapGet("/users", async Task<Ok<IReadOnlyList<UserDto>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUsersQuery(), ct);
            return TypedResults.Ok(result);
        });
    }
}
