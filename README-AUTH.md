# Authentication Module

## Setup

1. Configure `Jwt`, `LockoutSettings`, `MfaSettings`, `EmailSettings`, `SmsSettings`, `FrontendUrls`, and `Cors` in `src/Gateway/Alphabet.AppWire/appsettings.json`.
2. Run migrations:
   ```bash
   dotnet ef migrations add AddIdentityModule --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   dotnet ef database update --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```
3. Start the API. Roles and a default admin user are seeded at startup.

## Default Seed

- Admin email: `admin@alphabet.local`
- Password: `Admin12345!`

## MFA Flow

1. Register and confirm the user email.
2. Call `POST /api/v1/auth/mfa/enable-authenticator`.
3. Render the returned `AuthenticatorUri` as a QR code or show `ManualEntryKey`.
4. Call `POST /api/v1/auth/mfa/verify-authenticator`.
5. Store the recovery codes securely.

For OTP MFA:

1. Call `POST /api/v1/auth/mfa/enable-otp`.
2. Submit the received code to `POST /api/v1/auth/mfa/verify-otp`.

## Troubleshooting

- If email confirmation never arrives, verify `EmailSettings`.
- If OTP verification fails, check the cache provider and server time.
- If users remain locked, use `POST /api/v1/admin/users/{userId}/unlock`.
