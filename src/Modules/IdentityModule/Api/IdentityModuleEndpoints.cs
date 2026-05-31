using Alphabet.Modules.IdentityModule.Api.Resource;
using Alphabet.Common.Extensions;
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
    /// <summary>
    /// Map auth.
    /// </summary>

    private static void MapAuth(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Identity Module");

        group.MapPost(ApiResource.RegisterUser.Endpoint, async Task<Results<Created<UserDto>, BadRequest<ProblemDetails>>> (
            [FromBody] RegisterCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Registration failed", Detail = result.Error })
                : TypedResults.Created($"/api/v1/admin/users/{result.Value.UserId}", result.Value);
        })
        .Accepts<RegisterCommand>("application/json")
        .Produces<UserDto>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.RegisterUser);

        group.MapPost(ApiResource.ConfirmEmail.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromBody] ConfirmEmailCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Email confirmation failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<ConfirmEmailCommand>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.ConfirmEmail);

        group.MapPost(ApiResource.Login.Endpoint, async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (
            [FromBody] LoginRequest request,
            HttpContext httpContext,
            [FromServices] ISender sender,
            CancellationToken ct) =>
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
        })
        .Accepts<LoginRequest>("application/json")
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.Login);

        group.MapPost(ApiResource.ForgotPassword.Endpoint, async Task<Ok> (
            [FromBody] ForgotPasswordCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return TypedResults.Ok();
        })
        .Accepts<ForgotPasswordCommand>("application/json")
        .Produces(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.ForgotPassword);

        group.MapPost(ApiResource.ResetPassword.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromBody] ResetPasswordCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Reset password failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Accepts<ResetPasswordCommand>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.ResetPassword);

        group.MapPost(ApiResource.RefreshToken.Endpoint, async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (
            [FromBody] RefreshTokenRequest request,
            HttpContext httpContext,
            [FromServices] ISender sender,
            CancellationToken ct) =>
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
        })
        .Accepts<RefreshTokenRequest>("application/json")
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.RefreshToken);

        group.MapPost(ApiResource.Logout.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromBody] LogoutRequest request,
            HttpContext httpContext,
            [FromServices] ISender sender,
            CancellationToken ct) =>
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
        })
        .RequireAuthorization()
        .Accepts<LogoutRequest>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.Logout);

        group.MapPost(ApiResource.ChangePassword.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromBody] ChangePasswordCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Change password failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .RequireAuthorization()
        .Accepts<ChangePasswordCommand>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.ChangePassword);

        group.MapPost(ApiResource.EnableAuthenticator.Endpoint, async Task<Results<Ok<AuthenticatorSetupDto>, BadRequest<ProblemDetails>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new EnableAuthenticatorCommand(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Enable authenticator failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .RequireAuthorization()
        .Produces<AuthenticatorSetupDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.EnableAuthenticator);

        group.MapPost(ApiResource.VerifyAuthenticator.Endpoint, async Task<Results<Ok<RecoveryCodesDto>, BadRequest<ProblemDetails>>> (
            [FromBody] VerifyAuthenticatorCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Verify authenticator failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .RequireAuthorization()
        .Accepts<VerifyAuthenticatorCommand>("application/json")
        .Produces<RecoveryCodesDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.VerifyAuthenticator);

        group.MapPost(ApiResource.EnableOtp.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromBody] EnableOtpCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Enable OTP failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .RequireAuthorization()
        .Accepts<EnableOtpCommand>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.EnableOtp);

        group.MapPost(ApiResource.VerifyOtp.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            [FromBody] VerifyOtpCommand command,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Verify OTP failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .RequireAuthorization()
        .Accepts<VerifyOtpCommand>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.VerifyOtp);

        group.MapPost(ApiResource.MfaLogin.Endpoint, async Task<Results<Ok<AuthResponseDto>, BadRequest<ProblemDetails>>> (
            [FromBody] MfaLoginRequest request,
            HttpContext httpContext,
            [FromServices] ISender sender,
            CancellationToken ct) =>
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
        })
        .Accepts<MfaLoginRequest>("application/json")
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.MfaLogin);

        group.MapGet(ApiResource.GetCurrentUser.Endpoint, async Task<Results<Ok<CurrentUserDto>, BadRequest<ProblemDetails>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Current user could not be resolved", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .RequireAuthorization()
        .Produces<CurrentUserDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.GetCurrentUser);

        group.MapGet(ApiResource.GetRecoveryCodes.Endpoint, async Task<Results<Ok<RecoveryCodesDto>, BadRequest<ProblemDetails>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRecoveryCodesQuery(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Get recovery codes failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .RequireAuthorization()
        .Produces<RecoveryCodesDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.GetRecoveryCodes);

        group.MapPost(ApiResource.RegenerateRecoveryCodes.Endpoint, async Task<Results<Ok<RecoveryCodesDto>, BadRequest<ProblemDetails>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RegenerateRecoveryCodesCommand(), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Regenerate recovery codes failed", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .RequireAuthorization()
        .Produces<RecoveryCodesDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.RegenerateRecoveryCodes);
    }
    /// <summary>
    /// Map admin.
    /// </summary>

    private static void MapAdmin(IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/users", async Task<Results<Created<UserDto>, BadRequest<ProblemDetails>>> (
            [FromBody] AdminCreateUserRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
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

        group.MapGet(ApiResource.AdminGetUsers.Endpoint, async Task<Ok<IReadOnlyList<UserDto>>> (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUsersQuery(), ct);
            return TypedResults.Ok(result);
        })
        .Produces<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)
        .WithDocumentation(ApiResource.AdminGetUsers);

        group.MapGet(ApiResource.AdminGetUserById.Endpoint, async Task<Results<Ok<AdminUserDetailDto>, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUserByIdQuery(userId), ct);
            return result.IsFailure || result.Value is null
                ? TypedResults.BadRequest(new ProblemDetails { Title = "User not found", Detail = result.Error })
                : TypedResults.Ok(result.Value);
        })
        .Produces<AdminUserDetailDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.AdminGetUserById);

        group.MapPost("/users/{userId:guid}/lock", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromBody] AdminLockUserRequest? request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
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

        group.MapPost(ApiResource.AdminUnlockUser.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UnlockUserCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Unlock user failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.AdminUnlockUser);

        group.MapPost("/users/{userId:guid}/reset-password", async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromBody] AdminResetPasswordRequest request,
            [FromServices] ISender sender,
            CancellationToken ct) =>
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

        group.MapPost(ApiResource.AdminSendResetLink.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminSendResetLinkCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Send reset link failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.AdminSendResetLink);

        group.MapPost(ApiResource.AdminForceLogout.Endpoint, async Task<Results<Ok, BadRequest<ProblemDetails>>> (
            Guid userId,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AdminForceLogoutCommand(userId), ct);
            return result.IsFailure
                ? TypedResults.BadRequest(new ProblemDetails { Title = "Force logout failed", Detail = result.Error })
                : TypedResults.Ok();
        })
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithDocumentation(ApiResource.AdminForceLogout);

        group.MapGet("/users/{userId:guid}/audit-logs", async Task<Ok<IReadOnlyList<AuditLogDto>>> (
            Guid userId,
            [FromQuery] string? take,
            [FromQuery] string? skip,
            [FromServices] ISender sender,
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
    /// <summary>
    /// Parse or default.
    /// </summary>

    private static int ParseOrDefault(string? rawValue, int defaultValue)
    {
        return int.TryParse(rawValue, out var parsedValue) ? parsedValue : defaultValue;
    }
}
