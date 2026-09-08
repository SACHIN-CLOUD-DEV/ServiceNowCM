using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence.Configurations
{
    public class IntegrationFieldConfiguration: IEntityTypeConfiguration<IntegrationField>
    {
        public void Configure(
            EntityTypeBuilder<IntegrationField> builder)
        {
            builder.ToTable(
                "IntegrationFields",
                "config");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceFieldName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.SourceFieldLabel)
                .HasMaxLength(300);

            builder.Property(x => x.SourceFieldDataType)
                .HasMaxLength(100);

            builder.Property(x => x.DisplayOrder)
                .IsRequired();

            builder.HasIndex(x =>
                new
                {
                    x.IntegrationConfigurationId,
                    x.SourceFieldName
                })
                .IsUnique();

            builder.Property(x => x.TargetFieldName)
                .HasMaxLength(200);

            builder.Property(x => x.TargetFieldType)
                .HasMaxLength(50);

            builder.Property(x => x.TargetFieldUri);
        }
    }
}
