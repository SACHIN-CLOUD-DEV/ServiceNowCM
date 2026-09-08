using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence.Configurations
{
    public class ServiceNowConnectionConfiguration: IEntityTypeConfiguration<ServiceNowConnection>
    {

        public void Configure(EntityTypeBuilder<ServiceNowConnection> builder)
        {
            builder.ToTable("ServiceNowConnections", "config");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.InstanceUrl).HasMaxLength(500).IsRequired();
            builder.Property(x => x.AuthenticationType).HasConversion<int>();
            builder.Property(x => x.ClientId).HasMaxLength(500);
            builder.Property(x => x.Username).HasMaxLength(200);
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.ModifiedAtUtc);
            builder.Ignore(x => x.ClientSecret);
            builder.Ignore(x => x.Password);
            builder.Property(x => x.CredentialReference).HasMaxLength(500);

        }
    }
}
