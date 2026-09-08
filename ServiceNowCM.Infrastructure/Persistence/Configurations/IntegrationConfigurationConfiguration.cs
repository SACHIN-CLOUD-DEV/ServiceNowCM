using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence.Configurations
{
    public class IntegrationConfigurationConfiguration: IEntityTypeConfiguration<IntegrationConfiguration>
    {
        public void Configure(EntityTypeBuilder<IntegrationConfiguration> builder)
        {
            builder.ToTable(
                "Integrations",
                "config");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.TableName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.EncodedQuery)
                .HasMaxLength(4000);

            builder.Property(x => x.PageSize)
                .IsRequired();

            builder.Property(x => x.ProcessingBatchSize)
                .IsRequired();

            builder.Property(x => x.DisplayValues)
                .IsRequired();

            builder.Property(x => x.ExcludeReferenceLinks)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.ModifiedAtUtc);

            builder.Property(x => x.ContentManagerConnectionId);

            builder.Property(x => x.ContentManagerRecordTypeUri);

            builder.Property(x => x.ContentManagerRecordTypeName)
                .HasMaxLength(200);

            builder.HasOne<ServiceNowConnection>()
                .WithMany()
                .HasForeignKey(x => x.ServiceNowConnectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Fields)
                .WithOne(x => x.IntegrationConfiguration)
                .HasForeignKey(x => x.IntegrationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ContentManagerConnection>()
                .WithMany()
                .HasForeignKey(x => x.ContentManagerConnectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ServiceNowConnectionId);

            builder.HasIndex(x => x.Name)
                .IsUnique();

            
        }
    }
}