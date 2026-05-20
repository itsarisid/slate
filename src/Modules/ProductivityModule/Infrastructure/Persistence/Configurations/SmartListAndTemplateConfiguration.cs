using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;
/// <summary>
/// Smart list configuration.
/// </summary>

public sealed class SmartListConfiguration : IEntityTypeConfiguration<SmartList>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<SmartList> builder)
    {
        builder.ToTable("ProductivitySmartLists");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CriteriaJson).HasColumnType("nvarchar(max)");
    }
}
/// <summary>
/// Productivity template configuration.
/// </summary>

public sealed class ProductivityTemplateConfiguration : IEntityTypeConfiguration<ProductivityTemplate>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<ProductivityTemplate> builder)
    {
        builder.ToTable("ProductivityTemplates");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.TemplateJson).HasColumnType("nvarchar(max)");
    }
}
