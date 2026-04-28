# Identity Module Admin API Documentation

This document outlines all the newly added Administrative endpoints for the Identity Module. All endpoints below are secured and require the calling user to fulfill the `AdminOnly` authorization policy.

---

## 1. Create User
**Endpoint**: `POST /api/v1/admin/users`
**Method**: `AdminCreateUserAsync`
**Internal Command**: `AdminCreateUserCommand`

Allows an administrator to directly provision a new user account.
- **Workflow**: 
  - Validates if the email is already in use.
  - Creates the user in the ASP.NET Identity system with their `EmailConfirmed` automatically flagged as true, bypassing the email confirmation flow.
  - Automatically assigns them a role (defaulting to `"User"` if none is provided).
  - Logs an audit event internally for the action.
- **Request Body**:
  ```json
  {
      "email": "user@example.com",
      "password": "SecurePassword123!",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Admin"
  }
  ```
- **Returns**: `201 Created` with the location header pointing to the newly queried user (`/api/v1/admin/users/{userId}`) and the constructed `UserDto`.

---

## 2. Get Users (List)
**Endpoint**: `GET /api/v1/admin/users`
**Method**: `GetUsersAsync`
**Internal Query**: `GetUsersQuery`

Retrieves a high-level summary of all registered users in the system.
- **Returns**: `200 OK` containing a list of `UserDto` objects detailing basic identifiers, names, and email confirmation status.

---

## 3. Get User By ID (Detailed)
**Endpoint**: `GET /api/v1/admin/users/{userId}`
**Method**: `GetUserByIdAsync`
**Internal Query**: `GetUserByIdQuery`

Provides deep, detailed insight into a single user account for administrative review.
- **Workflow**: Looks up the user, queries their identity roles, checks if they are currently locked out, and determines if they possess 2FA.
- **Returns**: `200 OK` containing `AdminUserDetailDto`.
  - Additional fields exposed: `IsLockedOut`, `LockoutEnd`, `LastLoginAt`, `CreatedAt`, `IsTwoFactorEnabled`, and an array of `Roles`.

---

## 4. Lock User
**Endpoint**: `POST /api/v1/admin/users/{userId}/lock`
**Method**: `LockUserAsync`
**Internal Command**: `AdminLockUserCommand`

Immediately freezes a user account, preventing login.
- **Workflow**: 
  - Accepts a `DurationMinutes` modifier.
  - If `DurationMinutes > 0`, it evaluates a timestamp in the future and sets the `LockoutEndDate` in the Identity system.
  - If `DurationMinutes` is `0` or omitted, it sets the lockout end date to `DateTimeOffset.MaxValue`, creating an indefinite ban.
- **Request Body**:
  ```json
  {
      "durationMinutes": 60 
  }
  ```
- **Returns**: `200 OK` upon successful lockout application.

---

## 5. Unlock User
**Endpoint**: `POST /api/v1/admin/users/{userId}/unlock`
**Method**: `UnlockUserAsync`
**Internal Command**: `UnlockUserCommand`

Removes a lockout restriction from an account.
- **Workflow**: Resets the `LockoutEndDateAsync` to `null` and resets the user's `AccessFailedCount` back to 0.
- **Returns**: `200 OK`.

---

## 6. Reset Password (Direct Override)
**Endpoint**: `POST /api/v1/admin/users/{userId}/reset-password`
**Method**: `AdminResetPasswordAsync`
**Internal Command**: `AdminResetPasswordCommand`

Directly assigns a completely new password to a user. This bypasses the old-password requirement standard users are subjected to.
- **Workflow**:
  - The system internally generates a password reset token.
  - Handed the new password, it secretly consumes that reset token internally using `ResetPasswordAsync`.
  - All existing active Refresh Tokens for that user are immediately revoked.
- **Request Body**:
  ```json
  {
      "newPassword": "NewSecurePassword123!"
  }
  ```
- **Returns**: `200 OK`.

---

## 7. Send Password Reset Link
**Endpoint**: `POST /api/v1/admin/users/{userId}/send-reset-link`
**Method**: `AdminSendResetLinkAsync`
**Internal Command**: `AdminSendResetLinkCommand`

Sends an email to the user with a token-based reset URL. Functions exactly like the public "Forgot Password" feature but orchestrated by an admin.
- **Workflow**:
  - Validates user existence.
  - Generates the `PasswordResetToken`.
  - Dispatches the customized Email Template to the user via the `IEmailService`.
- **Returns**: `200 OK`.

---

## 8. Force Logout
**Endpoint**: `POST /api/v1/admin/users/{userId}/force-logout`
**Method**: `AdminForceLogoutAsync`
**Internal Command**: `AdminForceLogoutCommand`

Terminates all active sessions for a user globally.
- **Workflow**:
  - Marks every stored `RefreshToken` for the user as revoked.
  - Triggers a security stamp update via `UpdateSecurityStampAsync`. This causes the active, short-lived JWT Access Tokens to invalidate the next time they hit the server's authentication handlers.
- **Returns**: `200 OK`.

---

## 9. Get User Audit Logs
**Endpoint**: `GET /api/v1/admin/users/{userId}/audit-logs`
**Method**: `GetUserAuditLogsAsync`
**Internal Query**: `GetUserAuditLogsQuery`

Allows an admin to inspect the chronological timeline of actions performed by a user.
- **Workflow**: Queries the `IAuditLogRepository` specifically scoped to the `userId`, sorting sequentially with the newest actions first across pagination markers.
- **Query Parameters**:
  - `take` (default 50)
  - `skip` (default 0)
- **Returns**: `200 OK` exposing `List<AuditLogDto>` with fields `Action`, `Success`, `IpAddress`, `UserAgent`, `Message`, and the `Timestamp`.
