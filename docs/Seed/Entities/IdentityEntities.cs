// Inferred from AdminCreateUserRequest, AdminUserDetailDto/UserDto/
// CurrentUserDto, UserProfileDto/UpdateUserProfileRequest,
// UserPreferencesDto/UpdateUserPreferencesRequest, UserSessionDto, and
// RegisterCommand in swagger.json.
//
// This module clearly sits on ASP.NET Core Identity (userId is a uuid,
// there's email/password/lockout/2FA/roles, /auth/refresh-token, etc.), so
// AppUser/AppRole extend IdentityUser<Guid>/IdentityRole<Guid> rather than
// being plain entities like the other modules. If your project already has
// these (it almost certainly does, under a different name — e.g.
// ApplicationUser), delete these two classes and point the seeder at yours.
//
// UserPreferences, UserSession, and UserAuditLog are plain entities — not
// part of Identity itself.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Alphabet.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumberSecondary { get; set; } // avoid clashing with Identity's own PhoneNumber
        public string? Bio { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }

    public class AppRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
    }

    public class UserPreferences
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Theme { get; set; } = "system"; // "light" | "dark" | "system"
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public string? Timezone { get; set; }
    }

    public class UserSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsRevoked { get; set; }
    }

    public class UserAuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Action { get; set; } = default!; // "Created","Locked","Unlocked","PasswordReset","ForceLogout","Login","Logout"
        public string? PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public string? IpAddress { get; set; }
        public Dictionary<string, string?> Details { get; set; } = new();
    }
}
