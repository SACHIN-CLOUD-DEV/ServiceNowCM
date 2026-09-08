using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence.Configurations
{
    public class ContentManagerConnectionConfiguration: IEntityTypeConfiguration<ContentManagerConnection>
    {
        public void Configure(EntityTypeBuilder<ContentManagerConnection> builder)
        {
            builder.ToTable(
                "ContentManagerConnections",
                "config");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.WorkgroupServerName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.WorkgroupServerPort)
                .IsRequired();

            builder.Property(x => x.DatasetId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.AuthenticationType)
                .IsRequired();

            builder.Property(x => x.Username)
                .HasMaxLength(200);

            builder.Property(x => x.CredentialReference)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ModifiedAtUtc);

            builder.HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}