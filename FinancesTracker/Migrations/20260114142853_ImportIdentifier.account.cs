using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class ImportIdentifieraccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "importidentifier",
                table: "accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "importidentifier",
                table: "accounts");
        }
    }
}
