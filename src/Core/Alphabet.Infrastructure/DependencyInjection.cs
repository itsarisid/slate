using System.Text;
using System.Security.Claims;
using Alphabet.Application.Common.Authentication;
using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Application.Common.Interfaces.Productivity;
using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Interfaces.Productivity;
using Alphabet.Domain.Interfaces.Privilege;
using Alphabet.Infrastructure.BackgroundJobs;
using Alphabet.Infrastructure.Caching;
using Alphabet.Infrastructure.External.Email;
using Alphabet.Infrastructure.Health;
using Alphabet.Infrastructure.Identity;
using Alphabet.Infrastructure.Options;
using Alphabet.Infrastructure.Persistence.Context;
using Alphabet.Infrastructure.Persistence.Repositories;
using Alphabet.Infrastructure.Repositories;
using Alphabet.Infrastructure.Repositories.AssetManagement;
using Alphabet.Infrastructure.Repositories.Privilege;
using Alphabet.Infrastructure.Scheduler;
using Alphabet.Infrastructure.Scheduler.JobHandlers;
using Alphabet.Infrastructure.Security;
using Alphabet.Infrastructure.Services;
using Alphabet.Infrastructure.Services.AssetManagement;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
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
        services.Configure<PrivilegeSettings>(configuration.GetSection(PrivilegeSettings.SectionName));
        services.Configure<PrivilegeAuthorizationSettings>(configuration.GetSection(PrivilegeAuthorizationSettings.SectionName));
        services.Configure<SchedulerSettings>(configuration.GetSection(SchedulerSettings.SectionName));
        services.Configure<ProductivitySettings>(configuration.GetSection(ProductivitySettings.SectionName));
        services.Configure<ProductivityNotificationSettings>(configuration.GetSection(ProductivityNotificationSettings.SectionName));
        services.Configure<ProductivityFileStorageSettings>(configuration.GetSection(ProductivityFileStorageSettings.SectionName));
        services.Configure<RecurrenceSettings>(configuration.GetSection(RecurrenceSettings.SectionName));
        services.Configure<AssetManagementSettings>(configuration.GetSection(AssetManagementSettings.SectionName));
        services.Configure<AssetWorkflowSettings>(configuration.GetSection(AssetWorkflowSettings.SectionName));
        services.Configure<AssetDepreciationSettings>(configuration.GetSection(AssetDepreciationSettings.SectionName));
        services.Configure<AssetNotificationSettings>(configuration.GetSection(AssetNotificationSettings.SectionName));
        services.Configure<AssetBarcodeSettings>(configuration.GetSection(AssetBarcodeSettings.SectionName));
        services.Configure<AssetImportExportSettings>(configuration.GetSection(AssetImportExportSettings.SectionName));
        services.Configure<LeaveManagementSettings>(configuration.GetSection(LeaveManagementSettings.SectionName));

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
        services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
        services.AddScoped<IPrivilegeAuditRepository, PrivilegeAuditRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ILeaveRepository, LeaveRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTime, DateTimeService>();
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
        services.AddScoped<PrivilegeCacheRepository>();
        services.AddScoped<IPrivilegeService, PrivilegeService>();
        services.AddScoped<IPrivilegeEvaluationService, PrivilegeService>();
        services.AddScoped<IBackgroundJobService, InProcessBackgroundJobService>();
        services.AddScoped<IJobExecutionService, JobExecutionService>();
        services.AddScoped<IReminderService, ReminderSchedulerService>();
        services.AddScoped<Alphabet.Application.Common.Interfaces.Productivity.INotificationService, Alphabet.Infrastructure.Services.NotificationService>();
        services.AddScoped<Alphabet.Application.Common.Interfaces.Productivity.IFileStorageService, FileStorageService>();
        services.AddScoped<ICalendarExportService, CalendarExportService>();
        services.AddScoped<IProductivityReadService, ProductivityReadService>();
        services.AddScoped<IAssetTagGenerator, AssetTagGenerator>();
        services.AddScoped<IAssetBarcodeService, AssetBarcodeService>();
        services.AddScoped<IAssetDepreciationService, AssetDepreciationService>();
        services.AddScoped<IAssetUserDirectory, AssetUserDirectory>();
        services.AddScoped<IAssetNotificationService, AssetNotificationService>();
        services.AddScoped<ILeaveCalendarService, LeaveCalendarService>();
        services.AddScoped<ILeaveApproverResolver, LeaveApproverResolver>();
        services.AddScoped<ILeaveNotificationService, LeaveNotificationService>();
        services.AddScoped<ILeaveCalendarSyncService, LeaveCalendarSyncService>();
        services.AddScoped<NoteSearchService>();
        services.AddScoped<ReminderTriggerJob>();
        services.AddScoped<RecurringTaskGeneratorJob>();
        services.AddScoped<TrashCleanupJob>();
        services.AddScoped<ProductivityReportJob>();
        services.AddScoped<CalendarSyncJob>();
        services.AddScoped<WorkflowEscalationJob>();
        services.AddScoped<MaintenanceReminderJob>();
        services.AddScoped<LeaveAccrualJob>();
        services.AddScoped<LeaveApprovalEscalationJob>();
        services.AddScoped<HttpCallJobHandler>();
        services.AddScoped<StoredProcedureJobHandler>();
        services.AddScoped<CodeExecutionJobHandler>();
        services.AddScoped<FileOperationJobHandler>();
        services.AddScoped<JobExecutor>();
        services.AddScoped<ICronExpressionValidator, CronExpressionValidator>();
        services.AddScoped<Alphabet.Infrastructure.Scheduler.ExampleJobs.SampleJob>();
        services.AddScoped<Alphabet.Infrastructure.Scheduler.ExampleJobs.ReportGenerationJob>();
        services.AddScoped<Alphabet.Infrastructure.Scheduler.ExampleJobs.CleanupJob>();
        services.AddHttpClient("Alphabet.Scheduler.Http");
        services.AddHttpClient("Alphabet.Productivity.Notifications");

        var cacheSettings = configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>() ?? new CacheSettings();
        var lockoutSettings = configuration.GetSection(LockoutSettings.SectionName).Get<LockoutSettings>() ?? new LockoutSettings();
        var schedulerSettings = configuration.GetSection(SchedulerSettings.SectionName).Get<SchedulerSettings>() ?? new SchedulerSettings();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();

        var hangfireConnectionString = configuration.GetConnectionString(schedulerSettings.Hangfire.ConnectionStringName ?? "HangfireConnection")
            ?? databaseSettings.ConnectionString;

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    hangfireConnectionString,
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = schedulerSettings.Hangfire.WorkerCount;
            options.Queues = schedulerSettings.Hangfire.Queues;
        });

        services.AddScoped<ISchedulerService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<SchedulerSettings>>().Value;
            return settings.Provider.Equals("Quartz", StringComparison.OrdinalIgnoreCase)
                ? new QuartzSchedulerService()
                : ActivatorUtilities.CreateInstance<HangfireSchedulerService>(sp);
        });

        // Use AddIdentityCore instead of AddIdentity to avoid overriding
        // the default authentication scheme. AddIdentity internally calls
        // AddAuthentication and sets the default to Identity.Application
        // (cookie-based), which causes JWT Bearer tokens to be ignored and
        // all protected endpoints to return 401.
        services.AddIdentityCore<ApplicationUser>(options =>
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
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
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

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AuthenticationConstants.BearerScheme;
            options.DefaultChallengeScheme = AuthenticationConstants.BearerScheme;
            options.DefaultScheme = AuthenticationConstants.BearerScheme;
        })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
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
                        var authorizationHeader = context.Request.Headers.Authorization.ToString();
                        if (!string.IsNullOrWhiteSpace(authorizationHeader))
                        {
                            var normalizedHeader = authorizationHeader.Trim().Trim('"', '\'');
                            if (normalizedHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                normalizedHeader = normalizedHeader["Bearer ".Length..].Trim().Trim('"', '\'');
                            }

                            if (normalizedHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                normalizedHeader = normalizedHeader["Bearer ".Length..].Trim().Trim('"', '\'');
                            }

                            if (!string.IsNullOrWhiteSpace(normalizedHeader))
                            {
                                context.Token = normalizedHeader;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(context.Token) &&
                            context.Request.Cookies.TryGetValue(cookieSettings.AccessTokenCookieName, out var accessToken) &&
                            !string.IsNullOrWhiteSpace(accessToken))
                        {
                            context.Token = accessToken.Trim().Trim('"', '\'');
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("Alphabet.Authentication");

                        logger.LogInformation(
                            "JWT token validated successfully for {Path}. UserId={UserId}",
                            context.HttpContext.Request.Path,
                            context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier));

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("Alphabet.Authentication");

                        logger.LogWarning(
                            context.Exception,
                            "JWT authentication failed for {Path}: {Message}",
                            context.HttpContext.Request.Path,
                            context.Exception.Message);

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("Alphabet.Authentication");

                        logger.LogWarning(
                            "JWT challenge triggered for {Path}. Error={Error}, Description={Description}",
                            context.HttpContext.Request.Path,
                            context.Error,
                            context.ErrorDescription);

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("CatalogWrite", policy => policy.RequireRole("Admin", "CatalogManager"))
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("PrivilegeManagers", policy => policy.RequireRole("Admin", "PrivilegeManager"))
            .AddPolicy("SchedulerViewer", policy => policy.RequireRole("Admin", "Viewer"))
            .AddPolicy("SchedulerOperator", policy => policy.RequireRole("Admin", "User"));

        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, PrivilegePolicyProvider>();
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PrivilegeAuthorizationHandler>();

        return services;
    }
}
