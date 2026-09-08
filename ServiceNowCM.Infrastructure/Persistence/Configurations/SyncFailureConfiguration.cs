using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence.Configurations;

public class SyncFailureConfiguration
    : IEntityTypeConfiguration<SyncFailure>
{
    public void Configure(
        EntityTypeBuilder<SyncFailure> builder)
    {
        builder.ToTable(
            "SyncFailures",
            "sync");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceSysId)
            .HasMaxLength(64);

        builder.Property(x => x.SourceRecordNumber)
            .HasMaxLength(100);

        builder.Property(x => x.ErrorCategory)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.AttemptNumber)
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.Property(x => x.ContentManagerRecordUri);

        builder.Property(x => x.IsResolved)
            .IsRequired();

        builder.Property(x => x.ResolvedAtUtc);

        // SyncFailure -> SyncJob
        builder.HasOne(x => x.SyncJob)
            .WithMany()
            .HasForeignKey(x => x.SyncJobId)
            .OnDelete(DeleteBehavior.Cascade);

        // SyncFailure -> Integration
        builder.HasOne(x => x.Integration)
            .WithMany()
            .HasForeignKey(x => x.IntegrationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Useful for displaying all failures for a job
        builder.HasIndex(x => x.SyncJobId);

        // Useful for integration-level failure history
        builder.HasIndex(x => x.IntegrationId);

        // Useful for Failed Transactions dashboard
        builder.HasIndex(x => new
        {
            x.IntegrationId,
            x.IsResolved
        });

        // Useful when investigating/retrying one SN record
        builder.HasIndex(x => new
        {
            x.IntegrationId,
            x.SourceSysId
        });

        builder.HasIndex(x => x.OccurredAtUtc);
    }
}