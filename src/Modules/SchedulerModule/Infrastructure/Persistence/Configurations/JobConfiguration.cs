using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for scheduler jobs.
/// </summary>
public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("SchedulerJobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ScheduleExpression).HasMaxLength(100);
        builder.Property(x => x.JobConfiguration).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.RetryPolicy).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.Tags).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.ExclusionsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.DependenciesJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Timezone).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.LastModifiedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SchedulerJobId).HasMaxLength(200);
        builder.Property(x => x.LastExecutionStatus).HasConversion<string>().HasMaxLength(32);
    }
}
