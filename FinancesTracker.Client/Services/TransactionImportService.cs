using System.Globalization;
using System.Text;
using FinancesTracker.Shared.Models;
using FinancesTracker.Shared.DTOs;
using System.Net.Http.Json;

namespace FinancesTracker.Client.Services;

public class TransactionImportService {

  private readonly HttpClient mHttp;

  public TransactionImportService(HttpClient xHttp) {
    //xHttp - klient HTTP do komunikacji z API

    mHttp = xHttp;

  }

  public async Task<List<cTransaction>> ImportFromMbankCsvAsync(Stream xCsvStream) {
    //funkcja importuje transakcje z pliku CSV mbanku
    //xCsvStream - strumień pliku CSV mbanku

    List<cTransaction> pTransactions = new();
    using StreamReader pReader = new(xCsvStream, Encoding.UTF8, true);

    //przewiń do nagłówka kolumn
    string? pLine;

    while ((pLine = await pReader.ReadLineAsync()) != null) {
      if (pLine.Trim().StartsWith("#Data operacji")) break;
    }

    //parsuj transakcje
    while ((pLine = await pReader.ReadLineAsync()) != null) {
      if (string.IsNullOrWhiteSpace(pLine)) continue;
      string[] pParts = SplitCsvLine(pLine);
      if (pParts.Length < 5) continue;

      //przykład: 2024-01-31;"gopass.travel ...";"Bieżące ...";"Podróże ...";-25,00 PLN;
      if (!DateTime.TryParse(pParts[0], out DateTime pDate)) continue;

      string pDescription = pParts[1].Trim('"');
      string pAccount = pParts[2].Trim('"');
      string pCategory = pParts[3].Trim('"');
      string pAmountStr = pParts[4].Replace("PLN", "").Replace(" ", "").Replace("\"", "");
      if (!decimal.TryParse(pAmountStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal pAmount)) continue;

      pTransactions.Add(new cTransaction {
        Date = pDate,
        Description = pDescription,
        //Account = pAccount,
        //Category = pCategory,
        Amount = pAmount
      });
    }

    return pTransactions;

  }

  private static string[] SplitCsvLine(string xLine) {
    //funkcja dzieli linię CSV na części, obsługuje pola w cudzysłowie
    //xLine - linia tekstu CSV

    List<string> pResult = new();
    StringBuilder pSb = new();
    bool pInQuotes = false;

    foreach (char pC in xLine) {
      if (pC == '"') {
        pInQuotes = !pInQuotes;
        continue;
      }
      if (pC == ';' && !pInQuotes) {
        pResult.Add(pSb.ToString());
        pSb.Clear();
      } else {
        pSb.Append(pC);
      }
    }
    pResult.Add(pSb.ToString());
    return pResult.ToArray();

  }

  public async Task<bool> ImportTransactionsAsync(List<cTransaction> xTransactions) {
    //funkcja importuje listę transakcji do API
    //xTransactions - lista transakcji do zaimportowania

    List<TransactionDto> pTransactionDtos = new();

    List<TransactionDto> pDtos = new();

    foreach (cTransaction pT in xTransactions) {
      //pobierz CategoryId na podstawie nazwy kategorii
      //xCategoryNameToId.TryGetValue(pT.Category, out int pCategoryId);

      //pobierz SubcategoryId na podstawie CategoryId i nazwy podkategorii (jeśli masz podkategorię w cTransaction)
      //int pSubcategoryId = 0;
      //if (xSubcategoryKeyToId.TryGetValue((pCategoryId, pT.Subcategory ?? ""), out int pSubId))
      //  pSubcategoryId = pSubId;

      pDtos.Add(new TransactionDto {
        Date = pT.Date,
        Description = pT.Description,
        Amount = pT.Amount,
        //CategoryId = pCategoryId,
        //SubcategoryId = pSubcategoryId,
        BankName = "mbank"
      });
    }
    var pResponse = await mHttp.PostAsJsonAsync("api/transactions/import", pDtos);

    return pResponse.IsSuccessStatusCode;

  }

  public async Task<bool> ImportTransactionsAsync(
    List<cTransaction> xTransactions,
    Dictionary<string, int> xCategoryNameToId,
    Dictionary<(int xCategoryId, string xSubcategoryName), int> xSubcategoryKeyToId) {
    //funkcja importuje transakcje z mapowaniem kategorii i podkategorii
    //xTransactions - lista transakcji do zaimportowania
    //xCategoryNameToId - słownik mapujący nazwę kategorii na jej ID
    //xSubcategoryKeyToId - słownik mapujący (ID kategorii, nazwa podkategorii) na ID podkategorii

    List<TransactionDto> pDtos = new();
    foreach (cTransaction pT in xTransactions) {
      //pobierz CategoryId na podstawie nazwy kategorii
      //xCategoryNameToId.TryGetValue(pT.Category, out int pCategoryId);

      //pobierz SubcategoryId na podstawie CategoryId i nazwy podkategorii (jeśli masz podkategorię w cTransaction)
      //int pSubcategoryId = 0;
      //if (xSubcategoryKeyToId.TryGetValue((pCategoryId, pT.Subcategory ?? ""), out int pSubId))
      //  pSubcategoryId = pSubId;

      pDtos.Add(new TransactionDto {
        Date = pT.Date,
        Description = pT.Description,
        Amount = pT.Amount,
        //CategoryId = pCategoryId,
        //SubcategoryId = pSubcategoryId,
        BankName = "mbank"
      });
    }

    var pResponse = await mHttp.PostAsJsonAsync("api/transactions/import", pDtos);

    return pResponse.IsSuccessStatusCode;

  }
}
