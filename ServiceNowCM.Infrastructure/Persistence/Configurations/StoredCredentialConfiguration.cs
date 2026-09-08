using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence.Configurations;

public class StoredCredentialConfiguration
    : IEntityTypeConfiguration<StoredCredential>
{
    public void Configure(
        EntityTypeBuilder<StoredCredential> builder)
    {
        builder.ToTable("Credentials", "security");

        builder.HasKey(x => x.Reference);

        builder.Property(x => x.Reference)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EncryptedSecret)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ModifiedAtUtc)
            .IsRequired();
    }
}