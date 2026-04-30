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
    /// <summary>
    /// Registers the identity module endpoints.
    /// </summary>
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

        group.MapPost("/users", async Task<Results<Created<UserDto>, BadRequest<ProblemDetails>>> (AdminCreateUserRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminCreateUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Role), ct);

            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Create user failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/admin/users/{result.Value.UserId}", result.Value);
        })
        .Accepts<AdminCreateUserRequest>("application/json")
        .Produces<UserDto>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdminCreateUser")
        .WithSummary("Creates a new user account.")
        .WithDescription("""
            Creates a user directly from the administration area without requiring self-registration. This is useful for provisioning back-office, support, or managed accounts.

            Example request:
            {
              "email": "operator@alphabet.local",
              "password": "TempPassword123!",
              "firstName": "Amina",
              "lastName": "Rahman",
              "role": "Support"
            }
            """);

        group.MapGet("/users", async Task<Ok<IReadOnlyList<UserDto>>> (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUsersQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)
        .WithName("AdminGetUsers")
        .WithSummary("Lists all users.")
        .WithDescription("Returns the users that administrators can search, review, and manage.");

        group.MapGet("/users/{userId:guid}", async Task<Results<Ok<AdminUserDetailDto>, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserByIdQuery(userId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "User not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<AdminUserDetailDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdminGetUserById")
        .WithSummary("Gets detailed information for a single user.")
        .WithDescription("Returns account status, role assignments, lockout details, two-factor status, and audit-friendly timestamps for the selected user.");

        group.MapPost("/users/{userId:guid}/lock", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, AdminLockUserRequest? request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminLockUserCommand(userId, request?.DurationMinutes ?? 0), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Lock user failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AdminLockUserRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdminLockUser")
        .WithSummary("Locks a user account.")
        .WithDescription("""
            Locks the specified user either for a fixed duration or indefinitely when durationMinutes is 0.

            Example request:
            {
              "durationMinutes": 0
            }
            """);

        group.MapPost("/users/{userId:guid}/unlock", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UnlockUserCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Unlock user failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdminUnlockUser")
        .WithSummary("Unlocks a previously locked account.")
        .WithDescription("Clears the user's lockout state so they can sign in again.");

        group.MapPost("/users/{userId:guid}/reset-password", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, AdminResetPasswordRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminResetPasswordCommand(userId, request.NewPassword), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Reset password failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<AdminResetPasswordRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdminResetPassword")
        .WithSummary("Resets a user's password without the old password.")
        .WithDescription("""
            Generates an internal reset token and replaces the user's password immediately. This is intended for administrator-led recovery and support scenarios.

            Example request:
            {
              "newPassword": "NewStrongPassword123!"
            }
            """);

        group.MapPost("/users/{userId:guid}/send-reset-link", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminSendResetLinkCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Send reset link failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdminSendResetLink")
        .WithSummary("Sends a password reset link to the user.")
        .WithDescription("Creates a password reset token and emails a reset link to the user's registered address using the configured communication provider.");

        group.MapPost("/users/{userId:guid}/force-logout", async Task<Results<Ok, BadRequest<ProblemDetails>>> (Guid userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminForceLogoutCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Force logout failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithName("AdminForceLogout")
        .WithSummary("Forces a user to sign out everywhere.")
        .WithDescription("Revokes refresh tokens and updates the user's security stamp so existing sessions become invalid.");

        group.MapGet("/users/{userId:guid}/audit-logs", async Task<Ok<IReadOnlyList<AuditLogDto>>> (
            Guid userId,
            [FromQuery] string? take,
            [FromQuery] string? skip,
            ISender sender,
            CancellationToken ct) =>
        {
            var resolvedTake = ParseOrDefault(take, 50);
            var resolvedSkip = ParseOrDefault(skip, 0);
            var result = await sender.Send(new GetUserAuditLogsQuery(userId, resolvedTake, resolvedSkip), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)
        .WithName("AdminGetUserAuditLogs")
        .WithSummary("Gets the user's activity and audit history.")
        .WithDescription("""
            Returns security and administrative activity for the selected user, including sign-in attempts, password actions, and account-management operations.

            Query parameters:
            - take: Number of records to return. Defaults to 50.
            - skip: Number of records to skip before returning results. Defaults to 0.
            """);
    }

    private static int ParseOrDefault(string? rawValue, int defaultValue)
    {
        return int.TryParse(rawValue, out var parsedValue) ? parsedValue : defaultValue;
    }
}
