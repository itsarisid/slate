using Alphabet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alphabet.Infrastructure.Identity.Configurations;

/// <summary>
/// Configures the application user entity.
/// </summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.Department).HasMaxLength(256);
        builder.Property(x => x.Location).HasMaxLength(256);
        builder.Property(x => x.AvatarUrl).HasMaxLength(1024);
        builder.Property(x => x.Preferences).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RecoveryCodes).HasColumnType("nvarchar(max)");
        builder.Property(x => x.OtpDestination).HasMaxLength(256);
        builder.Property(x => x.TwoFactorMethod).HasConversion<string>().HasMaxLength(32);
    }
}
