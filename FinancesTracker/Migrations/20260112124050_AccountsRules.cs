using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class AccountsRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    keyword = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    accountid = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_rules_accounts_account_id",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_rules_account_id",
                table: "account_rules",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRule_IsActive",
                table: "account_rules",
                column: "isactive");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRule_Keyword",
                table: "account_rules",
                column: "keyword");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_rules");
        }
    }
}
