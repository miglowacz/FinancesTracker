using FinancesTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Data;

public class FinancesTrackerDbContext : DbContext
{
    public FinancesTrackerDbContext(DbContextOptions<FinancesTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Subcategory> Subcategories { get; set; }
    public DbSet<CategoryRule> CategoryRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfiguracja relacji
        modelBuilder.Entity<Subcategory>()
            .HasOne(s => s.Category)
            .WithMany(c => c.Subcategories)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Subcategory)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.SubcategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CategoryRule>()
            .HasOne(cr => cr.Category)
            .WithMany(c => c.CategoryRules)
            .HasForeignKey(cr => cr.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CategoryRule>()
            .HasOne(cr => cr.Subcategory)
            .WithMany(s => s.CategoryRules)
            .HasForeignKey(cr => cr.SubcategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indeksy dla wydajnoœci
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.Year, t.MonthNumber })
            .HasDatabaseName("IX_Transaction_Year_Month");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.Date)
            .HasDatabaseName("IX_Transaction_Date");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.CategoryId)
            .HasDatabaseName("IX_Transaction_CategoryId");

        modelBuilder.Entity<CategoryRule>()
            .HasIndex(cr => cr.Keyword)
            .HasDatabaseName("IX_CategoryRule_Keyword");

        modelBuilder.Entity<CategoryRule>()
            .HasIndex(cr => cr.IsActive)
            .HasDatabaseName("IX_CategoryRule_IsActive");

        // Konfiguracja w³aœciwoœci
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        // Dane pocz¹tkowe - polskie kategorie
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Kategorie g³ówne
        var categories = new[]
        {
            new Category { Id = 1, Name = "Dochód" },
            new Category { Id = 2, Name = "Jedzenie" },
            new Category { Id = 3, Name = "Transport" },
            new Category { Id = 4, Name = "Rozrywka" },
            new Category { Id = 5, Name = "Zdrowie" },
            new Category { Id = 6, Name = "Dom i mieszkanie" },
            new Category { Id = 7, Name = "Ubrania" },
            new Category { Id = 8, Name = "Edukacja" },
            new Category { Id = 9, Name = "Oszczêdnoœci" },
            new Category { Id = 10, Name = "Inne wydatki" }
        };

        modelBuilder.Entity<Category>().HasData(categories);

        // Podkategorie
        var subcategories = new[]
        {
            // Dochód
            new Subcategory { Id = 1, Name = "Wynagrodzenie", CategoryId = 1 },
            new Subcategory { Id = 2, Name = "Premia", CategoryId = 1 },
            new Subcategory { Id = 3, Name = "Freelance", CategoryId = 1 },
            new Subcategory { Id = 4, Name = "Inne dochody", CategoryId = 1 },
            
            // Jedzenie
            new Subcategory { Id = 5, Name = "Zakupy spo¿ywcze", CategoryId = 2 },
            new Subcategory { Id = 6, Name = "Restauracje", CategoryId = 2 },
            new Subcategory { Id = 7, Name = "Fast food", CategoryId = 2 },
            new Subcategory { Id = 8, Name = "Kawa i napoje", CategoryId = 2 },
            
            // Transport
            new Subcategory { Id = 9, Name = "Paliwo", CategoryId = 3 },
            new Subcategory { Id = 10, Name = "Komunikacja publiczna", CategoryId = 3 },
            new Subcategory { Id = 11, Name = "Taxi/Uber", CategoryId = 3 },
            new Subcategory { Id = 12, Name = "Serwis samochodu", CategoryId = 3 },
            
            // Rozrywka
            new Subcategory { Id = 13, Name = "Kino", CategoryId = 4 },
            new Subcategory { Id = 14, Name = "Subskrypcje", CategoryId = 4 },
            new Subcategory { Id = 15, Name = "Gry", CategoryId = 4 },
            new Subcategory { Id = 16, Name = "Sport", CategoryId = 4 },
            
            // Zdrowie
            new Subcategory { Id = 17, Name = "Leki", CategoryId = 5 },
            new Subcategory { Id = 18, Name = "Lekarz", CategoryId = 5 },
            new Subcategory { Id = 19, Name = "Dentysta", CategoryId = 5 },
            new Subcategory { Id = 20, Name = "Gimnastyka", CategoryId = 5 },
            
            // Dom i mieszkanie
            new Subcategory { Id = 21, Name = "Czynsz", CategoryId = 6 },
            new Subcategory { Id = 22, Name = "Pr¹d", CategoryId = 6 },
            new Subcategory { Id = 23, Name = "Gaz", CategoryId = 6 },
            new Subcategory { Id = 24, Name = "Internet", CategoryId = 6 },
            new Subcategory { Id = 25, Name = "Meble", CategoryId = 6 },
            
            // Ubrania
            new Subcategory { Id = 26, Name = "Odzie¿", CategoryId = 7 },
            new Subcategory { Id = 27, Name = "Obuwie", CategoryId = 7 },
            
            // Edukacja
            new Subcategory { Id = 28, Name = "Kursy", CategoryId = 8 },
            new Subcategory { Id = 29, Name = "Ksi¹¿ki", CategoryId = 8 },
            
            // Oszczêdnoœci
            new Subcategory { Id = 30, Name = "Lokata", CategoryId = 9 },
            new Subcategory { Id = 31, Name = "Inwestycje", CategoryId = 9 },
            
            // Inne wydatki
            new Subcategory { Id = 32, Name = "Prezenty", CategoryId = 10 },
            new Subcategory { Id = 33, Name = "Ró¿ne", CategoryId = 10 }
        };

        modelBuilder.Entity<Subcategory>().HasData(subcategories);

        // Przyk³adowe regu³y kategoryzacji
        var categoryRules = new[]
        {
            new CategoryRule { Id = 1, Keyword = "wynagrodzenie", CategoryId = 1, SubcategoryId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 2, Keyword = "pensja", CategoryId = 1, SubcategoryId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 3, Keyword = "biedronka", CategoryId = 2, SubcategoryId = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 4, Keyword = "¿abka", CategoryId = 2, SubcategoryId = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 5, Keyword = "mcdonald", CategoryId = 2, SubcategoryId = 7, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 6, Keyword = "kfc", CategoryId = 2, SubcategoryId = 7, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 7, Keyword = "orlen", CategoryId = 3, SubcategoryId = 9, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 8, Keyword = "shell", CategoryId = 3, SubcategoryId = 9, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 9, Keyword = "netflix", CategoryId = 4, SubcategoryId = 14, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 10, Keyword = "spotify", CategoryId = 4, SubcategoryId = 14, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 11, Keyword = "apteka", CategoryId = 5, SubcategoryId = 17, IsActive = true, CreatedAt = DateTime.UtcNow },
            new CategoryRule { Id = 12, Keyword = "leki", CategoryId = 5, SubcategoryId = 17, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        modelBuilder.Entity<CategoryRule>().HasData(categoryRules);
    }
}