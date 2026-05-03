using Alphabet.AppWire.Configuration;
using Alphabet.AppWire.Middleware;
using Alphabet.Application;
using Alphabet.Infrastructure;
using Alphabet.Infrastructure.Identity;
using Alphabet.Infrastructure.Logging;
using Alphabet.Infrastructure.Options;
using Alphabet.Infrastructure.Scheduler;
using Alphabet.Modules.CommunicationModule.Api;
using Alphabet.Modules.IdentityModule.Api;
using Alphabet.Modules.ProductModule.Api;
using Alphabet.Modules.SchedulerModule.Api;
using Asp.Versioning;
using Hangfire;
using Serilog;
using AspNetCore.Swagger.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
{
    SerilogSetup.Configure(loggerConfiguration, context.Configuration);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerSetup>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DefaultCors",
        policy => policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["https://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

//await ApplyDatabaseMigrationsAsync(app);
await IdentitySetup.SeedAsync(app.Services);

app.UseSerilogRequestLogging();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers.XXSSProtection = "1; mode=block";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapCommunicationModule();
app.MapProductModule();
app.MapIdentityModule();
app.MapSchedulerModule();

var schedulerSettings = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SchedulerSettings>>().Value;
if (schedulerSettings.Provider.Equals("Hangfire", StringComparison.OrdinalIgnoreCase) &&
    schedulerSettings.Hangfire.DashboardEnabled)
{
    app.UseHangfireDashboard(
        schedulerSettings.Hangfire.DashboardPath,
        new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter()]
        });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // Apply a theme
    //app.UseSwaggerUI(Theme.Dark);

    // Enable runtime theme switcher!
    app.UseSwaggerUI(Theme.Dark, c =>
    {
        c.EnableThemeSwitcher();
        c.DocumentTitle = "Alphabet API Docs";
    });
}

app.Run();
