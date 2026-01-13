using System.Globalization;
using System.Text;
using FinancesTracker.Shared.Models;
using FinancesTracker.Shared.DTOs;
using System.Net.Http.Json;

namespace FinancesTracker.Client.Services;

public class cTransactionImportService {

  private readonly HttpClient _httpClient;

  // Lista słów kluczowych oznaczających transfer
  private readonly string[] _transferKeywords = new[] {
    "PRZELEW WŁASNY",
    "PRZELEW WEWNĘTRZNY",
    "TRANSFER",
    "WPŁATA WŁASNA"
  };

  public cTransactionImportService(HttpClient httpClient) {
    _httpClient = httpClient;
  }

  public async Task<List<cTransaction>> ImportFromMbankCsvAsync(Stream csvStream) {

    List<cTransaction> transactions = new();

    using StreamReader streamReader = new(csvStream, Encoding.UTF8, true);

    string? line;

    while ((line = await streamReader.ReadLineAsync()) != null) {
      if (line.Trim().StartsWith("#Data operacji")) break;
    }

    while ((line = await streamReader.ReadLineAsync()) != null) {
      if (string.IsNullOrWhiteSpace(line)) continue;
      string[] parts = SplitCsvLine(line);
      if (parts.Length < 5) continue;

      if (!DateTime.TryParse(parts[0], out DateTime date)) continue;

      string description = parts[1].Trim('"');
      string accountName = parts[2].Trim('"');
      string category = parts[3].Trim('"');
      string amountStr = parts[4].Replace("PLN", "").Replace(" ", "").Replace("\"", "");
      if (!decimal.TryParse(amountStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal amount)) continue;

      // Sprawdź czy to transfer
      bool isTransfer = CheckIfTransfer(description);

      transactions.Add(new cTransaction {
        Date = date,
        Description = description,
        Amount = amount,
        AccountName = accountName,
        IsTransfer = isTransfer // Ustaw flagę
      });
    }

    return transactions;

  }

  public async Task<List<cTransaction>> ImportFromMillenniumCsvAsync(Stream csvStream) {

    List<cTransaction> transactionsCln = new();
    using StreamReader reader = new(csvStream, Encoding.UTF8, true);

    string? line = await reader.ReadLineAsync();

    while ((line = await reader.ReadLineAsync()) != null) {
      if (string.IsNullOrWhiteSpace(line)) continue;

      string[] parts = SplitCsvLineWithComma(line);
      if (parts.Length < 11) continue;

      if (!DateTime.TryParse(parts[1].Trim('"'), out DateTime date)) continue;

      string description = BuildMillenniumDescription(parts);

      decimal amount = 0;
      string debitStr = parts[7].Trim('"').Replace(" ", "").Replace(".", ",");
      string creditStr = parts[8].Trim('"').Replace(" ", "").Replace(".", ",");

      if (!string.IsNullOrEmpty(debitStr)) {
        if (decimal.TryParse(debitStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal debit))
          amount = -Math.Abs(debit);
      }
      else if (!string.IsNullOrEmpty(creditStr)) {
        if (decimal.TryParse(creditStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal credit))
          amount = Math.Abs(credit);
      }

      if (amount == 0) continue;

      string accountName = parts[0].Trim('"');
      
      // Sprawdź czy to transfer
      bool isTransfer = CheckIfTransfer(description);

      transactionsCln.Add(new cTransaction {
        Date = date,
        Description = description,
        Amount = amount,
        AccountName = accountName,
        IsTransfer = isTransfer // Ustaw flagę
      });
    }

    return transactionsCln;
  }

  // Metoda pomocnicza do wykrywania transferów
  private bool CheckIfTransfer(string description) {
    if (string.IsNullOrWhiteSpace(description)) return false;
    var upperDesc = description.ToUpper();
    return _transferKeywords.Any(k => upperDesc.Contains(k));
  }

  private static string BuildMillenniumDescription(string[] parts) {

    StringBuilder stringBuilder = new();
    
    string pType = parts[3].Trim('"');
    if (!string.IsNullOrEmpty(pType))
      stringBuilder.Append(pType);

    string pCounterparty = parts[5].Trim('"');
    if (!string.IsNullOrEmpty(pCounterparty)) {
      if (stringBuilder.Length > 0) stringBuilder.Append(" - ");
      stringBuilder.Append(pCounterparty);
    }

    string description = parts[6].Trim('"');
    if (!string.IsNullOrEmpty(description)) {
      if (stringBuilder.Length > 0) stringBuilder.Append(" - ");
      stringBuilder.Append(description);
    }

    return stringBuilder.Length > 0 ? stringBuilder.ToString() : "Transakcja Millennium";
  }

  private static string[] SplitCsvLine(string line) {

    List<string> result = new();
    StringBuilder stringBuilder = new();
    bool inQuotes = false;

    foreach (char @char in line) {
      if (@char == '"') {
        inQuotes = !inQuotes;
        continue;
      }
      if (@char == ';' && !inQuotes) {
        result.Add(stringBuilder.ToString());
        stringBuilder.Clear();
      } else {
        stringBuilder.Append(@char);
      }
    }
    result.Add(stringBuilder.ToString());

    return result.ToArray();

  }

  private static string[] SplitCsvLineWithComma(string line) {

    List<string> result = new();
    StringBuilder stringBuilder = new();
    bool pInQuotes = false;

    foreach (char pC in line) {
      if (pC == '"') {
        pInQuotes = !pInQuotes;
        stringBuilder.Append(pC);
        continue;
      }
      if (pC == ',' && !pInQuotes) {
        result.Add(stringBuilder.ToString());
        stringBuilder.Clear();
      } else {
        stringBuilder.Append(pC);
      }
    }

    result.Add(stringBuilder.ToString());

    return result.ToArray();

  }

  public async Task<bool> ImportTransactionsAsync(List<cTransaction> ransactionsCln) {

    List<cTransaction_DTO> transactions_DTO_Cln = new();

    foreach (cTransaction transaction in ransactionsCln) {
      transactions_DTO_Cln.Add(new cTransaction_DTO {
        Date = transaction.Date,
        Description = transaction.Description,
        Amount = transaction.Amount,
        AccountName = transaction.AccountName,
        IsTransfer = transaction.IsTransfer // Przekaż flagę IsTransfer do DTO
      });
    }

    var pResponse = await _httpClient.PostAsJsonAsync("api/transactions/import", transactions_DTO_Cln);

    return pResponse.IsSuccessStatusCode;

  }

  public async Task<bool> ImportTransactionsAsync(

    List<cTransaction> xTransactions,
    Dictionary<string, int> xCategoryNameToId,
    Dictionary<(int xCategoryId, string xSubcategoryName), int> xSubcategoryKeyToId,
    string xBankName = "mbank") {
    List<cTransaction_DTO> pDtos = new();
    
    foreach (cTransaction pT in xTransactions) {
      pDtos.Add(new cTransaction_DTO {
        Date = pT.Date,
        Description = pT.Description,
        Amount = pT.Amount,
        AccountName = pT.AccountName,
        IsTransfer = pT.IsTransfer // Przekaż flagę IsTransfer do DTO
      });
    }

    var pResponse = await _httpClient.PostAsJsonAsync("api/transactions/import", pDtos);

    return pResponse.IsSuccessStatusCode;

  }
}
