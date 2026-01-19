using Bogus;

using Microsoft.EntityFrameworkCore;
using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

public class cDataSeedService {
  private readonly FinancesTrackerDbContext _context;

  public cDataSeedService(FinancesTrackerDbContext context) {
    _context = context;
  }

  public async Task GenerateEverythingAsync(int transactionCount = 200) {
    // 1. CZYSZCZENIE (opcjonalne - odkomentuj jeśli chcesz startować od zera)
    _context.Transactions.RemoveRange(_context.Transactions);
    _context.Subcategories.RemoveRange(_context.Subcategories);
    _context.Categories.RemoveRange(_context.Categories);
    _context.Accounts.RemoveRange(_context.Accounts);
    await _context.SaveChangesAsync();

    if (await _context.Accounts.AnyAsync()) return; // Zabezpieczenie przed duplikowaniem

    var faker = new Faker("pl");

    // 2. GENEROWANIE KONT
    var cAccounts = new List<cAccount>
    {
      new cAccount { Name = "Konto Osobiste", InitialBalance = 5000, Currency = "PLN" },
      new cAccount { Name = "Oszczędnościowe", InitialBalance = 20000, Currency = "PLN" },
      new cAccount { Name = "Portfel (Gotówka)", InitialBalance = 300, Currency = "PLN" }
    };
    await _context.Accounts.AddRangeAsync(cAccounts);
    await _context.SaveChangesAsync(); // Zapisz, aby uzyskać ID

    // 3. GENEROWANIE KATEGORII I PODKATEGORII
    // Mapa: Nazwa kategorii głównej -> Lista podkategorii z keywords
    var categoryStructure = new Dictionary<string, List<(string SubcategoryName, string[] Keywords)>>
    {
      { "Jedzenie", new List<(string, string[])>
        {
          ("Supermarkety", new[] { "Biedronka", "Lidl", "Auchan", "Żabka" }),
          ("Restauracje", new[] { "KFC", "McDonalds", "Pizza Hut", "Restauracja Starówka" })
        }
      },
      { "Transport", new List<(string, string[])>
        {
          ("Paliwo", new[] { "Orlen", "Shell", "BP", "Circle K" }),
          ("Bilety", new[] { "ZTM", "PKP", "Uber", "Bolt" })
        }
      },
      { "Czas Wolny", new List<(string, string[])>
        {
          ("Rozrywka", new[] { "Netflix", "Spotify", "Kino Cinema", "Basen" })
        }
      },
      { "Dom", new List<(string, string[])>
        {
          ("Czynsz i Media", new[] { "Wspólnota", "Energa", "Gazownia", "Internet" })
        }
      }
    };

    var categories = new List<cCategory>();
    var subcategoryKeywordMap = new Dictionary<string, string[]>();

    foreach (var (categoryName, subcategories) in categoryStructure) {
      var category = new cCategory { Name = categoryName };
      categories.Add(category);
      await _context.Categories.AddAsync(category);
      await _context.SaveChangesAsync(); // Zapisz kategorię, aby uzyskać ID

      foreach (var (subcategoryName, keywords) in subcategories) {
        var subcategory = new cSubcategory {
          Name = subcategoryName,
          CategoryId = category.Id
        };
        await _context.Subcategories.AddAsync(subcategory);
        subcategoryKeywordMap[subcategoryName] = keywords;
      }
      await _context.SaveChangesAsync(); // Zapisz podkategorie
    }

    // 4. GENEROWANIE TRANSAKCJI
    // 4. GENEROWANIE TRANSAKCJI
    var transactions = new List<cTransaction>();
    var allSubcategories = await _context.Subcategories.Include(s => s.Category).ToListAsync();
    var accountIds = cAccounts.Select(a => a.Id).ToList(); // Pobieramy wygenerowane przez bazę ID

    for (int i = 0; i < transactionCount; i++) {
      var subcategory = faker.PickRandom(allSubcategories);
      var keywords = subcategoryKeywordMap[subcategory.Name];
      var amount = faker.Random.Decimal(-300, -10);
      var generatedDate = faker.Date.Past(1).ToUniversalTime(); // Konwersja na UTC

      transactions.Add(new cTransaction {
        Date = generatedDate,
        Description = $"{faker.PickRandom(keywords)} {generatedDate:dd/MM}",
        Amount = amount,
        AccountId = faker.PickRandom(accountIds),
        CategoryId = subcategory.CategoryId,
        SubcategoryId = subcategory.Id,
        MonthNumber = generatedDate.Month,
        Year = generatedDate.Year
      });
    }

    // Dodaj kilka wpływów (Wynagrodzenie)
    var incomeCategory = new cCategory { Name = "Przychody" };
    await _context.Categories.AddAsync(incomeCategory);
    await _context.SaveChangesAsync();

    var salarySubcategory = new cSubcategory {
      Name = "Wynagrodzenie",
      CategoryId = incomeCategory.Id
    };
    await _context.Subcategories.AddAsync(salarySubcategory);
    await _context.SaveChangesAsync();

    for (int i = 0; i < 12; i++) {
      var salaryDate = DateTime.UtcNow.AddMonths(-i).AddDays(-faker.Random.Int(1, 5)); // Użyj UtcNow zamiast Now
      transactions.Add(new cTransaction {
        Date = salaryDate,
        Description = "Przelew wynagrodzenia FIRMA XYZ",
        Amount = faker.Random.Decimal(5000, 7000),
        AccountId = cAccounts[0].Id,
        CategoryId = incomeCategory.Id,
        SubcategoryId = salarySubcategory.Id,
        MonthNumber = salaryDate.Month,
        Year = salaryDate.Year
      });
    }

    await _context.Transactions.AddRangeAsync(transactions);
    await _context.SaveChangesAsync();
  }
}
