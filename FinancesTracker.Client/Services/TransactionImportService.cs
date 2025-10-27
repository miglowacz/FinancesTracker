using System.Globalization;
using System.Text;
using FinancesTracker.Shared.Models;
using FinancesTracker.Shared.DTOs; // Upewnij siê, ¿e masz DTO
using System.Net.Http.Json;

namespace FinancesTracker.Client.Services;

public class TransactionImportService
{
  private readonly HttpClient _http;

  public TransactionImportService(HttpClient http)
  {
    _http = http;
  }

  public async Task<List<cTransaction>> ImportFromMbankCsvAsync(Stream csvStream)
  {
    var transactions = new List<cTransaction>();
    using var reader = new StreamReader(csvStream, Encoding.UTF8, true);

    // Przewiñ do nag³ówka kolumn
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
      if (line.Trim().StartsWith("#Data operacji")) break;
    }

    // Parsuj transakcje
    while ((line = await reader.ReadLineAsync()) != null)
    {
      if (string.IsNullOrWhiteSpace(line)) continue;
      var parts = SplitCsvLine(line);
      if (parts.Length < 5) continue;

      // Przyk³ad: 2024-01-31;"gopass.travel ...";"Bie¿¹ce ...";"Podró¿e ...";-25,00 PLN;
      if (!DateTime.TryParse(parts[0], out var date)) continue;

      var description = parts[1].Trim('"');
      var account = parts[2].Trim('"');
      var category = parts[3].Trim('"');
      var amountStr = parts[4].Replace("PLN", "").Replace(" ", "").Replace("\"", "");
      if (!decimal.TryParse(amountStr, NumberStyles.Any, new CultureInfo("pl-PL"), out var amount)) continue;

      transactions.Add(new cTransaction
      {
        Date = date,
        Description = description,
        //Account = account,
        //Category = category,
        Amount = amount
      });
    }

    return transactions;
  }

  // Prosty parser CSV z obs³ug¹ pól w cudzys³owie
  private static string[] SplitCsvLine(string line)
  {
    var result = new List<string>();
    var sb = new StringBuilder();
    bool inQuotes = false;

    foreach (var c in line)
    {
      if (c == '"')
      {
        inQuotes = !inQuotes;
        continue;
      }
      if (c == ';' && !inQuotes)
      {
        result.Add(sb.ToString());
        sb.Clear();
      }
      else
      {
        sb.Append(c);
      }
    }
    result.Add(sb.ToString());
    return result.ToArray();
  }

  public async Task<bool> ImportTransactionsAsync(List<cTransaction> transactions)
  {




    var response = await _http.PostAsJsonAsync("api/transactions/import", transactions);

    return response.IsSuccessStatusCode;
  }

  public async Task<bool> ImportTransactionsAsync(
    List<cTransaction> transactions,
    Dictionary<string, int> categoryNameToId,
    Dictionary<(int categoryId, string subcategoryName), int> subcategoryKeyToId)
  {
    // Mapowanie cTransaction na TransactionDto
    var dtos = new List<TransactionDto>();
    foreach (var t in transactions)
    {
      // Pobierz CategoryId na podstawie nazwy kategorii
      //categoryNameToId.TryGetValue(t.Category, out var categoryId);

      //// Pobierz SubcategoryId na podstawie CategoryId i nazwy podkategorii (jeœli masz podkategoriê w cTransaction)
      //// Jeœli nie masz podkategorii, mo¿esz pomin¹æ lub ustawiæ na 0
      //int subcategoryId = 0;
      //if (subcategoryKeyToId.TryGetValue((categoryId, t.Subcategory ?? ""), out var subId))
      //  subcategoryId = subId;

      dtos.Add(new TransactionDto
      {
        Date = t.Date,
        Description = t.Description,
        Amount = t.Amount,
        //CategoryId = categoryId,
        //SubcategoryId = subcategoryId,
        BankName = "mbank"
      });
    }

    var response = await _http.PostAsJsonAsync("api/transactions/import", dtos);
    return response.IsSuccessStatusCode;
  }
}