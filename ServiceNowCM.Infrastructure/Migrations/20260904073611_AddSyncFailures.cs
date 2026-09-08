using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncFailures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncFailures",
                schema: "sync",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SyncJobId = table.Column<long>(type: "bigint", nullable: false),
                    IntegrationId = table.Column<long>(type: "bigint", nullable: false),
                    SourceSysId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SourceRecordNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContentManagerRecordUri = table.Column<long>(type: "bigint", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncFailures_Integrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalSchema: "config",
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncFailures_SyncJobs_SyncJobId",
                        column: x => x.SyncJobId,
                        principalSchema: "sync",
                        principalTable: "SyncJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncFailures_IntegrationId",
                schema: "sync",
                table: "SyncFailures",
                column: "IntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncFailures_IntegrationId_IsResolved",
                schema: "sync",
                table: "SyncFailures",
                columns: new[] { "IntegrationId", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncFailures_IntegrationId_SourceSysId",
                schema: "sync",
                table: "SyncFailures",
                columns: new[] { "IntegrationId", "SourceSysId" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncFailures_OccurredAtUtc",
                schema: "sync",
                table: "SyncFailures",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SyncFailures_SyncJobId",
                schema: "sync",
                table: "SyncFailures",
                column: "SyncJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncFailures",
                schema: "sync");
        }
    }
}
