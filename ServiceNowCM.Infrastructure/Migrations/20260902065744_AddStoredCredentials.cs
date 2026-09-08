using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceNowCM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "Credentials",
                schema: "security",
                columns: table => new
                {
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EncryptedSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Reference);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credentials",
                schema: "security");
        }
    }
}
