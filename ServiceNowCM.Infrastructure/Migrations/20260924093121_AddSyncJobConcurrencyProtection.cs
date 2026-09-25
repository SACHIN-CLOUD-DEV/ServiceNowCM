using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncJobConcurrencyProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncJobs_IntegrationId",
                schema: "sync",
                table: "SyncJobs");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_IntegrationId_StartedAtUtc",
                schema: "sync",
                table: "SyncJobs",
                columns: new[] { "IntegrationId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_SyncJobs_IntegrationId_Running",
                schema: "sync",
                table: "SyncJobs",
                column: "IntegrationId",
                unique: true,
                filter: "[Status] = 'Running'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncJobs_IntegrationId_StartedAtUtc",
                schema: "sync",
                table: "SyncJobs");

            migrationBuilder.DropIndex(
                name: "UX_SyncJobs_IntegrationId_Running",
                schema: "sync",
                table: "SyncJobs");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_IntegrationId",
                schema: "sync",
                table: "SyncJobs",
                column: "IntegrationId");
        }
    }
}
