using System.Text;

using FinancesTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Data;

public class FinancesTrackerDbContext : DbContext {
  public FinancesTrackerDbContext(DbContextOptions<FinancesTrackerDbContext> options) : base(options) {
  }

  public DbSet<cAccount> Accounts { get; set; }
  public DbSet<cTransaction> Transactions { get; set; }
  public DbSet<cCategory> Categories { get; set; }
  public DbSet<cSubcategory> Subcategories { get; set; }
  public DbSet<cCategoryRule> CategoryRules { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    base.OnModelCreating(modelBuilder);

    // Konfiguracja nazewnictwa snake_case dla tabel
    foreach (var entity in modelBuilder.Model.GetEntityTypes()) {
      entity.SetTableName(ToSnakeCase(entity.GetTableName()));
      foreach (var property in entity.GetProperties()) {
        property.SetColumnName(ToSnakeCase(property.Name));
      }
    }

    // Konfiguracja cAccount
    modelBuilder.Entity<cAccount>()
      .Property(a => a.InitialBalance)
      .HasPrecision(18, 2);

    modelBuilder.Entity<cAccount>()
      .Property(a => a.Name)
      .HasMaxLength(100)
      .IsRequired();

    modelBuilder.Entity<cAccount>()
      .Property(a => a.BankName)
      .HasMaxLength(100);

    modelBuilder.Entity<cAccount>()
      .Property(a => a.Currency)
      .HasMaxLength(3)
      .HasDefaultValue("PLN");

    modelBuilder.Entity<cAccount>()
      .HasIndex(a => a.Name)
      .HasDatabaseName("IX_Account_Name");

    modelBuilder.Entity<cAccount>()
      .HasIndex(a => a.IsActive)
      .HasDatabaseName("IX_Account_IsActive");

    // Konfiguracja relacji cTransaction -> cAccount
    modelBuilder.Entity<cTransaction>()
      .HasOne(t => t.Account)
      .WithMany(a => a.Transactions)
      .HasForeignKey(t => t.AccountId)
      .OnDelete(DeleteBehavior.Restrict);

    // Konfiguracja relacji cTransaction -> cCategory
    modelBuilder.Entity<cTransaction>()
      .HasOne(t => t.Category)
      .WithMany(c => c.Transactions)
      .HasForeignKey(t => t.CategoryId)
      .OnDelete(DeleteBehavior.Restrict);

    // Konfiguracja relacji cTransaction -> cSubcategory
    modelBuilder.Entity<cTransaction>()
      .HasOne(t => t.Subcategory)
      .WithMany(s => s.Transactions)
      .HasForeignKey(t => t.SubcategoryId)
      .OnDelete(DeleteBehavior.Restrict);

    // Konfiguracja cSubcategory -> cCategory
    modelBuilder.Entity<cSubcategory>()
      .HasOne(s => s.Category)
      .WithMany(c => c.Subcategories)
      .HasForeignKey(s => s.CategoryId)
      .OnDelete(DeleteBehavior.Cascade);

    // Konfiguracja cCategoryRule -> cCategory
    modelBuilder.Entity<cCategoryRule>()
      .HasOne(cr => cr.Category)
      .WithMany(c => c.CategoryRules)
      .HasForeignKey(cr => cr.CategoryId)
      .OnDelete(DeleteBehavior.Cascade);

    // Konfiguracja cCategoryRule -> cSubcategory
    modelBuilder.Entity<cCategoryRule>()
      .HasOne(cr => cr.Subcategory)
      .WithMany(s => s.CategoryRules)
      .HasForeignKey(cr => cr.SubcategoryId)
      .OnDelete(DeleteBehavior.Cascade);

    // Indeksy dla wydajności
    modelBuilder.Entity<cTransaction>()
      .HasIndex(t => new { t.Year, t.MonthNumber })
      .HasDatabaseName("IX_Transaction_Year_Month");

    modelBuilder.Entity<cTransaction>()
      .HasIndex(t => t.Date)
      .HasDatabaseName("IX_Transaction_Date");

    modelBuilder.Entity<cTransaction>()
      .HasIndex(t => t.CategoryId)
      .HasDatabaseName("IX_Transaction_CategoryId");

    modelBuilder.Entity<cTransaction>()
      .HasIndex(t => t.AccountId)
      .HasDatabaseName("IX_Transaction_AccountId");

    modelBuilder.Entity<cCategoryRule>()
      .HasIndex(cr => cr.Keyword)
      .HasDatabaseName("IX_CategoryRule_Keyword");

    modelBuilder.Entity<cCategoryRule>()
      .HasIndex(cr => cr.IsActive)
      .HasDatabaseName("IX_CategoryRule_IsActive");

    // Konfiguracja właściwości cTransaction
    modelBuilder.Entity<cTransaction>()
      .Property(t => t.Amount)
      .HasPrecision(18, 2);

    // Dane początkowe
    SeedData(modelBuilder);
  }

  private static string ToSnakeCase(string? name) {
    if (string.IsNullOrEmpty(name)) return name ?? string.Empty;
    
    var result = new StringBuilder();
    result.Append(char.ToLower(name[0]));
    
    for (int i = 1; i < name.Length; i++) {
      if (char.IsUpper(name[i])) {
        result.Append('_');
        result.Append(char.ToLower(name[i]));
      } else {
        result.Append(name[i]);
      }
    }
    
    return result.ToString();
  }

