using Alphabet.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Alphabet.Infrastructure.Persistence.Context;

/// <summary>
/// Creates <see cref="AppDbContext"/> instances for Entity Framework design-time operations.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Creates the application database context for migrations and other EF tooling commands.
    /// </summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = BuildConfiguration(environment);
        var databaseSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var provider = databaseSettings.Provider.Trim().ToLowerInvariant();

        if (provider is "postgresql" or "postgres")
        {
            optionsBuilder.UseNpgsql(
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
            optionsBuilder.UseInMemoryDatabase("AlphabetDb");
        }
        else
        {
            optionsBuilder.UseSqlServer(
                databaseSettings.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
        }

        return new AppDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration(string environment)
    {
        var startupProjectPath = ResolveStartupProjectPath();

        return new ConfigurationBuilder()
            .SetBasePath(startupProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string ResolveStartupProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidatePaths = new[]
        {
            Path.Combine(currentDirectory, "src", "Gateway", "Alphabet.AppWire"),
            Path.Combine(currentDirectory, "..", "..", "..", "Gateway", "Alphabet.AppWire"),
            currentDirectory
        };

        foreach (var candidatePath in candidatePaths.Select(Path.GetFullPath))
        {
            if (File.Exists(Path.Combine(candidatePath, "appsettings.json")))
            {
                return candidatePath;
            }
        }

        throw new InvalidOperationException("Could not locate the Alphabet.AppWire startup project to load appsettings.json for design-time DbContext creation.");
    }
}
