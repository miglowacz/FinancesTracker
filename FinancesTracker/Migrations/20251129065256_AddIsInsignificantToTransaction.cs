using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInsignificantToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInsignificant",
                table: "Transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInsignificant",
                table: "Transactions");
        }
    }
}