  private void SeedData(ModelBuilder modelBuilder) {
    // Kategorie główne
    var categories = new[] {
      new cCategory { Id = 1, Name = "Dochód" },
      new cCategory { Id = 2, Name = "Jedzenie" },
      new cCategory { Id = 3, Name = "Transport" },
      new cCategory { Id = 4, Name = "Rozrywka" },
      new cCategory { Id = 5, Name = "Zdrowie" },
      new cCategory { Id = 6, Name = "Dom i mieszkanie" },
      new cCategory { Id = 7, Name = "Ubrania" },
      new cCategory { Id = 8, Name = "Edukacja" },
      new cCategory { Id = 9, Name = "Oszczędności" },
      new cCategory { Id = 10, Name = "Inne wydatki" }
    };

    modelBuilder.Entity<cCategory>().HasData(categories);

    // Podkategorie
    var subcategories = new[] {
      new cSubcategory { Id = 1, Name = "Wynagrodzenie", CategoryId = 1 },
      new cSubcategory { Id = 2, Name = "Premia", CategoryId = 1 },
      new cSubcategory { Id = 3, Name = "Freelance", CategoryId = 1 },
      new cSubcategory { Id = 4, Name = "Inne dochody", CategoryId = 1 },
      new cSubcategory { Id = 5, Name = "Zakupy spożywcze", CategoryId = 2 },
      new cSubcategory { Id = 6, Name = "Restauracje", CategoryId = 2 },
      new cSubcategory { Id = 7, Name = "Fast food", CategoryId = 2 },
      new cSubcategory { Id = 8, Name = "Kawa i napoje", CategoryId = 2 },
      new cSubcategory { Id = 9, Name = "Paliwo", CategoryId = 3 },
      new cSubcategory { Id = 10, Name = "Komunikacja publiczna", CategoryId = 3 },
      new cSubcategory { Id = 11, Name = "Taxi/Uber", CategoryId = 3 },
      new cSubcategory { Id = 12, Name = "Serwis samochodu", CategoryId = 3 },
      new cSubcategory { Id = 13, Name = "Kino", CategoryId = 4 },
      new cSubcategory { Id = 14, Name = "Subskrypcje", CategoryId = 4 },
      new cSubcategory { Id = 15, Name = "Gry", CategoryId = 4 },
      new cSubcategory { Id = 16, Name = "Sport", CategoryId = 4 },
      new cSubcategory { Id = 17, Name = "Leki", CategoryId = 5 },
      new cSubcategory { Id = 18, Name = "Lekarz", CategoryId = 5 },
      new cSubcategory { Id = 19, Name = "Dentysta", CategoryId = 5 },
      new cSubcategory { Id = 20, Name = "Gimnastyka", CategoryId = 5 },
      new cSubcategory { Id = 21, Name = "Czynsz", CategoryId = 6 },
      new cSubcategory { Id = 22, Name = "Prąd", CategoryId = 6 },
      new cSubcategory { Id = 23, Name = "Gaz", CategoryId = 6 },
      new cSubcategory { Id = 24, Name = "Internet", CategoryId = 6 },
      new cSubcategory { Id = 25, Name = "Meble", CategoryId = 6 },
      new cSubcategory { Id = 26, Name = "Odzież", CategoryId = 7 },
      new cSubcategory { Id = 27, Name = "Obuwie", CategoryId = 7 },
      new cSubcategory { Id = 28, Name = "Kursy", CategoryId = 8 },
      new cSubcategory { Id = 29, Name = "Książki", CategoryId = 8 },
      new cSubcategory { Id = 30, Name = "Lokata", CategoryId = 9 },
      new cSubcategory { Id = 31, Name = "Inwestycje", CategoryId = 9 },
      new cSubcategory { Id = 32, Name = "Prezenty", CategoryId = 10 },
      new cSubcategory { Id = 33, Name = "Różne", CategoryId = 10 }
    };

    modelBuilder.Entity<cSubcategory>().HasData(subcategories);

    // Przykładowe reguły kategoryzacji
    var staticDate = new DateTime(2025, 10, 18, 5, 22, 37, 396, DateTimeKind.Utc);
    var categoryRules = new[] {
      new cCategoryRule { Id = 1, Keyword = "wynagrodzenie", CategoryId = 1, SubcategoryId = 1, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 2, Keyword = "pensja", CategoryId = 1, SubcategoryId = 1, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 3, Keyword = "biedronka", CategoryId = 2, SubcategoryId = 5, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 4, Keyword = "żabka", CategoryId = 2, SubcategoryId = 5, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 5, Keyword = "mcdonald", CategoryId = 2, SubcategoryId = 7, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 6, Keyword = "kfc", CategoryId = 2, SubcategoryId = 7, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 7, Keyword = "orlen", CategoryId = 3, SubcategoryId = 9, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 8, Keyword = "shell", CategoryId = 3, SubcategoryId = 9, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 9, Keyword = "netflix", CategoryId = 4, SubcategoryId = 14, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 10, Keyword = "spotify", CategoryId = 4, SubcategoryId = 14, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 11, Keyword = "apteka", CategoryId = 5, SubcategoryId = 17, IsActive = true, CreatedAt = staticDate },
      new cCategoryRule { Id = 12, Keyword = "leki", CategoryId = 5, SubcategoryId = 17, IsActive = true, CreatedAt = staticDate }
    };

    modelBuilder.Entity<cCategoryRule>().HasData(categoryRules);
  }
}
