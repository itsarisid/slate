namespace Alphabet.Infrastructure.Services;

internal static class EmailTemplates
{
    public static string Confirmation(string firstName, string confirmationUrl) =>
        $"""
        <html><body style="font-family:Arial,sans-serif;color:#1f2937;">
        <h2>Confirm your email</h2>
        <p>Hello {firstName},</p>
        <p>Thanks for registering. Confirm your email to unlock your account.</p>
        <p><a href="{confirmationUrl}">Confirm Email</a></p>
        </body></html>
        """;

    public static string PasswordReset(string firstName, string resetUrl) =>
        $"""
        <html><body style="font-family:Arial,sans-serif;color:#1f2937;">
        <h2>Reset your password</h2>
        <p>Hello {firstName},</p>
        <p>Use the link below to reset your password.</p>
        <p><a href="{resetUrl}">Reset Password</a></p>
        </body></html>
        """;

    public static string Otp(string code) => $"Your OTP code is: {code}. Valid for 5 minutes.";

    public static string Lockout(string firstName, DateTimeOffset lockoutEnd) =>
        $"""
        <html><body style="font-family:Arial,sans-serif;color:#1f2937;">
        <h2>Account temporarily locked</h2>
        <p>Hello {firstName},</p>
        <p>Your account has been locked until {lockoutEnd:u} after repeated failed sign-in attempts.</p>
        </body></html>
        """;

    public static string Welcome(string firstName) =>
        $"""
        <html><body style="font-family:Arial,sans-serif;color:#1f2937;">
        <h2>Welcome to Alphabet</h2>
        <p>Hello {firstName},</p>
        <p>Your registration was successful. Please confirm your email to begin using your account.</p>
        </body></html>
        """;
}
