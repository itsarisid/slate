using System.Text;
using System.Security.Claims;
using Alphabet.Application.Common.Authentication;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Infrastructure.BackgroundJobs;
using Alphabet.Infrastructure.Caching;
using Alphabet.Infrastructure.External.Email;
using Alphabet.Infrastructure.Health;
using Alphabet.Infrastructure.Identity;
using Alphabet.Infrastructure.Options;
using Alphabet.Infrastructure.Persistence.Context;
using Alphabet.Infrastructure.Persistence.Repositories;
using Alphabet.Infrastructure.Security;
using Alphabet.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Alphabet.Infrastructure;

/// <summary>
/// Registers infrastructure services and adapters.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure-layer services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        services.Configure<LockoutSettings>(configuration.GetSection(LockoutSettings.SectionName));
        services.Configure<MfaSettings>(configuration.GetSection(MfaSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));
        services.Configure<CommunicationSettings>(configuration.GetSection(CommunicationSettings.SectionName));
        services.Configure<FrontendUrlsSettings>(configuration.GetSection(FrontendUrlsSettings.SectionName));
        services.Configure<CookieAuthenticationSettings>(configuration.GetSection(CookieAuthenticationSettings.SectionName));

        var databaseSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();

        services.AddDbContext<AppDbContext>(options =>
        {
            var provider = databaseSettings.Provider.Trim().ToLowerInvariant();
            if (provider == "postgresql" || provider == "postgres")
            {
                options.UseNpgsql(
                    databaseSettings.ConnectionString,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);
                    });
            }
            else if (provider == "inmemory")
            {
                options.UseInMemoryDatabase("AlphabetDb");
            }
            else
            {
                options.UseSqlServer(
                    databaseSettings.ConnectionString,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();
        services.AddScoped<IInAppNotificationService, InAppNotificationService>();
        services.AddScoped<IWebhookNotificationService, WebhookNotificationService>();
        services.AddScoped<ICommunicationService, CommunicationService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IBackgroundJobService, InProcessBackgroundJobService>();

        var cacheSettings = configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>() ?? new CacheSettings();
        var lockoutSettings = configuration.GetSection(LockoutSettings.SectionName).Get<LockoutSettings>() ?? new LockoutSettings();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutSettings.LockoutDurationMinutes);
                options.Lockout.MaxFailedAccessAttempts = lockoutSettings.MaxFailedAttempts;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        if (cacheSettings.Provider.Equals("Redis", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(cacheSettings.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheSettings.RedisConnectionString;
            });

            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>()
                .AddCheck<RedisHealthCheck>("redis", failureStatus: HealthStatus.Unhealthy);
        }
        else
        {
            services.AddScoped<ICacheService, MemoryCacheService>();
            services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
        }

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        var cookieSettings = configuration.GetSection(CookieAuthenticationSettings.SectionName).Get<CookieAuthenticationSettings>() ?? new CookieAuthenticationSettings();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        services.AddAuthentication(AuthenticationConstants.BearerScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = signingKey,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrWhiteSpace(context.Token) &&
                            context.Request.Cookies.TryGetValue(cookieSettings.AccessTokenCookieName, out var accessToken) &&
                            !string.IsNullOrWhiteSpace(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("CatalogWrite", policy => policy.RequireRole("Admin", "CatalogManager"))
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

        return services;
    }
}
