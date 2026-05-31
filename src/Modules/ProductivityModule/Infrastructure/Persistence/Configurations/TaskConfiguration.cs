using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;
/// <summary>
/// Productivity task configuration.
/// </summary>

public sealed class ProductivityTaskConfiguration : IEntityTypeConfiguration<ProductivityTask>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<ProductivityTask> builder)
    {
        builder.ToTable("ProductivityTasks");
        builder.Ignore(x => x.Checklist);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ChecklistJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.CommentsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.EstimatedHours).HasPrecision(18, 2);
        builder.Property(x => x.ActualHours).HasPrecision(18, 2);
    }
}
/// <summary>
/// Time entry configuration.
/// </summary>

public sealed class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("ProductivityTimeEntries");
        builder.Property(x => x.Description).HasMaxLength(1000);
    }
}
/// <summary>
/// Task dependency configuration.
/// </summary>

public sealed class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<TaskDependency> builder)
    {
        builder.ToTable("ProductivityTaskDependencies");
        builder.HasIndex(x => new { x.ProductivityTaskId, x.DependsOnTaskId }).IsUnique();
    }
}
