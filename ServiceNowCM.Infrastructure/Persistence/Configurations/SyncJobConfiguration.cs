using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence.Configurations
{
    public class SyncJobConfiguration : IEntityTypeConfiguration<SyncJob>
    {
        public void Configure(EntityTypeBuilder<SyncJob> builder)
        {
            builder.ToTable("SyncJobs", "sync");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.StartedAtUtc)
                .IsRequired();

            builder.Property(x => x.CompletedAtUtc);

            builder.Property(x => x.LastCompletedOffset)
                .IsRequired();

            builder.Property(x => x.LastCompletedPage)
                .IsRequired();

            builder.Property(x => x.TotalFetched)
                .IsRequired();

            builder.Property(x => x.TotalProcessed)
                .IsRequired();

            builder.Property(x => x.TotalSucceeded)
                .IsRequired();

            builder.Property(x => x.TotalSkipped)
                .IsRequired();

            builder.Property(x => x.TotalFailed)
                .IsRequired();

            builder.Property(x => x.RetryCount)
                .IsRequired();

            builder.Property(x => x.ErrorSummary)
                .HasMaxLength(4000);

            builder.HasOne(x => x.Integration)
                .WithMany()
                .HasForeignKey(x => x.IntegrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // General-purpose indexes
            builder.HasIndex(x => new
            {
                x.IntegrationId,
                x.StartedAtUtc
            }).HasDatabaseName("IX_SyncJobs_IntegrationId_StartedAtUtc");

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.StartedAtUtc);

            builder.HasIndex(x => x.IntegrationId)
                .HasDatabaseName(
                    "UX_SyncJobs_IntegrationId_Running")
                .IsUnique()
                .HasFilter("[Status] = 'Running'");
        }
    }
}