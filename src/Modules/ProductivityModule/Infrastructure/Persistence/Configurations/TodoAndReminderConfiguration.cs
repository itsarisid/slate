using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;
/// <summary>
/// Todo configuration.
/// </summary>

public sealed class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.ToTable("ProductivityTodos");
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32);
    }
}
/// <summary>
/// Reminder configuration.
/// </summary>

public sealed class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("ProductivityReminders");
        builder.Ignore(x => x.NotificationMethods);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.LinkedEntityType).HasMaxLength(64);
        builder.Property(x => x.NotificationMethodsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RecurrencePatternJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ReminderType).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
    }
}
