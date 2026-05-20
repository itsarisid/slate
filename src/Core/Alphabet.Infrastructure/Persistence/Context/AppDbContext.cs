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

    public DbSet<Todo> ProductivityTodos => Set<Todo>();

    public DbSet<Reminder> ProductivityReminders => Set<Reminder>();

    public DbSet<Note> ProductivityNotes => Set<Note>();

    public DbSet<Notebook> ProductivityNotebooks => Set<Notebook>();

    public DbSet<ProductivityTask> ProductivityTasks => Set<ProductivityTask>();

    public DbSet<CalendarEvent> ProductivityCalendarEvents => Set<CalendarEvent>();

    public DbSet<Attachment> ProductivityAttachments => Set<Attachment>();

    public DbSet<Comment> ProductivityComments => Set<Comment>();

    public DbSet<Tag> ProductivityTags => Set<Tag>();

    public DbSet<SmartList> ProductivitySmartLists => Set<SmartList>();

    public DbSet<ProductivityTemplate> ProductivityTemplates => Set<ProductivityTemplate>();

    public DbSet<TimeEntry> ProductivityTimeEntries => Set<TimeEntry>();

    public DbSet<TaskDependency> ProductivityTaskDependencies => Set<TaskDependency>();
    /// <summary>
    /// On model creating.
    /// </summary>

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
