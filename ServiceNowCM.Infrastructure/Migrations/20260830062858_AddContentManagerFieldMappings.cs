using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentManagerFieldMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SourceDataType",
                schema: "config",
                table: "IntegrationFields",
                newName: "SourceFieldDataType");

            migrationBuilder.AddColumn<string>(
                name: "TargetFieldName",
                schema: "config",
                table: "IntegrationFields",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetFieldType",
                schema: "config",
                table: "IntegrationFields",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TargetFieldUri",
                schema: "config",
                table: "IntegrationFields",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetFieldName",
                schema: "config",
                table: "IntegrationFields");

            migrationBuilder.DropColumn(
                name: "TargetFieldType",
                schema: "config",
                table: "IntegrationFields");

            migrationBuilder.DropColumn(
                name: "TargetFieldUri",
                schema: "config",
                table: "IntegrationFields");

            migrationBuilder.RenameColumn(
                name: "SourceFieldDataType",
                schema: "config",
                table: "IntegrationFields",
                newName: "SourceDataType");
        }
    }
}
