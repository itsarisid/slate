using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Persistence.Configurations;
/// <summary>
/// Note configuration.
/// </summary>

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("ProductivityNotes");
        builder.Ignore(x => x.Collaborators);
        builder.Ignore(x => x.Versions);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Color).HasMaxLength(32);
        builder.Property(x => x.Format).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CollaboratorsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.VersionHistoryJson).HasColumnType("nvarchar(max)");
    }
}
/// <summary>
/// Notebook configuration.
/// </summary>

public sealed class NotebookConfiguration : IEntityTypeConfiguration<Notebook>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<Notebook> builder)
    {
        builder.ToTable("ProductivityNotebooks");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Color).HasMaxLength(32);
    }
}
