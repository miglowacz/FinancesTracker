using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    initial_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PLN"),
                    cnt_account_type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subcategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subcategories", x => x.id);
                    table.ForeignKey(
                        name: "fk_subcategories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    keyword = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    subcategory_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_category_rules_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_category_rules_subcategories_subcategory_id",
                        column: x => x.subcategory_id,
                        principalTable: "subcategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    subcategory_id = table.Column<int>(type: "integer", nullable: true),
                    month_number = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    is_insignificant = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_transactions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_subcategories_subcategory_id",
                        column: x => x.subcategory_id,
                        principalTable: "subcategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Dochód" },
                    { 2, "Jedzenie" },
                    { 3, "Transport" },
                    { 4, "Rozrywka" },
                    { 5, "Zdrowie" },
                    { 6, "Dom i mieszkanie" },
                    { 7, "Ubrania" },
                    { 8, "Edukacja" },
                    { 9, "Oszczędności" },
                    { 10, "Inne wydatki" }
                });

            migrationBuilder.InsertData(
                table: "subcategories",
                columns: new[] { "id", "category_id", "name" },
                values: new object[,]
                {
                    { 1, 1, "Wynagrodzenie" },
                    { 2, 1, "Premia" },
                    { 3, 1, "Freelance" },
                    { 4, 1, "Inne dochody" },
                    { 5, 2, "Zakupy spożywcze" },
                    { 6, 2, "Restauracje" },
                    { 7, 2, "Fast food" },
                    { 8, 2, "Kawa i napoje" },
                    { 9, 3, "Paliwo" },
                    { 10, 3, "Komunikacja publiczna" },
                    { 11, 3, "Taxi/Uber" },
                    { 12, 3, "Serwis samochodu" },
                    { 13, 4, "Kino" },
                    { 14, 4, "Subskrypcje" },
                    { 15, 4, "Gry" },
                    { 16, 4, "Sport" },
                    { 17, 5, "Leki" },
                    { 18, 5, "Lekarz" },
                    { 19, 5, "Dentysta" },
                    { 20, 5, "Gimnastyka" },
                    { 21, 6, "Czynsz" },
                    { 22, 6, "Prąd" },
                    { 23, 6, "Gaz" },
                    { 24, 6, "Internet" },
                    { 25, 6, "Meble" },
                    { 26, 7, "Odzież" },
                    { 27, 7, "Obuwie" },
                    { 28, 8, "Kursy" },
                    { 29, 8, "Książki" },
                    { 30, 9, "Lokata" },
                    { 31, 9, "Inwestycje" },
                    { 32, 10, "Prezenty" },
                    { 33, 10, "Różne" }
                });

            migrationBuilder.InsertData(
                table: "category_rules",
                columns: new[] { "id", "category_id", "created_at", "is_active", "keyword", "subcategory_id" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "wynagrodzenie", 1 },
                    { 2, 1, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "pensja", 1 },
                    { 3, 2, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "biedronka", 5 },
                    { 4, 2, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "żabka", 5 },
                    { 5, 2, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "mcdonald", 7 },
                    { 6, 2, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "kfc", 7 },
                    { 7, 3, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "orlen", 9 },
                    { 8, 3, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "shell", 9 },
                    { 9, 4, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "netflix", 14 },
                    { 10, 4, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "spotify", 14 },
                    { 11, 5, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "apteka", 17 },
                    { 12, 5, new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc), true, "leki", 17 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_IsActive",
                table: "accounts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_Account_Name",
                table: "accounts",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_category_rules_category_id",
                table: "category_rules",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_rules_subcategory_id",
                table: "category_rules",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRule_IsActive",
                table: "category_rules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRule_Keyword",
                table: "category_rules",
                column: "keyword");

            migrationBuilder.CreateIndex(
                name: "ix_subcategories_category_id",
                table: "subcategories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AccountId",
                table: "transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CategoryId",
                table: "transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Date",
                table: "transactions",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Year_Month",
                table: "transactions",
                columns: new[] { "year", "month_number" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_subcategory_id",
                table: "transactions",
                column: "subcategory_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
