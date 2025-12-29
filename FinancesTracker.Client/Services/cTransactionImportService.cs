using System.Globalization;
using System.Text;
using FinancesTracker.Shared.Models;
using FinancesTracker.Shared.DTOs;
using System.Net.Http.Json;

namespace FinancesTracker.Client.Services;

public class cTransactionImportService {

  private readonly HttpClient _httpClient;

  public cTransactionImportService(HttpClient httpClient) {
    //xHttp - klient HTTP do komunikacji z API

    _httpClient = httpClient;

  }

  public async Task<List<cTransaction>> ImportFromMbankCsvAsync(Stream csvStream) {
    //funkcja importuje transakcje z pliku CSV mbanku
    //xCsvStream - strumień pliku CSV mbanku

    List<cTransaction> transactions = new();
    using StreamReader streamReader = new(csvStream, Encoding.UTF8, true);

    //przewiń do nagłówka kolumn
    string? line;

    while ((line = await streamReader.ReadLineAsync()) != null) {
      if (line.Trim().StartsWith("#Data operacji")) break;
    }

    //parsuj transakcje
    while ((line = await streamReader.ReadLineAsync()) != null) {
      if (string.IsNullOrWhiteSpace(line)) continue;
      string[] parts = SplitCsvLine(line);
      if (parts.Length < 5) continue;

      //przykład: 2024-01-31;"gopass.travel ...";"Bieżące ...";"Podróże ...";-25,00 PLN;
      if (!DateTime.TryParse(parts[0], out DateTime date)) continue;

      string description = parts[1].Trim('"');
      string account = parts[2].Trim('"');
      string category = parts[3].Trim('"');
      string amountStr = parts[4].Replace("PLN", "").Replace(" ", "").Replace("\"", "");
      if (!decimal.TryParse(amountStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal amount)) continue;

      transactions.Add(new cTransaction {
        Date = date,
        Description = description,
        //Account = account,
        //Category = category,
        Amount = amount
      });
    }

    return transactions;

  }

  public async Task<List<cTransaction>> ImportFromMillenniumCsvAsync(Stream csvStream) {
    //funkcja importuje transakcje z pliku CSV Millennium
    //csvStream - strumień pliku CSV Millennium

    List<cTransaction> transactionsCln = new();
    using StreamReader reader = new(csvStream, Encoding.UTF8, true);

    //pomiń nagłówek
    string? line = await reader.ReadLineAsync();

    //parsuj transakcje
    while ((line = await reader.ReadLineAsync()) != null) {
      if (string.IsNullOrWhiteSpace(line)) continue;

      string[] parts = SplitCsvLineWithComma(line);
      if (parts.Length < 11) continue;

      //format: "Numer rachunku","Data transakcji","Data rozliczenia","Rodzaj transakcji","Na konto/Z konta","Odbiorca/Zleceniodawca","Opis","Obciążenia","Uznania","Saldo","Waluta"
      //indeksy: 0-numer, 1-data transakcji, 2-data rozliczenia, 3-rodzaj, 4-konto, 5-odbiorca, 6-opis, 7-obciążenia, 8-uznania, 9-saldo, 10-waluta
      
      if (!DateTime.TryParse(parts[1].Trim('"'), out DateTime date)) continue;

      string description = BuildMillenniumDescription(parts);

      //pobierz kwotę z kolumny obciążenia (7) lub uznania (8)
      decimal amount = 0;
      string debitStr = parts[7].Trim('"').Replace(" ", "").Replace(".", ",");
      string creditStr = parts[8].Trim('"').Replace(" ", "").Replace(".", ",");

      if (!string.IsNullOrEmpty(debitStr)) {

        if (decimal.TryParse(debitStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal debit))
          amount = -Math.Abs(debit); //obciążenie jest ujemne
      }
      else if (!string.IsNullOrEmpty(creditStr)) {
        if (decimal.TryParse(creditStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal credit))
          amount = Math.Abs(credit); //uznanie jest dodatnie
      }

      if (amount == 0) continue; //pomiń transakcje bez kwoty

      transactionsCln.Add(new cTransaction {
        Date = date,
        Description = description,
        Amount = amount
      });
    }

    return transactionsCln;

  }

  private static string BuildMillenniumDescription(string[] parts) {
    //funkcja buduje opis transakcji z danych Millennium
    //xParts - tablica części linii CSV

    StringBuilder stringBuilder = new();
    
    //rodzaj transakcji
    string pType = parts[3].Trim('"');
    if (!string.IsNullOrEmpty(pType))
      stringBuilder.Append(pType);

    //odbiorca/zleceniodawca
    string pCounterparty = parts[5].Trim('"');
    if (!string.IsNullOrEmpty(pCounterparty)) {
      if (stringBuilder.Length > 0) stringBuilder.Append(" - ");
      stringBuilder.Append(pCounterparty);
    }

    //opis
    string description = parts[6].Trim('"');
    if (!string.IsNullOrEmpty(description)) {
      if (stringBuilder.Length > 0) stringBuilder.Append(" - ");
      stringBuilder.Append(description);
    }

    return stringBuilder.Length > 0 ? stringBuilder.ToString() : "Transakcja Millennium";

  }

  private static string[] SplitCsvLine(string line) {
    //funkcja dzieli linię CSV na części, obsługuje pola w cudzysłowie (separator: średnik)
    //xLine - linia tekstu CSV

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
    //funkcja dzieli linię CSV na części, obsługuje pola w cudzysłowie (separator: przecinek)
    //xLine - linia tekstu CSV

    List<string> result = new();
    StringBuilder stringBuilder = new();
    bool pInQuotes = false;

    foreach (char pC in line) {
      if (pC == '"') {
        pInQuotes = !pInQuotes;
        stringBuilder.Append(pC); //zachowaj cudzysłowy dla łatwiejszego trim
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

  public async Task<bool> ImportTransactionsAsync(List<cTransaction> ransactionsCln, string bankName = "mbank") {
    //funkcja importuje listę transakcji do API
    //xTransactions - lista transakcji do zaimportowania
    //xBankName - nazwa banku

    List<cTransaction_DTO> transactions_DTO_Cln = new();

    foreach (cTransaction transaction in ransactionsCln) {
      transactions_DTO_Cln.Add(new cTransaction_DTO {
        Date = transaction.Date,
        Description = transaction.Description,
        Amount = transaction.Amount,
        BankName = bankName
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
    //funkcja importuje transakcje z mapowaniem kategorii i podkategorii
    //xTransactions - lista transakcji do zaimportowania
    //xCategoryNameToId - słownik mapujący nazwę kategorii na jej ID
    //xSubcategoryKeyToId - słownik mapujący (ID kategorii, nazwa podkategorii) na ID podkategorii
    //xBankName - nazwa banku

    List<cTransaction_DTO> pDtos = new();
    foreach (cTransaction pT in xTransactions) {
      pDtos.Add(new cTransaction_DTO {
        Date = pT.Date,
        Description = pT.Description,
        Amount = pT.Amount,
        BankName = xBankName
      });
    }

    var pResponse = await _httpClient.PostAsJsonAsync("api/transactions/import", pDtos);

    return pResponse.IsSuccessStatusCode;

  }
}
