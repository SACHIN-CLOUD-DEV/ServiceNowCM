using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Infrastructure.Persistence
{
    public class ServiceNowCMDbContext : DbContext
    {
        public ServiceNowCMDbContext(DbContextOptions<ServiceNowCMDbContext> options) : base(options)
        {
        }


        public DbSet<ServiceNowConnection> ServiceNowConnections => Set<ServiceNowConnection>();

        public DbSet<IntegrationConfiguration> IntegrationConfigurations => Set<IntegrationConfiguration>();

        public DbSet<IntegrationField> IntegrationFields => Set<IntegrationField>();

        public DbSet<ContentManagerConnection> ContentManagerConnections => Set<ContentManagerConnection>();

        public DbSet<StoredCredential> StoredCredentials => Set<StoredCredential>();

        public DbSet<SyncedRecord> SyncedRecords => Set<SyncedRecord>();

        public DbSet<SyncJob> SyncJobs => Set<SyncJob>();

        public DbSet<SyncFailure> SyncFailures => Set<SyncFailure>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ServiceNowCMDbContext).Assembly);

            modelBuilder.Entity<SyncedRecord>(entity =>
            {
                entity.ToTable("SyncedRecords", "sync");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.SourceSysId)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(x => x.SourceRecordNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.ContentManagerRecordNumber)
                    .HasMaxLength(100);

                entity.Property(x => x.FirstSyncedAtUtc)
                    .IsRequired();

                entity.Property(x => x.LastSyncedAtUtc)
                    .IsRequired();

                entity.HasIndex(x => new
                {
                    x.IntegrationId,
                    x.SourceSysId
                })
                .IsUnique();

                entity.HasOne<IntegrationConfiguration>()
                    .WithMany()
                    .HasForeignKey(x => x.IntegrationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
