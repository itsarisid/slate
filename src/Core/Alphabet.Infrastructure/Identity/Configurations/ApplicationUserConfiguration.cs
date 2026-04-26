using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Identity.Configurations;

/// <summary>
/// Configures the application user entity.
/// </summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RecoveryCodes).HasColumnType("nvarchar(max)");
        builder.Property(x => x.OtpDestination).HasMaxLength(256);
        builder.Property(x => x.TwoFactorMethod).HasConversion<string>().HasMaxLength(32);
    }
}
