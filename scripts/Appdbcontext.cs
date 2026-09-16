using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Alphabet.Data.Entities.Identity;
using Alphabet.Data.Entities.Assets;
using Alphabet.Data.Entities.Leave;
using Alphabet.Data.Entities.Privileges;
using Alphabet.Data.Entities.Productivity;
using Alphabet.Data.Entities.Scheduling;
using Alphabet.Data.Entities.Common;

namespace Alphabet.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<AssetActivityLog> AssetActivityLogs => Set<AssetActivityLog>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<AssetLocation> AssetLocations => Set<AssetLocation>();
    public DbSet<AssetMaintenanceRecord> AssetMaintenanceRecords => Set<AssetMaintenanceRecord>();
    public DbSet<AssetMovement> AssetMovements => Set<AssetMovement>();
    public DbSet<AssetReservation> AssetReservations => Set<AssetReservation>();
    public DbSet<AssetWorkflowDefinition> AssetWorkflowDefinitions => Set<AssetWorkflowDefinition>();
    public DbSet<AssetWorkflowInstance> AssetWorkflowInstances => Set<AssetWorkflowInstance>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuthRecord> AuthRecords => Set<AuthRecord>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<LeaveAccrualRule> LeaveAccrualRules => Set<LeaveAccrualRule>();
    public DbSet<LeaveActivityLog> LeaveActivityLogs => Set<LeaveActivityLog>();
    public DbSet<LeaveApprovalChain> LeaveApprovalChains => Set<LeaveApprovalChain>();
    public DbSet<LeaveApprovalWorkflow> LeaveApprovalWorkflows => Set<LeaveApprovalWorkflow>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveBlackoutPeriod> LeaveBlackoutPeriods => Set<LeaveBlackoutPeriod>();
    public DbSet<LeaveDelegation> LeaveDelegations => Set<LeaveDelegation>();
    public DbSet<LeavePublicHoliday> LeavePublicHolidays => Set<LeavePublicHoliday>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveWorkflowStep> LeaveWorkflowSteps => Set<LeaveWorkflowStep>();
    public DbSet<LegacyUser> LegacyUsers => Set<LegacyUser>();
    public DbSet<PrivilegeAuditLog> PrivilegeAuditLogs => Set<PrivilegeAuditLog>();
    public DbSet<PrivilegeCategory> PrivilegeCategories => Set<PrivilegeCategory>();
    public DbSet<PrivilegeCondition> PrivilegeConditions => Set<PrivilegeCondition>();
    public DbSet<PrivilegeDependency> PrivilegeDependencies => Set<PrivilegeDependency>();
    public DbSet<PrivilegePolicy> PrivilegePolicies => Set<PrivilegePolicy>();
    public DbSet<PrivilegeRequest> PrivilegeRequests => Set<PrivilegeRequest>();
    public DbSet<Privilege> Privileges => Set<Privilege>();
    public DbSet<ProductivityAttachment> ProductivityAttachments => Set<ProductivityAttachment>();
    public DbSet<ProductivityCalendarEvent> ProductivityCalendarEvents => Set<ProductivityCalendarEvent>();
    public DbSet<ProductivityComment> ProductivityComments => Set<ProductivityComment>();
    public DbSet<ProductivityNotebook> ProductivityNotebooks => Set<ProductivityNotebook>();
    public DbSet<ProductivityNote> ProductivityNotes => Set<ProductivityNote>();
    public DbSet<ProductivityReminder> ProductivityReminders => Set<ProductivityReminder>();
    public DbSet<ProductivitySmartList> ProductivitySmartLists => Set<ProductivitySmartList>();
    public DbSet<ProductivityTag> ProductivityTags => Set<ProductivityTag>();
    public DbSet<ProductivityTaskDependency> ProductivityTaskDependencies => Set<ProductivityTaskDependency>();
    public DbSet<ProductivityTask> ProductivityTasks => Set<ProductivityTask>();
    public DbSet<ProductivityTemplate> ProductivityTemplates => Set<ProductivityTemplate>();
    public DbSet<ProductivityTimeEntry> ProductivityTimeEntries => Set<ProductivityTimeEntry>();
    public DbSet<ProductivityTodo> ProductivityTodos => Set<ProductivityTodo>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RolePrivilegePolicy> RolePrivilegePolicies => Set<RolePrivilegePolicy>();
    public DbSet<RolePrivilege> RolePrivileges => Set<RolePrivilege>();
    public DbSet<SchedulerJobExecution> SchedulerJobExecutions => Set<SchedulerJobExecution>();
    public DbSet<SchedulerJobHistory> SchedulerJobHistories => Set<SchedulerJobHistory>();
    public DbSet<SchedulerJob> SchedulerJobs => Set<SchedulerJob>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<UserPrivilegePolicy> UserPrivilegePolicies => Set<UserPrivilegePolicy>();
    public DbSet<UserPrivilege> UserPrivileges => Set<UserPrivilege>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- ASP.NET Core Identity table remaps (script uses "Users" instead of default "AspNetUsers") ----
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            b.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            b.Property(u => u.TwoFactorMethod).HasMaxLength(32).IsRequired();
            b.Property(u => u.OtpDestination).HasMaxLength(256);
            b.HasIndex(u => u.LastLoginAt);
        });
        modelBuilder.Entity<ApplicationRole>(b => b.ToTable("AspNetRoles"));

        // ---- AssetActivityLogs ----
        modelBuilder.Entity<AssetActivityLog>(b =>
        {
            b.ToTable("AssetActivityLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(100).IsRequired();
            b.Property(x => x.Timestamp).IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(64);
            b.Property(x => x.UserAgent).HasMaxLength(1000);
            b.Property(x => x.Reason).HasMaxLength(2000);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.AssetId);
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AssetAssignments ----
        modelBuilder.Entity<AssetAssignment>(b =>
        {
            b.ToTable("AssetAssignments");
            b.HasKey(x => x.Id);
            b.Property(x => x.AssetId).IsRequired();
            b.Property(x => x.AssignedToUserId).IsRequired();
            b.Property(x => x.AssignedByUserId).IsRequired();
            b.Property(x => x.AssignedAt).IsRequired();
            b.Property(x => x.AssignmentType).HasMaxLength(32).IsRequired();
            b.Property(x => x.ConditionAtAssignment).HasMaxLength(32).IsRequired();
            b.Property(x => x.ConditionOnReturn).HasMaxLength(32);
            b.Property(x => x.Purpose).HasMaxLength(500);
            b.Property(x => x.Notes).HasMaxLength(2000);
            b.Property(x => x.DamageNotes).HasMaxLength(2000);
            b.Property(x => x.MissingItemsJson).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.AssetId);
            b.HasIndex(x => x.AssignedToUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.AssignedByUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ReturnedByUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReturnedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ReceivedByUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReceivedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AssetCategories ----
        modelBuilder.Entity<AssetCategory>(b =>
        {
            b.ToTable("AssetCategories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.CustomFieldsSchemaJson).IsRequired();
            b.Property(x => x.DepreciationRate).HasPrecision(6, 2);
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.ParentCategoryId);
            b.HasIndex(x => x.DefaultLocationId);
            b.HasOne<AssetLocation>().WithMany().HasForeignKey(x => x.DefaultLocationId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AssetLocations ----
        modelBuilder.Entity<AssetLocation>(b =>
        {
            b.ToTable("AssetLocations");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Code).HasMaxLength(50).IsRequired();
            b.Property(x => x.Type).HasMaxLength(32).IsRequired();
            b.Property(x => x.Street).HasMaxLength(200).IsRequired();
            b.Property(x => x.City).HasMaxLength(100).IsRequired();
            b.Property(x => x.State).HasMaxLength(100).IsRequired();
            b.Property(x => x.PostalCode).HasMaxLength(20).IsRequired();
            b.Property(x => x.Country).HasMaxLength(100).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.Latitude).HasPrecision(10, 6);
            b.Property(x => x.Longitude).HasPrecision(10, 6);
            b.Property(x => x.ContactPerson).HasMaxLength(200);
            b.Property(x => x.ContactPhone).HasMaxLength(50);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.ParentLocationId);
            b.HasIndex(x => x.Code).IsUnique();
        });

        // ---- AssetMaintenanceRecords ----
        modelBuilder.Entity<AssetMaintenanceRecord>(b =>
        {
            b.ToTable("AssetMaintenanceRecords");
            b.HasKey(x => x.Id);
            b.Property(x => x.AssetId).IsRequired();
            b.Property(x => x.MaintenanceType).HasMaxLength(32).IsRequired();
            b.Property(x => x.ScheduledDate).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.AssignedToVendor).HasMaxLength(200);
            b.Property(x => x.EstimatedCost).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.Priority).HasMaxLength(32).IsRequired();
            b.Property(x => x.ActualCost).HasPrecision(18, 2);
            b.Property(x => x.Notes).HasMaxLength(4000);
            b.Property(x => x.IsCompleted).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.AssetId);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AssetMovements ----
        modelBuilder.Entity<AssetMovement>(b =>
        {
            b.ToTable("AssetMovements");
            b.HasKey(x => x.Id);
            b.Property(x => x.AssetId).IsRequired();
            b.Property(x => x.ToLocationId).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            b.Property(x => x.MovedByUserId).IsRequired();
            b.Property(x => x.MovedAt).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.AssetId);
            b.HasIndex(x => x.FromLocationId);
            b.HasIndex(x => x.ToLocationId);
            b.HasIndex(x => x.MovedByUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.MovedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AssetLocation>().WithMany().HasForeignKey(x => x.FromLocationId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AssetLocation>().WithMany().HasForeignKey(x => x.ToLocationId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AssetReservations ----
        modelBuilder.Entity<AssetReservation>(b =>
        {
            b.ToTable("AssetReservations");
            b.HasKey(x => x.Id);
            b.Property(x => x.AssetId).IsRequired();
            b.Property(x => x.ReservedByUserId).IsRequired();
            b.Property(x => x.StartAt).IsRequired();
            b.Property(x => x.EndAt).IsRequired();
            b.Property(x => x.Purpose).HasMaxLength(1000);
            b.Property(x => x.IsCancelled).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.AssetId);
            b.HasIndex(x => x.ReservedByUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReservedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AssetWorkflowDefinitions ----
        modelBuilder.Entity<AssetWorkflowDefinition>(b =>
        {
            b.ToTable("AssetWorkflowDefinitions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            b.Property(x => x.Version).IsRequired();
            b.Property(x => x.StepsJson).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
        });

        // ---- AssetWorkflowInstances ----
        modelBuilder.Entity<AssetWorkflowInstance>(b =>
        {
            b.ToTable("AssetWorkflowInstances");
            b.HasKey(x => x.Id);
            b.Property(x => x.WorkflowDefinitionId).IsRequired();
            b.Property(x => x.AssetId).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.ContextJson).IsRequired();
            b.Property(x => x.InitiatedByUserId).IsRequired();
            b.Property(x => x.InitiatedAt).IsRequired();
            b.Property(x => x.StepsJson).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.WorkflowDefinitionId);
            b.HasIndex(x => x.AssetId);
            b.HasIndex(x => x.CurrentStepId);
            b.HasIndex(x => x.InitiatedByUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.InitiatedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AssetWorkflowDefinition>().WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Assets ----
        modelBuilder.Entity<Asset>(b =>
        {
            b.ToTable("Assets");
            b.HasKey(x => x.Id);
            b.Property(x => x.AssetTag).HasMaxLength(50).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            b.Property(x => x.CategoryId).IsRequired();
            b.Property(x => x.Subcategory).HasMaxLength(100);
            b.Property(x => x.Manufacturer).HasMaxLength(100);
            b.Property(x => x.Model).HasMaxLength(100);
            b.Property(x => x.SerialNumber).HasMaxLength(100);
            b.Property(x => x.Cost).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.Condition).HasMaxLength(32).IsRequired();
            b.Property(x => x.LocationId).IsRequired();
            b.Property(x => x.CustomFieldsJson).IsRequired();
            b.Property(x => x.ImagesJson).IsRequired();
            b.Property(x => x.DocumentsJson).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.CategoryId);
            b.HasIndex(x => x.LocationId);
            b.HasIndex(x => x.SupplierId);
            b.HasIndex(x => x.AssignedToUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.AssetTag).IsUnique();
            b.HasOne<AssetCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AssetLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AuditLogs ----
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(128).IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(64);
            b.Property(x => x.UserAgent).HasMaxLength(512);
            b.Property(x => x.Timestamp).IsRequired();
            b.Property(x => x.Success).IsRequired();
            b.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- AuthRecords ----
        modelBuilder.Entity<AuthRecord>(b =>
        {
            b.ToTable("AuthRecords");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- InventoryBalances ----
        modelBuilder.Entity<InventoryBalance>(b =>
        {
            b.ToTable("InventoryBalances");
            b.HasKey(x => x.Id);
            b.Property(x => x.AssetId).IsRequired();
            b.Property(x => x.LocationId).IsRequired();
            b.Property(x => x.QuantityOnHand).IsRequired();
            b.Property(x => x.MinimumThreshold).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.AssetId);
            b.HasIndex(x => x.LocationId);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AssetLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeaveAccrualRules ----
        modelBuilder.Entity<LeaveAccrualRule>(b =>
        {
            b.ToTable("LeaveAccrualRules");
            b.HasKey(x => x.Id);
            b.Property(x => x.LeaveTypeId).IsRequired();
            b.Property(x => x.AccrualMethod).HasMaxLength(40).IsRequired();
            b.Property(x => x.AccrualRate).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.MaxAccrual).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.TenureRulesJson).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.LeaveTypeId);
            b.HasOne<LeaveType>().WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeaveActivityLogs ----
        modelBuilder.Entity<LeaveActivityLog>(b =>
        {
            b.ToTable("LeaveActivityLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(100).IsRequired();
            b.Property(x => x.Timestamp).IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(100);
            b.Property(x => x.UserAgent).HasMaxLength(500);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.LeaveRequestId);
            b.HasOne<LeaveRequest>().WithMany().HasForeignKey(x => x.LeaveRequestId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeaveApprovalChains ----
        modelBuilder.Entity<LeaveApprovalChain>(b =>
        {
            b.ToTable("LeaveApprovalChains");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Code).HasMaxLength(60).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.ApplicableToJson).IsRequired();
            b.Property(x => x.ApprovalLevelsJson).IsRequired();
            b.Property(x => x.FinalApprovalLevel).IsRequired();
            b.Property(x => x.AllowSkipLevels).IsRequired();
            b.Property(x => x.ParallelApproval).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        // ---- LeaveApprovalWorkflows ----
        modelBuilder.Entity<LeaveApprovalWorkflow>(b =>
        {
            b.ToTable("LeaveApprovalWorkflows");
            b.HasKey(x => x.Id);
            b.Property(x => x.LeaveRequestId).IsRequired();
            b.Property(x => x.ApprovalChainId).IsRequired();
            b.Property(x => x.Status).HasMaxLength(40).IsRequired();
            b.Property(x => x.CurrentLevel).IsRequired();
            b.Property(x => x.InitiatedAt).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.LeaveRequestId);
            b.HasIndex(x => x.ApprovalChainId);
            b.HasOne<LeaveRequest>().WithMany().HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<LeaveApprovalChain>().WithMany().HasForeignKey(x => x.ApprovalChainId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeaveBalances ----
        modelBuilder.Entity<LeaveBalance>(b =>
        {
            b.ToTable("LeaveBalances");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.LeaveTypeId).IsRequired();
            b.Property(x => x.Year).IsRequired();
            b.Property(x => x.Allocated).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.Taken).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.Pending).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.Approved).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.Remaining).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.CarryForward).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.LeaveTypeId);
            b.HasOne<LeaveType>().WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeaveBlackoutPeriods ----
        modelBuilder.Entity<LeaveBlackoutPeriod>(b =>
        {
            b.ToTable("LeaveBlackoutPeriods");
            b.HasKey(x => x.Id);
            b.Property(x => x.StartDate).IsRequired();
            b.Property(x => x.EndDate).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            b.Property(x => x.ApplicableToJson).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
        });

        // ---- LeaveDelegations ----
        modelBuilder.Entity<LeaveDelegation>(b =>
        {
            b.ToTable("LeaveDelegations");
            b.HasKey(x => x.Id);
            b.Property(x => x.DelegatorUserId).IsRequired();
            b.Property(x => x.DelegateToUserId).IsRequired();
            b.Property(x => x.DelegationType).HasMaxLength(40).IsRequired();
            b.Property(x => x.Permission).HasMaxLength(40).IsRequired();
            b.Property(x => x.ApplicableLeaveTypesJson).IsRequired();
            b.Property(x => x.ApplicableApprovalLevelsJson).IsRequired();
            b.Property(x => x.ApplicableDepartmentsJson).IsRequired();
            b.Property(x => x.ApplicableEmployeesJson).IsRequired();
            b.Property(x => x.StartDate).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.DelegatorUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.DelegatorUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.DelegateToUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.DelegateToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeavePublicHolidays ----
        modelBuilder.Entity<LeavePublicHoliday>(b =>
        {
            b.ToTable("LeavePublicHolidays");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Date).IsRequired();
            b.Property(x => x.Country).HasMaxLength(10).IsRequired();
            b.Property(x => x.State).HasMaxLength(50);
            b.Property(x => x.IsPaid).IsRequired();
            b.Property(x => x.Recurring).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
        });

        // ---- LeaveRequests ----
        modelBuilder.Entity<LeaveRequest>(b =>
        {
            b.ToTable("LeaveRequests");
            b.HasKey(x => x.Id);
            b.Property(x => x.LeaveTypeId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.StartDate).IsRequired();
            b.Property(x => x.EndDate).IsRequired();
            b.Property(x => x.PartialDaysJson).IsRequired();
            b.Property(x => x.TotalDays).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            b.Property(x => x.Status).HasMaxLength(40).IsRequired();
            b.Property(x => x.CurrentApprovalLevel).IsRequired();
            b.Property(x => x.AppliedAt).IsRequired();
            b.Property(x => x.CancelledReason).HasMaxLength(2000);
            b.Property(x => x.AttachmentIdsJson).IsRequired();
            b.Property(x => x.ContactNumber).HasMaxLength(50);
            b.Property(x => x.AlternateArrangements).HasMaxLength(2000);
            b.Property(x => x.ApplyToAllDays).IsRequired();
            b.Property(x => x.IsHalfDay).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.LeaveTypeId);
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<LeaveType>().WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeaveTypes ----
        modelBuilder.Entity<LeaveType>(b =>
        {
            b.ToTable("LeaveTypes");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Code).HasMaxLength(40).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.Color).HasMaxLength(20).IsRequired();
            b.Property(x => x.Icon).HasMaxLength(100);
            b.Property(x => x.IsPaid).IsRequired();
            b.Property(x => x.DefaultDaysPerYear).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.MinDaysPerRequest).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.MaxDaysPerRequest).HasPrecision(10, 2);
            b.Property(x => x.RequiresApproval).IsRequired();
            b.Property(x => x.CarryForwardEnabled).IsRequired();
            b.Property(x => x.MaxCarryForwardDays).HasPrecision(10, 2).IsRequired();
            b.Property(x => x.CarryForwardExpiryMonths).IsRequired();
            b.Property(x => x.EncashmentEnabled).IsRequired();
            b.Property(x => x.EncashmentRate).HasPrecision(10, 2);
            b.Property(x => x.ProrationEnabled).IsRequired();
            b.Property(x => x.EligibilityRulesJson).IsRequired();
            b.Property(x => x.BlackoutDatesJson).IsRequired();
            b.Property(x => x.RequiresAttachment).IsRequired();
            b.Property(x => x.AllowedAttachmentTypesJson).IsRequired();
            b.Property(x => x.AutoApproveRulesJson).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.ApprovalChainId);
            b.HasOne<LeaveApprovalChain>().WithMany().HasForeignKey(x => x.ApprovalChainId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LeaveWorkflowSteps ----
        modelBuilder.Entity<LeaveWorkflowStep>(b =>
        {
            b.ToTable("LeaveWorkflowSteps");
            b.HasKey(x => x.Id);
            b.Property(x => x.WorkflowId).IsRequired();
            b.Property(x => x.Level).IsRequired();
            b.Property(x => x.ApproverType).HasMaxLength(50).IsRequired();
            b.Property(x => x.ApproverValue).HasMaxLength(250).IsRequired();
            b.Property(x => x.Status).HasMaxLength(40).IsRequired();
            b.Property(x => x.Action).HasMaxLength(60);
            b.Property(x => x.Comment).HasMaxLength(2000);
            b.Property(x => x.AttachmentsJson).IsRequired();
            b.Property(x => x.AssignedAt).IsRequired();
            b.Property(x => x.TimeoutHours).IsRequired();
            b.Property(x => x.IsEscalated).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.WorkflowId);
            b.HasIndex(x => x.ApproverUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ApproverUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<LeaveApprovalWorkflow>().WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- LegacyUsers ----
        modelBuilder.Entity<LegacyUser>(b =>
        {
            b.ToTable("LegacyUsers");
            b.HasKey(x => x.Id);
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
        });

        // ---- PrivilegeAuditLogs ----
        modelBuilder.Entity<PrivilegeAuditLog>(b =>
        {
            b.ToTable("PrivilegeAuditLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(32).IsRequired();
            b.Property(x => x.Source).HasMaxLength(64).IsRequired();
            b.Property(x => x.PerformedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.PerformedAt).IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(128);
            b.Property(x => x.MetadataJson).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.PrivilegeId);
            b.HasOne<Privilege>().WithMany().HasForeignKey(x => x.PrivilegeId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- PrivilegeCategories ----
        modelBuilder.Entity<PrivilegeCategory>(b =>
        {
            b.ToTable("PrivilegeCategories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.SortOrder).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.ParentCategoryId);
        });

        // ---- PrivilegeConditions ----
        modelBuilder.Entity<PrivilegeCondition>(b =>
        {
            b.ToTable("PrivilegeConditions");
            b.HasKey(x => x.Id);
            b.Property(x => x.PrivilegeId).IsRequired();
            b.Property(x => x.AttributeName).HasMaxLength(128).IsRequired();
            b.Property(x => x.Operator).HasMaxLength(64).IsRequired();
            b.Property(x => x.AttributeValue).HasMaxLength(512).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.PrivilegeId);
            b.HasOne<Privilege>().WithMany().HasForeignKey(x => x.PrivilegeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- PrivilegeDependencies ----
        modelBuilder.Entity<PrivilegeDependency>(b =>
        {
            b.ToTable("PrivilegeDependencies");
            b.HasKey(x => new { x.PrivilegeId, x.DependsOnPrivilegeId });
            b.Property(x => x.PrivilegeId).IsRequired();
            b.Property(x => x.DependsOnPrivilegeId).IsRequired();
            b.HasIndex(x => x.PrivilegeId);
            b.HasIndex(x => x.DependsOnPrivilegeId);
            b.HasOne<Privilege>().WithMany().HasForeignKey(x => x.PrivilegeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Privilege>().WithMany().HasForeignKey(x => x.DependsOnPrivilegeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- PrivilegePolicies ----
        modelBuilder.Entity<PrivilegePolicy>(b =>
        {
            b.ToTable("PrivilegePolicies");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.PrivilegeNamesJson).IsRequired();
            b.Property(x => x.Condition).HasMaxLength(32).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
        });

        // ---- PrivilegeRequests ----
        modelBuilder.Entity<PrivilegeRequest>(b =>
        {
            b.ToTable("PrivilegeRequests");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.PrivilegeId).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            b.Property(x => x.RequestedDurationDays).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.ApproverEmail).HasMaxLength(256);
            b.Property(x => x.DecisionNotes).HasMaxLength(1000);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.PrivilegeId);
            b.HasIndex(x => x.ApproverId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ApproverId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Privilege>().WithMany().HasForeignKey(x => x.PrivilegeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Privileges ----
        modelBuilder.Entity<Privilege>(b =>
        {
            b.ToTable("Privileges");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.ResourceType).HasMaxLength(100);
            b.Property(x => x.AllowedActionsJson).IsRequired();
            b.Property(x => x.IsGlobal).IsRequired();
            b.Property(x => x.IsDeprecated).IsRequired();
            b.Property(x => x.DependsOnJson).IsRequired();
            b.Property(x => x.AttributesJson).IsRequired();
            b.Property(x => x.CreatedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.UpdatedBy).HasMaxLength(256);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.CategoryId);
            b.HasOne<AssetCategory>().WithMany().HasForeignKey(x => x.CategoryId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityAttachments ----
        modelBuilder.Entity<ProductivityAttachment>(b =>
        {
            b.ToTable("ProductivityAttachments");
            b.HasKey(x => x.Id);
            b.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            b.Property(x => x.EntityId).IsRequired();
            b.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            b.Property(x => x.ContentType).HasMaxLength(256).IsRequired();
            b.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            b.Property(x => x.SizeBytes).IsRequired();
            b.Property(x => x.UploadedByUserId).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.EntityId);
            b.HasIndex(x => x.UploadedByUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityCalendarEvents ----
        modelBuilder.Entity<ProductivityCalendarEvent>(b =>
        {
            b.ToTable("ProductivityCalendarEvents");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            b.Property(x => x.Location).HasMaxLength(200);
            b.Property(x => x.IsAllDay).IsRequired();
            b.Property(x => x.StartTime).IsRequired();
            b.Property(x => x.EndTime).IsRequired();
            b.Property(x => x.Timezone).HasMaxLength(128).IsRequired();
            b.Property(x => x.ConferenceLink).HasMaxLength(500);
            b.Property(x => x.Visibility).HasMaxLength(32).IsRequired();
            b.Property(x => x.Color).HasMaxLength(32);
            b.Property(x => x.AttendeesJson).IsRequired();
            b.Property(x => x.ReminderMinutesJson).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityComments ----
        modelBuilder.Entity<ProductivityComment>(b =>
        {
            b.ToTable("ProductivityComments");
            b.HasKey(x => x.Id);
            b.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            b.Property(x => x.EntityId).IsRequired();
            b.Property(x => x.AuthorUserId).IsRequired();
            b.Property(x => x.Content).HasMaxLength(4000).IsRequired();
            b.Property(x => x.IsEdited).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.EntityId);
            b.HasIndex(x => x.AuthorUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ParentCommentId);
        });

        // ---- ProductivityNotebooks ----
        modelBuilder.Entity<ProductivityNotebook>(b =>
        {
            b.ToTable("ProductivityNotebooks");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Color).HasMaxLength(32);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ParentNotebookId);
        });

        // ---- ProductivityNotes ----
        modelBuilder.Entity<ProductivityNote>(b =>
        {
            b.ToTable("ProductivityNotes");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Content).IsRequired();
            b.Property(x => x.Format).HasMaxLength(32).IsRequired();
            b.Property(x => x.Category).HasMaxLength(100);
            b.Property(x => x.Color).HasMaxLength(32);
            b.Property(x => x.IsPinned).IsRequired();
            b.Property(x => x.IsFavorite).IsRequired();
            b.Property(x => x.CollaboratorsJson).IsRequired();
            b.Property(x => x.VersionHistoryJson).IsRequired();
            b.Property(x => x.VersionNumber).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.NotebookId);
            b.HasOne<ProductivityNotebook>().WithMany().HasForeignKey(x => x.NotebookId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityReminders ----
        modelBuilder.Entity<ProductivityReminder>(b =>
        {
            b.ToTable("ProductivityReminders");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            b.Property(x => x.ReminderTime).IsRequired();
            b.Property(x => x.ReminderType).HasMaxLength(32).IsRequired();
            b.Property(x => x.SoundEnabled).IsRequired();
            b.Property(x => x.VibrationEnabled).IsRequired();
            b.Property(x => x.SnoozeEnabled).IsRequired();
            b.Property(x => x.LinkedEntityType).HasMaxLength(64);
            b.Property(x => x.NotificationMethodsJson).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.LinkedEntityId);
        });

        // ---- ProductivitySmartLists ----
        modelBuilder.Entity<ProductivitySmartList>(b =>
        {
            b.ToTable("ProductivitySmartLists");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            b.Property(x => x.CriteriaJson).IsRequired();
            b.Property(x => x.IsShared).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityTags ----
        modelBuilder.Entity<ProductivityTag>(b =>
        {
            b.ToTable("ProductivityTags");
            b.HasKey(x => x.Id);
            b.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            b.Property(x => x.EntityId).IsRequired();
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Color).HasMaxLength(32);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.EntityId);
        });

        // ---- ProductivityTaskDependencies ----
        modelBuilder.Entity<ProductivityTaskDependency>(b =>
        {
            b.ToTable("ProductivityTaskDependencies");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProductivityTaskId).IsRequired();
            b.Property(x => x.DependsOnTaskId).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.ProductivityTaskId);
            b.HasIndex(x => x.DependsOnTaskId);
            b.HasOne<ProductivityTask>().WithMany().HasForeignKey(x => x.ProductivityTaskId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProductivityTask>().WithMany().HasForeignKey(x => x.DependsOnTaskId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityTasks ----
        modelBuilder.Entity<ProductivityTask>(b =>
        {
            b.ToTable("ProductivityTasks");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            b.Property(x => x.Priority).HasMaxLength(32).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.EstimatedHours).HasPrecision(18, 2);
            b.Property(x => x.ActualHours).HasPrecision(18, 2);
            b.Property(x => x.ChecklistJson).IsRequired();
            b.Property(x => x.CommentsJson).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.AssigneeId);
            b.HasIndex(x => x.ReviewerId);
            b.HasIndex(x => x.ParentTaskId);
            b.HasIndex(x => x.ProjectId);
        });

        // ---- ProductivityTemplates ----
        modelBuilder.Entity<ProductivityTemplate>(b =>
        {
            b.ToTable("ProductivityTemplates");
            b.HasKey(x => x.Id);
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.TemplateJson).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityTimeEntries ----
        modelBuilder.Entity<ProductivityTimeEntry>(b =>
        {
            b.ToTable("ProductivityTimeEntries");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProductivityTaskId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.StartTime).IsRequired();
            b.Property(x => x.EndTime).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.ProductivityTaskId);
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProductivityTask>().WithMany().HasForeignKey(x => x.ProductivityTaskId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProductivityTodos ----
        modelBuilder.Entity<ProductivityTodo>(b =>
        {
            b.ToTable("ProductivityTodos");
            b.HasKey(x => x.Id);
            b.Property(x => x.CreatedByUserId).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            b.Property(x => x.Priority).HasMaxLength(32).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.Category).HasMaxLength(100);
            b.Property(x => x.IsRecurring).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.CreatedByUserId);
            b.HasIndex(x => x.AssignedToUserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.ConvertedTaskId);
        });

        // ---- Products ----
        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            b.Property(x => x.PriceAmount).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.PriceCurrency).HasMaxLength(3).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
        });

        // ---- RefreshTokens ----
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Token).HasMaxLength(512).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
            b.Property(x => x.IsRevoked).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.CreatedByIp).HasMaxLength(128);
            b.Property(x => x.RevokedByIp).HasMaxLength(128);
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- RolePrivilegePolicies ----
        modelBuilder.Entity<RolePrivilegePolicy>(b =>
        {
            b.ToTable("RolePrivilegePolicies");
            b.HasKey(x => x.Id);
            b.Property(x => x.RoleId).IsRequired();
            b.Property(x => x.PolicyId).IsRequired();
            b.Property(x => x.GrantedAt).IsRequired();
            b.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.RoleId);
            b.HasOne<ApplicationRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.PolicyId);
        });

        // ---- RolePrivileges ----
        modelBuilder.Entity<RolePrivilege>(b =>
        {
            b.ToTable("RolePrivileges");
            b.HasKey(x => x.Id);
            b.Property(x => x.RoleId).IsRequired();
            b.Property(x => x.PrivilegeId).IsRequired();
            b.Property(x => x.GrantedAt).IsRequired();
            b.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.RoleId);
            b.HasOne<ApplicationRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.PrivilegeId);
            b.HasOne<Privilege>().WithMany().HasForeignKey(x => x.PrivilegeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- SchedulerJobExecutions ----
        modelBuilder.Entity<SchedulerJobExecution>(b =>
        {
            b.ToTable("SchedulerJobExecutions");
            b.HasKey(x => x.Id);
            b.Property(x => x.JobId).IsRequired();
            b.Property(x => x.StartedAt).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();
            b.Property(x => x.RetryCount).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.JobId);
            b.HasIndex(x => x.RetryParentId);
        });

        // ---- SchedulerJobHistory ----
        modelBuilder.Entity<SchedulerJobHistory>(b =>
        {
            b.ToTable("SchedulerJobHistory");
            b.HasKey(x => x.Id);
            b.Property(x => x.JobId).IsRequired();
            b.Property(x => x.Action).HasMaxLength(32).IsRequired();
            b.Property(x => x.Changes).IsRequired();
            b.Property(x => x.PerformedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.PerformedAt).IsRequired();
            b.Property(x => x.IpAddress).HasMaxLength(128);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.JobId);
        });

        // ---- SchedulerJobs ----
        modelBuilder.Entity<SchedulerJob>(b =>
        {
            b.ToTable("SchedulerJobs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            b.Property(x => x.JobType).IsRequired();
            b.Property(x => x.ScheduleType).IsRequired();
            b.Property(x => x.ScheduleExpression).HasMaxLength(100);
            b.Property(x => x.JobConfiguration).IsRequired();
            b.Property(x => x.RetryPolicy).IsRequired();
            b.Property(x => x.Timezone).HasMaxLength(100).IsRequired();
            b.Property(x => x.IsEnabled).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.IsPaused).IsRequired();
            b.Property(x => x.CreatedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.LastModifiedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.Tags).IsRequired();
            b.Property(x => x.SchedulerJobId).HasMaxLength(200);
            b.Property(x => x.LastExecutionStatus).HasMaxLength(32);
            b.Property(x => x.ConsecutiveFailures).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.SchedulerJobId);
        });

        // ---- StockAdjustments ----
        modelBuilder.Entity<StockAdjustment>(b =>
        {
            b.ToTable("StockAdjustments");
            b.HasKey(x => x.Id);
            b.Property(x => x.AssetId).IsRequired();
            b.Property(x => x.LocationId).IsRequired();
            b.Property(x => x.AdjustmentType).HasMaxLength(32).IsRequired();
            b.Property(x => x.Quantity).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            b.Property(x => x.PerformedByUserId).IsRequired();
            b.Property(x => x.ReferenceNumber).HasMaxLength(100);
            b.Property(x => x.PerformedAt).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.AssetId);
            b.HasIndex(x => x.LocationId);
            b.HasIndex(x => x.PerformedByUserId);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AssetLocation>().WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- UserPrivilegePolicies ----
        modelBuilder.Entity<UserPrivilegePolicy>(b =>
        {
            b.ToTable("UserPrivilegePolicies");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.PolicyId).IsRequired();
            b.Property(x => x.GrantedAt).IsRequired();
            b.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.PolicyId);
        });

        // ---- UserPrivileges ----
        modelBuilder.Entity<UserPrivilege>(b =>
        {
            b.ToTable("UserPrivileges");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.PrivilegeId).IsRequired();
            b.Property(x => x.Effect).HasMaxLength(16).IsRequired();
            b.Property(x => x.GrantedAt).IsRequired();
            b.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(1000);
            b.Property(x => x.RevokedBy).HasMaxLength(256);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.UserId);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.PrivilegeId);
            b.HasOne<Privilege>().WithMany().HasForeignKey(x => x.PrivilegeId).OnDelete(DeleteBehavior.Restrict);
        });

    }
}
