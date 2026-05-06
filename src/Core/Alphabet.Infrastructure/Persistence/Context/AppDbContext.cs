using Alphabet.Domain.Entities;
using Alphabet.Domain.Entities.Privilege;
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

    public DbSet<Privilege> Privileges => Set<Privilege>();

    public DbSet<PrivilegeCategory> PrivilegeCategories => Set<PrivilegeCategory>();

    public DbSet<RolePrivilege> RolePrivileges => Set<RolePrivilege>();

    public DbSet<UserPrivilege> UserPrivileges => Set<UserPrivilege>();

    public DbSet<PrivilegePolicy> PrivilegePolicies => Set<PrivilegePolicy>();

    public DbSet<PrivilegeRequest> PrivilegeRequests => Set<PrivilegeRequest>();

    public DbSet<PrivilegeAuditLog> PrivilegeAuditLogs => Set<PrivilegeAuditLog>();

    public DbSet<RolePrivilegePolicy> RolePrivilegePolicies => Set<RolePrivilegePolicy>();

    public DbSet<UserPrivilegePolicy> UserPrivilegePolicies => Set<UserPrivilegePolicy>();

    public DbSet<PrivilegeDependency> PrivilegeDependencies => Set<PrivilegeDependency>();

    public DbSet<PrivilegeCondition> PrivilegeConditions => Set<PrivilegeCondition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
