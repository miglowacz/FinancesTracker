using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddBankNameAndSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bank_name",
                table: "accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_name",
                table: "accounts");
        }
    }
}
