using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinancesTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subcategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subcategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Keyword = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    SubcategoryId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryRules_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryRules_Subcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Subcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    SubcategoryId = table.Column<int>(type: "integer", nullable: false),
                    MonthNumber = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BankName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Subcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Subcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
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
                table: "Subcategories",
                columns: new[] { "Id", "CategoryId", "Name" },
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
                table: "CategoryRules",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "IsActive", "Keyword", "SubcategoryId" },
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
                name: "IX_CategoryRule_IsActive",
                table: "CategoryRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRule_Keyword",
                table: "CategoryRules",
                column: "Keyword");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRules_CategoryId",
                table: "CategoryRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRules_SubcategoryId",
                table: "CategoryRules",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_CategoryId",
                table: "Subcategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Date",
                table: "Transactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Year_Month",
                table: "Transactions",
                columns: new[] { "Year", "MonthNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SubcategoryId",
                table: "Transactions",
                column: "SubcategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryRules");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Subcategories");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
