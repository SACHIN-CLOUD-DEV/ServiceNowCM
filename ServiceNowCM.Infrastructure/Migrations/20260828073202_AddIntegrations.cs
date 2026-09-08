using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Integrations",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ServiceNowConnectionId = table.Column<long>(type: "bigint", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EncodedQuery = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PageSize = table.Column<int>(type: "int", nullable: false),
                    ProcessingBatchSize = table.Column<int>(type: "int", nullable: false),
                    DisplayValues = table.Column<bool>(type: "bit", nullable: false),
                    ExcludeReferenceLinks = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Integrations_ServiceNowConnections_ServiceNowConnectionId",
                        column: x => x.ServiceNowConnectionId,
                        principalSchema: "config",
                        principalTable: "ServiceNowConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationFields",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntegrationConfigurationId = table.Column<long>(type: "bigint", nullable: false),
                    SourceFieldName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceFieldLabel = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SourceDataType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationFields_Integrations_IntegrationConfigurationId",
                        column: x => x.IntegrationConfigurationId,
                        principalSchema: "config",
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationFields_IntegrationConfigurationId_SourceFieldName",
                schema: "config",
                table: "IntegrationFields",
                columns: new[] { "IntegrationConfigurationId", "SourceFieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_Name",
                schema: "config",
                table: "Integrations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_ServiceNowConnectionId",
                schema: "config",
                table: "Integrations",
                column: "ServiceNowConnectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationFields",
                schema: "config");

            migrationBuilder.DropTable(
                name: "Integrations",
                schema: "config");
        }
    }
}
