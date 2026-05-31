using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;
/// <summary>
/// Calendar event configuration.
/// </summary>

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("ProductivityCalendarEvents");
        builder.Ignore(x => x.Attendees);
        builder.Ignore(x => x.ReminderMinutesBefore);
        builder.Ignore(x => x.Responses);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.Timezone).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Visibility).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Color).HasMaxLength(32);
        builder.Property(x => x.ConferenceLink).HasMaxLength(500);
        builder.Property(x => x.AttendeesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ReminderMinutesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RecurrencePatternJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ResponsesJson).HasColumnType("nvarchar(max)");
    }
}
/// <summary>
/// Attachment configuration.
/// </summary>

public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("ProductivityAttachments");
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(256).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
    }
}
/// <summary>
/// Comment configuration.
/// </summary>

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("ProductivityComments");
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000).IsRequired();
    }
}
/// <summary>
/// Tag configuration.
/// </summary>

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("ProductivityTags");
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(32);
    }
}
