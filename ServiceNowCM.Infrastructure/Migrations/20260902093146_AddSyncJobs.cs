using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncJobs",
                schema: "sync",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrationId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCheckpoint = table.Column<int>(type: "int", nullable: false),
                    TotalFetched = table.Column<int>(type: "int", nullable: false),
                    TotalProcessed = table.Column<int>(type: "int", nullable: false),
                    TotalSucceeded = table.Column<int>(type: "int", nullable: false),
                    TotalSkipped = table.Column<int>(type: "int", nullable: false),
                    TotalFailed = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ErrorSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncJobs_Integrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalSchema: "config",
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_IntegrationId",
                schema: "sync",
                table: "SyncJobs",
                column: "IntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_StartedAtUtc",
                schema: "sync",
                table: "SyncJobs",
                column: "StartedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SyncJobs_Status",
                schema: "sync",
                table: "SyncJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncJobs",
                schema: "sync");
        }
    }
}
