# Privilege Module

The Privilege module adds privilege-based access control (PBAC) on top of ASP.NET Core Identity roles. It supports:

- privilege catalog management
- role-to-privilege assignments
- direct user allow/deny assignments
- composite privilege policies
- self-service privilege requests
- runtime privilege evaluation
- audit logging and analytics

## Setup

1. Restore and build the solution:

   ```bash
   dotnet restore Alphabet.slnx
   dotnet build src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```

2. Add and apply migrations:

   ```bash
   dotnet tool run dotnet-ef migrations add PrivilegeModule \
     --context AppDbContext \
     --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj \
     --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj

   dotnet tool run dotnet-ef database update \
     --context AppDbContext \
     --project src/Core/Alphabet.Infrastructure/Alphabet.Infrastructure.csproj \
     --startup-project src/Gateway/Alphabet.AppWire/Alphabet.AppWire.csproj
   ```

3. Start the API. On startup, the module seeds default categories, privileges, and role mappings.

## Configuration

The module reads from these settings:

- `PrivilegeSettings`
- `Authorization`

Key options:

- `CacheEnabled`: enables effective privilege caching
- `CacheDurationMinutes`: cache TTL for user privilege snapshots
- `EnableAuditLogging`: logs privilege creation, assignment, revocation, and checks
- `AdminRoles`: roles allowed to manage privilege administration
- `MaxPrivilegeRequestDurationDays`: max self-service request duration

## Naming Privileges

Use `resource.action` naming by default:

- `user.view`
- `user.delete`
- `report.export`

Best practices:

- keep names stable and immutable
- prefer business capabilities over UI labels
- avoid creating ultra-granular privileges for every screen element

## Using in Endpoints

For minimal APIs, use a policy generated from privilege metadata:

```csharp
.RequireAuthorization("Privilege:user.delete:False")
```

For attributes in MVC-style flows or filters:

```csharp
[PrivilegeAuthorize("user.delete")]
```

For programmatic checks:

```csharp
var result = await privilegeService.CheckPrivilegeAsync(userId, "user.delete", cancellationToken);
if (!result.IsSuccess || !result.Value!.HasPrivilege)
{
    // deny action
}
```

## Migration from Role-Based Authorization

1. Keep existing roles.
2. Seed privilege definitions for the protected capabilities.
3. Map current roles to equivalent privileges.
4. Replace selected role checks with privilege checks endpoint by endpoint.
5. Keep dual mode where needed:
   - roles for broad admin fences
   - privileges for fine-grained capability checks

## Troubleshooting

If a privilege is not evaluating correctly:

- confirm the privilege exists and is not deprecated
- confirm direct deny assignments are not overriding grants
- confirm role assignments are active and not expired
- confirm dependent privileges are also granted
- clear or wait for the privilege cache TTL

If cache invalidation seems stale:

- direct user assignments invalidate that user's cache immediately
- role-based changes rely on normal refresh/TTL unless you explicitly clear caches

If performance becomes a concern:

- keep cache enabled
- prefer batch checks for UI/API aggregates
- index privilege and audit tables by `UserId`, `PrivilegeId`, and `PerformedAt`

## Security Guidance

- follow least privilege
- review privileged access regularly
- watch for unused or overpowered privileges
- require audit review for sensitive operations
- do not let users self-approve privilege requests
