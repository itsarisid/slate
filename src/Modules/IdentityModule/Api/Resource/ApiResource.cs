using Alphabet.Common.Models;

namespace Alphabet.Modules.IdentityModule.Api.Resource;

public static class ApiResource
{
    // ── Auth endpoints ──────────────────────────────────────────────────

    public static EndpointDetails RegisterUser => new()
    {
        Endpoint = "/register",
        Name = "RegisterUser",
        Summary = "Registers a new user account.",
        Description = "Creates a new user account and returns the created user projection. Email confirmation may still be required before sign-in."
    };

    public static EndpointDetails ConfirmEmail => new()
    {
        Endpoint = "/confirm-email",
        Name = "ConfirmEmail",
        Summary = "Confirms a user's email address.",
        Description = "Validates the email confirmation token for the target account and marks the email address as confirmed."
    };

    public static EndpointDetails Login => new()
    {
        Endpoint = "/login",
        Name = "Login",
        Summary = "Authenticates a user.",
        Description = "Signs a user in with email and password. When useCookies is true, auth cookies are also written to the response."
    };

    public static EndpointDetails ForgotPassword => new()
    {
        Endpoint = "/forgot-password",
        Name = "ForgotPassword",
        Summary = "Starts the forgot-password flow.",
        Description = "Generates and sends a password reset message when the account exists. The response stays generic for security reasons."
    };

    public static EndpointDetails ResetPassword => new()
    {
        Endpoint = "/reset-password",
        Name = "ResetPassword",
        Summary = "Completes the password reset flow.",
        Description = "Resets the user's password using a valid reset token and the new password provided in the request."
    };

    public static EndpointDetails RefreshToken => new()
    {
        Endpoint = "/refresh-token",
        Name = "RefreshToken",
        Summary = "Refreshes an expired or expiring access token.",
        Description = "Uses a refresh token from the request body or auth cookie to issue a fresh access token and refresh token pair."
    };

    public static EndpointDetails Logout => new()
    {
        Endpoint = "/logout",
        Name = "Logout",
        Summary = "Signs the current user out.",
        Description = "Revokes the refresh token, clears auth cookies when present, and ends the current authenticated session."
    };

    public static EndpointDetails ChangePassword => new()
    {
        Endpoint = "/change-password",
        Name = "ChangePassword",
        Summary = "Changes the current user's password.",
        Description = "Changes the authenticated user's password by validating the current password and storing the new one."
    };

    public static EndpointDetails EnableAuthenticator => new()
    {
        Endpoint = "/mfa/enable-authenticator",
        Name = "EnableAuthenticator",
        Summary = "Starts authenticator-app MFA enrollment.",
        Description = "Generates the authenticator secret and setup payload needed to enroll an authenticator application."
    };

    public static EndpointDetails VerifyAuthenticator => new()
    {
        Endpoint = "/mfa/verify-authenticator",
        Name = "VerifyAuthenticator",
        Summary = "Completes authenticator-app MFA enrollment.",
        Description = "Validates the authenticator verification code and returns recovery codes when enrollment succeeds."
    };

    public static EndpointDetails EnableOtp => new()
    {
        Endpoint = "/mfa/enable-otp",
        Name = "EnableOtp",
        Summary = "Enables OTP-based MFA delivery.",
        Description = "Configures one-time-password delivery for the authenticated user through the supported OTP channel."
    };

    public static EndpointDetails VerifyOtp => new()
    {
        Endpoint = "/mfa/verify-otp",
        Name = "VerifyOtp",
        Summary = "Verifies an OTP code.",
        Description = "Validates the OTP code for the authenticated user and confirms the configured MFA method."
    };

    public static EndpointDetails MfaLogin => new()
    {
        Endpoint = "/mfa/login",
        Name = "MfaLogin",
        Summary = "Completes sign-in after MFA challenge.",
        Description = "Validates the MFA token and verification code, then issues the final authenticated token set."
    };

    public static EndpointDetails GetCurrentUser => new()
    {
        Endpoint = "/me",
        Name = "GetCurrentUser",
        Summary = "Gets the currently authenticated user.",
        Description = "Returns the current authenticated user identity, authentication type, and resolved role claims for the active bearer token or auth cookie."
    };

    public static EndpointDetails GetRecoveryCodes => new()
    {
        Endpoint = "/mfa/recovery-codes",
        Name = "GetRecoveryCodes",
        Summary = "Gets current MFA recovery codes.",
        Description = "Returns the currently active recovery codes for the authenticated user."
    };

    public static EndpointDetails RegenerateRecoveryCodes => new()
    {
        Endpoint = "/mfa/recovery-codes/regenerate",
        Name = "RegenerateRecoveryCodes",
        Summary = "Regenerates MFA recovery codes.",
        Description = "Generates a fresh set of recovery codes and invalidates the previous set for the authenticated user."
    };

    // ── Self-service user endpoints ───────────────────────────────────

    public static EndpointDetails UpdateMyProfile => new()
    {
        Endpoint = "",
        Name = "UpdateMyProfile",
        Summary = "Updates the authenticated user's profile.",
        Description = "Updates the authenticated user's name, phone number, biography, department, and location without requiring administrator access."
    };

