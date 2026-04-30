using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alphabet.Infrastructure.Persistence.Context;

/// <summary>
/// Main application database context.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : AppIdentityDbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<User> LegacyUsers => Set<User>();

    public DbSet<Auth> AuthRecords => Set<Auth>();

    public DbSet<Job> SchedulerJobs => Set<Job>();

    public DbSet<JobExecution> SchedulerJobExecutions => Set<JobExecution>();

    public DbSet<JobHistory> SchedulerJobHistory => Set<JobHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
