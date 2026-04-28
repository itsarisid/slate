using Alphabet.Application.Features.Identity.Commands;
using Alphabet.Application.Features.Identity.Commands.Mfa;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Features.Identity.Queries;
using Alphabet.Modules.IdentityModule.Api.Models;
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

        group.MapPost("/login", async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (LoginRequest request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);
            if (result.IsFailure || result.Value is null)
            {
                return TypedResults.BadRequest(new ProblemDetails { Title = "Login failed", Detail = result.Error });
            }

            if (request.UseCookies)
            {
                IdentityCookieWriter.WriteAuthCookies(httpContext, result.Value);
            }

            return TypedResults.Ok(result.Value);
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

        group.MapPost("/refresh-token", async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (RefreshTokenRequest request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var refreshToken = string.IsNullOrWhiteSpace(request.RefreshToken)
                ? IdentityCookieWriter.GetRefreshTokenFromCookie(httpContext)
                : request.RefreshToken;

            var result = await sender.Send(new RefreshTokenCommand(refreshToken ?? string.Empty), ct);
            if (result.IsFailure || result.Value is null)
            {
                return TypedResults.BadRequest(new ProblemDetails { Title = "Refresh token failed", Detail = result.Error });
            }

            if (request.UseCookies || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                IdentityCookieWriter.WriteAuthCookies(httpContext, result.Value);
            }

            return TypedResults.Ok(result.Value);
        });

        group.MapPost("/logout", async Task<Results<Ok, BadRequest<ProblemDetails>>> (LogoutRequest request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var refreshToken = string.IsNullOrWhiteSpace(request.RefreshToken)
                ? IdentityCookieWriter.GetRefreshTokenFromCookie(httpContext)
                : request.RefreshToken;

            var result = await sender.Send(new LogoutCommand(refreshToken ?? string.Empty), ct);
            if (result.IsFailure)
            {
                return TypedResults.BadRequest(new ProblemDetails { Title = "Logout failed", Detail = result.Error });
            }

            IdentityCookieWriter.ClearAuthCookies(httpContext);
            return TypedResults.Ok();
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

        group.MapPost("/mfa/login", async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (MfaLoginRequest request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new MfaLoginCommand(request.MfaToken, request.VerificationCode), ct);
            if (result.IsFailure || result.Value is null)
            {
                return TypedResults.BadRequest(new ProblemDetails { Title = "MFA login failed", Detail = result.Error });
            }

            if (request.UseCookies)
            {
                IdentityCookieWriter.WriteAuthCookies(httpContext, result.Value);
            }

            return TypedResults.Ok(result.Value);
        });

        group.MapGet("/me", async Task<Results<Ok<CurrentUserDto>, BadRequest<ProblemDetails>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Current user could not be resolved", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .RequireAuthorization()
        .Produces<CurrentUserDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("GetCurrentUser")
        .WithSummary("Gets the currently authenticated user.")
        .WithDescription("Returns the current authenticated user identity, authentication type, and resolved role claims for the active bearer token or auth cookie.");

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
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        // ── User CRUD ─────────────────────────────────────────────────

        group.MapPost("/users", async Task<Results<Created<UserDto>, BadRequest<ProblemDetails>>> (AdminCreateUserRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminCreateUserCommand(
                request.Email, request.Password, request.FirstName, request.LastName, request.Role), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Create user failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/admin/users/{result.Value.UserId}", result.Value);
        });

        group.MapGet("/users", async Task<Ok<IReadOnlyList<UserDto>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUsersQuery(), ct);
            return TypedResults.Ok(result);
        });

        group.MapGet("/users/{userId:guid}", async Task<Results<Ok<AdminUserDetailDto>, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserByIdQuery(userId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "User not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        });

        // ── Lock / Unlock ─────────────────────────────────────────────

        group.MapPost("/users/{userId:guid}/lock", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, AdminLockUserRequest? request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminLockUserCommand(userId, request?.DurationMinutes ?? 0), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Lock user failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        group.MapPost("/users/{userId:guid}/unlock", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UnlockUserCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Unlock user failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        // ── Password management ───────────────────────────────────────

        group.MapPost("/users/{userId:guid}/reset-password", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, AdminResetPasswordRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminResetPasswordCommand(userId, request.NewPassword), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Reset password failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        group.MapPost("/users/{userId:guid}/send-reset-link", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminSendResetLinkCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Send reset link failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        // ── Session management ────────────────────────────────────────

        group.MapPost("/users/{userId:guid}/force-logout", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminForceLogoutCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Force logout failed", Detail = result.Error })
                : TypedResults.Ok();
        });

        // ── Audit logs ────────────────────────────────────────────────

        group.MapGet("/users/{userId:guid}/audit-logs", async Task<Ok<IReadOnlyList<AuditLogDto>>> (Guid userId, int? take, int? skip, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserAuditLogsQuery(userId, take ?? 50, skip ?? 0), ct);
            return TypedResults.Ok(result);
        });
    }
}
