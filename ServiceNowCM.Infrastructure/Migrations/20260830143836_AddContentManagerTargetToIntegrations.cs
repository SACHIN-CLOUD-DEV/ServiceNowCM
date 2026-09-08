using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentManagerTargetToIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ContentManagerConnectionId",
                schema: "config",
                table: "Integrations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentManagerRecordTypeName",
                schema: "config",
                table: "Integrations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContentManagerRecordTypeUri",
                schema: "config",
                table: "Integrations",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_ContentManagerConnectionId",
                schema: "config",
                table: "Integrations",
                column: "ContentManagerConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Integrations_ContentManagerConnections_ContentManagerConnectionId",
                schema: "config",
                table: "Integrations",
                column: "ContentManagerConnectionId",
                principalSchema: "config",
                principalTable: "ContentManagerConnections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Integrations_ContentManagerConnections_ContentManagerConnectionId",
                schema: "config",
                table: "Integrations");

            migrationBuilder.DropIndex(
                name: "IX_Integrations_ContentManagerConnectionId",
                schema: "config",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ContentManagerConnectionId",
                schema: "config",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ContentManagerRecordTypeName",
                schema: "config",
                table: "Integrations");

            migrationBuilder.DropColumn(
                name: "ContentManagerRecordTypeUri",
                schema: "config",
                table: "Integrations");
        }
    }
}
