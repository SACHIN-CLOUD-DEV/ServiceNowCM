using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImproveSyncJobCheckpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastCheckpoint",
                schema: "sync",
                table: "SyncJobs",
                newName: "LastCompletedOffset");

            migrationBuilder.AddColumn<int>(
                name: "LastCompletedPage",
                schema: "sync",
                table: "SyncJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCompletedPage",
                schema: "sync",
                table: "SyncJobs");

            migrationBuilder.RenameColumn(
                name: "LastCompletedOffset",
                schema: "sync",
                table: "SyncJobs",
                newName: "LastCheckpoint");
        }
    }
}