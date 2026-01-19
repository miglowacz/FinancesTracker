using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bankname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    importidentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    initialbalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PLN"),
                    cntaccounttype = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "subcategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    categoryid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subcategories", x => x.id);
                    table.ForeignKey(
                        name: "fk_subcategories_categories_category_id",
                        column: x => x.categoryid,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    keyword = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    categoryid = table.Column<int>(type: "integer", nullable: false),
                    subcategoryid = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_category_rules_categories_category_id",
                        column: x => x.categoryid,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_category_rules_subcategories_subcategory_id",
                        column: x => x.subcategoryid,
                        principalTable: "subcategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    accountid = table.Column<int>(type: "integer", nullable: true),
                    categoryid = table.Column<int>(type: "integer", nullable: true),
                    subcategoryid = table.Column<int>(type: "integer", nullable: true),
                    monthnumber = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    isinsignificant = table.Column<bool>(type: "boolean", nullable: false),
                    istransfer = table.Column<bool>(type: "boolean", nullable: false),
                    relatedtransactionid = table.Column<int>(type: "integer", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_transactions_accounts_account_id",
                        column: x => x.accountid,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_categories_category_id",
                        column: x => x.categoryid,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_subcategories_subcategory_id",
                        column: x => x.subcategoryid,
                        principalTable: "subcategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_transactions_related_transaction_id",
                        column: x => x.relatedtransactionid,
                        principalTable: "transactions",
                        principalColumn: "id");
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

            migrationBuilder.CreateIndex(
                name: "IX_Account_IsActive",
                table: "accounts",
                column: "isactive");

            migrationBuilder.CreateIndex(
                name: "IX_Account_Name",
                table: "accounts",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_category_rules_category_id",
                table: "category_rules",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "ix_category_rules_subcategory_id",
                table: "category_rules",
                column: "subcategoryid");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRule_IsActive",
                table: "category_rules",
                column: "isactive");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRule_Keyword",
                table: "category_rules",
                column: "keyword");

            migrationBuilder.CreateIndex(
                name: "ix_subcategories_category_id",
                table: "subcategories",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountId",
                table: "transactions",
                column: "accountid");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CategoryId",
                table: "transactions",
                column: "categoryid");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Date",
                table: "transactions",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Year_Month",
                table: "transactions",
                columns: new[] { "year", "monthnumber" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_related_transaction_id",
                table: "transactions",
                column: "relatedtransactionid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_subcategory_id",
                table: "transactions",
                column: "subcategoryid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_rules");

            migrationBuilder.DropTable(
                name: "category_rules");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "subcategories");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
