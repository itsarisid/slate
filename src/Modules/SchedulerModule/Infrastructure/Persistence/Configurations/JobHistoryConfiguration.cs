using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for job history entries.
/// </summary>
public sealed class JobHistoryConfiguration : IEntityTypeConfiguration<JobHistory>
{
    public void Configure(EntityTypeBuilder<JobHistory> builder)
    {
        builder.ToTable("SchedulerJobHistory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Changes).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.PerformedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(128);
        builder.HasIndex(x => x.JobId);
        builder.HasIndex(x => x.PerformedAt);
    }
}
