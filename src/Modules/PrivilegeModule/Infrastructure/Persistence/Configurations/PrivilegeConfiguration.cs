using System.Text.Json;
using Alphabet.Domain.Entities.Privilege;
using Alphabet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Alphabet.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures privilege-related persistence mappings.
/// </summary>
public sealed class PrivilegeConfiguration :
    IEntityTypeConfiguration<Privilege>,
    IEntityTypeConfiguration<PrivilegeCategory>,
    IEntityTypeConfiguration<RolePrivilege>,
    IEntityTypeConfiguration<UserPrivilege>,
    IEntityTypeConfiguration<PrivilegePolicy>,
    IEntityTypeConfiguration<PrivilegeRequest>,
    IEntityTypeConfiguration<PrivilegeAuditLog>,
    IEntityTypeConfiguration<RolePrivilegePolicy>,
    IEntityTypeConfiguration<UserPrivilegePolicy>,
    IEntityTypeConfiguration<PrivilegeDependency>,
    IEntityTypeConfiguration<PrivilegeCondition>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        var stringCollectionConverter = new ValueConverter<IReadOnlyCollection<string>, string>(
            value => JsonSerializer.Serialize(value, JsonOptions),
            value => (IReadOnlyCollection<string>)(JsonSerializer.Deserialize<HashSet<string>>(value, JsonOptions) ?? new HashSet<string>()));

        var dictionaryConverter = new ValueConverter<IDictionary<string, string?>, string>(
            value => JsonSerializer.Serialize(value, JsonOptions),
            value => JsonSerializer.Deserialize<Dictionary<string, string?>>(value, JsonOptions) ?? new Dictionary<string, string?>());

        builder.ToTable("Privileges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ResourceType).HasMaxLength(100);
        builder.Property(x => x.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(256);
        builder.Property(x => x.AllowedActions)
            .HasConversion(stringCollectionConverter)
            .HasColumnName("AllowedActionsJson");
        builder.Property(x => x.DependsOn)
            .HasConversion(stringCollectionConverter)
            .HasColumnName("DependsOnJson");
        builder.Property(x => x.Attributes)
            .HasConversion(dictionaryConverter)
            .HasColumnName("AttributesJson");
    }

    public void Configure(EntityTypeBuilder<PrivilegeCategory> builder)
    {
        builder.ToTable("PrivilegeCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Description).HasMaxLength(1000);
    }

    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.ToTable("RolePrivileges");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.RoleId, x.PrivilegeId }).IsUnique();
        builder.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
    }

    public void Configure(EntityTypeBuilder<UserPrivilege> builder)
    {
        builder.ToTable("UserPrivileges");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.PrivilegeId, x.RevokedAt });
        builder.Property(x => x.Effect).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RevokedBy).HasMaxLength(256);
        builder.Property(x => x.Reason).HasMaxLength(1000);
    }

    public void Configure(EntityTypeBuilder<PrivilegePolicy> builder)
    {
        var stringCollectionConverter = new ValueConverter<IReadOnlyCollection<string>, string>(
            value => JsonSerializer.Serialize(value, JsonOptions),
            value => (IReadOnlyCollection<string>)(JsonSerializer.Deserialize<HashSet<string>>(value, JsonOptions) ?? new HashSet<string>()));

        builder.ToTable("PrivilegePolicies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Condition).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.PrivilegeNames)
            .HasConversion(stringCollectionConverter)
            .HasColumnName("PrivilegeNamesJson");
    }

    public void Configure(EntityTypeBuilder<PrivilegeRequest> builder)
    {
        builder.ToTable("PrivilegeRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ApproverEmail).HasMaxLength(256);
        builder.Property(x => x.DecisionNotes).HasMaxLength(1000);
    }

    public void Configure(EntityTypeBuilder<PrivilegeAuditLog> builder)
    {
        var dictionaryConverter = new ValueConverter<IDictionary<string, string?>, string>(
            value => JsonSerializer.Serialize(value, JsonOptions),
            value => JsonSerializer.Deserialize<Dictionary<string, string?>>(value, JsonOptions) ?? new Dictionary<string, string?>());

        builder.ToTable("PrivilegeAuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Source).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PerformedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(128);
        builder.Property(x => x.Metadata)
            .HasConversion(dictionaryConverter)
            .HasColumnName("MetadataJson");
    }

    public void Configure(EntityTypeBuilder<RolePrivilegePolicy> builder)
    {
        builder.ToTable("RolePrivilegePolicies");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.RoleId, x.PolicyId }).IsUnique();
        builder.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
    }

    public void Configure(EntityTypeBuilder<UserPrivilegePolicy> builder)
    {
        builder.ToTable("UserPrivilegePolicies");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.PolicyId }).IsUnique();
        builder.Property(x => x.GrantedBy).HasMaxLength(256).IsRequired();
    }

    public void Configure(EntityTypeBuilder<PrivilegeDependency> builder)
    {
        builder.ToTable("PrivilegeDependencies");
        builder.HasKey(x => new { x.PrivilegeId, x.DependsOnPrivilegeId });
    }

    public void Configure(EntityTypeBuilder<PrivilegeCondition> builder)
    {
        builder.ToTable("PrivilegeConditions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttributeName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Operator).HasMaxLength(64).IsRequired();
        builder.Property(x => x.AttributeValue).HasMaxLength(512).IsRequired();
    }
}
