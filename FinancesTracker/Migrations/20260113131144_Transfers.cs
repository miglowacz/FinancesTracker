using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class Transfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "istransfer",
                table: "transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "relatedtransactionid",
                table: "transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_related_transaction_id",
                table: "transactions",
                column: "relatedtransactionid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_transactions_transactions_related_transaction_id",
                table: "transactions",
                column: "relatedtransactionid",
                principalTable: "transactions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transactions_transactions_related_transaction_id",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "ix_transactions_related_transaction_id",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "istransfer",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "relatedtransactionid",
                table: "transactions");
        }
    }
}
