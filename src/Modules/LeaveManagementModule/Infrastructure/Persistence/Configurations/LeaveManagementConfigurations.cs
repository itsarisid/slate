using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;

public sealed class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.DefaultDaysPerYear).HasPrecision(10, 2);
        builder.Property(x => x.MinDaysPerRequest).HasPrecision(10, 2);
        builder.Property(x => x.MaxDaysPerRequest).HasPrecision(10, 2);
        builder.Property(x => x.MaxCarryForwardDays).HasPrecision(10, 2);
        builder.Property(x => x.EncashmentRate).HasPrecision(10, 2);
        builder.Property(x => x.EligibilityRulesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.BlackoutDatesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.AllowedAttachmentTypesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.AutoApproveRulesJson).HasColumnType("nvarchar(max)");
    }
}

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.LeaveTypeId, x.Year }).IsUnique();
        builder.Property(x => x.Allocated).HasPrecision(10, 2);
        builder.Property(x => x.Taken).HasPrecision(10, 2);
        builder.Property(x => x.Pending).HasPrecision(10, 2);
        builder.Property(x => x.Approved).HasPrecision(10, 2);
        builder.Property(x => x.Remaining).HasPrecision(10, 2);
        builder.Property(x => x.CarryForward).HasPrecision(10, 2);
        builder.Ignore(x => x.TotalAvailable);
    }
}

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.StartDate, x.EndDate });
        builder.Property(x => x.TotalDays).HasPrecision(10, 2);
        builder.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.PartialDaysJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.AttachmentIdsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ContactNumber).HasMaxLength(50);
        builder.Property(x => x.AlternateArrangements).HasMaxLength(2000);
        builder.Property(x => x.CancelledReason).HasMaxLength(2000);
    }
}

public sealed class ApprovalChainConfiguration : IEntityTypeConfiguration<ApprovalChain>
{
    public void Configure(EntityTypeBuilder<ApprovalChain> builder)
    {
        builder.ToTable("LeaveApprovalChains");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ApplicableToJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ApprovalLevelsJson).HasColumnType("nvarchar(max)");
    }
}

public sealed class ApprovalWorkflowConfiguration : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        builder.ToTable("LeaveApprovalWorkflows");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.LeaveRequestId);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
    }
}

public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("LeaveWorkflowSteps");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WorkflowId, x.Level, x.Status });
        builder.Property(x => x.ApproverType).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.ApproverValue).HasMaxLength(250);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Action).HasMaxLength(60);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.AttachmentsJson).HasColumnType("nvarchar(max)");
    }
}

public sealed class DelegationConfiguration : IEntityTypeConfiguration<Delegation>
{
    public void Configure(EntityTypeBuilder<Delegation> builder)
    {
        builder.ToTable("LeaveDelegations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.DelegatorUserId, x.DelegateToUserId, x.IsActive });
        builder.Property(x => x.DelegationType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Permission).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.ApplicableLeaveTypesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ApplicableApprovalLevelsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ApplicableDepartmentsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ApplicableEmployeesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Reason).HasMaxLength(2000);
    }
}

public sealed class PublicHolidayConfiguration : IEntityTypeConfiguration<PublicHoliday>
{
    public void Configure(EntityTypeBuilder<PublicHoliday> builder)
    {
        builder.ToTable("LeavePublicHolidays");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(10).IsRequired();
        builder.Property(x => x.State).HasMaxLength(50);
    }
}

public sealed class BlackoutPeriodConfiguration : IEntityTypeConfiguration<BlackoutPeriod>
{
    public void Configure(EntityTypeBuilder<BlackoutPeriod> builder)
    {
        builder.ToTable("LeaveBlackoutPeriods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.ApplicableToJson).HasColumnType("nvarchar(max)");
    }
}

public sealed class AccrualRuleConfiguration : IEntityTypeConfiguration<AccrualRule>
{
    public void Configure(EntityTypeBuilder<AccrualRule> builder)
    {
        builder.ToTable("LeaveAccrualRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AccrualMethod).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.AccrualRate).HasPrecision(10, 2);
        builder.Property(x => x.MaxAccrual).HasPrecision(10, 2);
        builder.Property(x => x.TenureRulesJson).HasColumnType("nvarchar(max)");
    }
}

public sealed class LeaveActivityLogConfiguration : IEntityTypeConfiguration<LeaveActivityLog>
{
    public void Configure(EntityTypeBuilder<LeaveActivityLog> builder)
    {
        builder.ToTable("LeaveActivityLogs");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.Timestamp });
        builder.HasIndex(x => new { x.LeaveRequestId, x.Timestamp });
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OldValueJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.NewValueJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.IpAddress).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.DetailsJson).HasColumnType("nvarchar(max)");
    }
}