    public static EndpointDetails UploadMyAvatar => new()
    {
        Endpoint = "/avatar",
        Name = "UploadMyAvatar",
        Summary = "Uploads or replaces the authenticated user's avatar.",
        Description = "Accepts a JPEG, PNG, GIF, or WebP image as multipart/form-data and returns the persisted hosted image URL."
    };

    public static EndpointDetails DeleteMyAvatar => new()
    {
        Endpoint = "/avatar",
        Name = "DeleteMyAvatar",
        Summary = "Removes the authenticated user's custom avatar.",
        Description = "Deletes the hosted custom avatar and clears its URL from the user profile."
    };

    public static EndpointDetails GetMyActivity => new()
    {
        Endpoint = "/activity",
        Name = "GetMyActivity",
        Summary = "Gets the authenticated user's security activity.",
        Description = "Returns the user's sign-in, session, and security audit history, including recorded IP addresses."
    };

    public static EndpointDetails UpdateMyPreferences => new()
    {
        Endpoint = "/preferences",
        Name = "UpdateMyPreferences",
        Summary = "Updates the authenticated user's interface preferences.",
        Description = "Saves theme, notification channel toggles, and preferred timezone for the authenticated user."
    };

    public static EndpointDetails GetSessions => new()
    {
        Endpoint = "/sessions",
        Name = "GetSessions",
        Summary = "Lists active authenticated sessions.",
        Description = "Returns active refresh-token sessions and their originating IP addresses for the authenticated user."
    };

    public static EndpointDetails RevokeSession => new()
    {
        Endpoint = "/sessions/{sessionId:guid}",
        Name = "RevokeSession",
        Summary = "Revokes an authenticated session.",
        Description = "Revokes one session owned by the authenticated user."
    };

    public static EndpointDetails RevokeAllSessions => new()
    {
        Endpoint = "/sessions",
        Name = "RevokeAllSessions",
        Summary = "Signs the authenticated user out everywhere.",
        Description = "Revokes all active refresh-token sessions and invalidates the user's security stamp."
    };

    // ── Admin endpoints ─────────────────────────────────────────────────

    public static EndpointDetails AdminCreateUser => new()
    {
        Endpoint = "/users",
        Name = "AdminCreateUser",
        Summary = "Creates a new user account.",
        Description = """
            Creates a user directly from the administration area without requiring self-registration. This is useful for provisioning back-office, support, or managed accounts.

            Example request:
            {
              "email": "operator@alphabet.local",
              "password": "TempPassword123!",
              "firstName": "Amina",
              "lastName": "Rahman",
              "role": "Support"
            }
            """
    };

    public static EndpointDetails AdminGetUsers => new()
    {
        Endpoint = "/users",
        Name = "AdminGetUsers",
        Summary = "Lists all users.",
        Description = "Returns the users that administrators can search, review, and manage."
    };

    public static EndpointDetails AdminGetUserById => new()
    {
        Endpoint = "/users/{userId:guid}",
        Name = "AdminGetUserById",
        Summary = "Gets detailed information for a single user.",
        Description = "Returns account status, role assignments, lockout details, two-factor status, and audit-friendly timestamps for the selected user."
    };

    public static EndpointDetails AdminLockUser => new()
    {
        Endpoint = "/users/{userId:guid}/lock",
        Name = "AdminLockUser",
        Summary = "Locks a user account.",
        Description = """
            Locks the specified user either for a fixed duration or indefinitely when durationMinutes is 0.

            Example request:
            {
              "durationMinutes": 0
            }
            """
    };

    public static EndpointDetails AdminUnlockUser => new()
    {
        Endpoint = "/users/{userId:guid}/unlock",
        Name = "AdminUnlockUser",
        Summary = "Unlocks a previously locked account.",
        Description = "Clears the user's lockout state so they can sign in again."
    };

    public static EndpointDetails AdminResetPassword => new()
    {
        Endpoint = "/users/{userId:guid}/reset-password",
        Name = "AdminResetPassword",
        Summary = "Resets a user's password without the old password.",
        Description = """
            Generates an internal reset token and replaces the user's password immediately. This is intended for administrator-led recovery and support scenarios.

            Example request:
            {
              "newPassword": "NewStrongPassword123!"
            }
            """
    };

    public static EndpointDetails AdminSendResetLink => new()
    {
        Endpoint = "/users/{userId:guid}/send-reset-link",
        Name = "AdminSendResetLink",
        Summary = "Sends a password reset link to the user.",
        Description = "Creates a password reset token and emails a reset link to the user's registered address using the configured communication provider."
    };

    public static EndpointDetails AdminForceLogout => new()
    {
        Endpoint = "/users/{userId:guid}/force-logout",
        Name = "AdminForceLogout",
        Summary = "Forces a user to sign out everywhere.",
        Description = "Revokes refresh tokens and updates the user's security stamp so existing sessions become invalid."
    };

    public static EndpointDetails AdminGetUserAuditLogs => new()
    {
        Endpoint = "/users/{userId:guid}/audit-logs",
        Name = "AdminGetUserAuditLogs",
        Summary = "Gets the user's activity and audit history.",
        Description = """
            Returns security and administrative activity for the selected user, including sign-in attempts, password actions, and account-management operations.

            Query parameters:
            - take: Number of records to return. Defaults to 50.
            - skip: Number of records to skip before returning results. Defaults to 0.
            """
    };

}
