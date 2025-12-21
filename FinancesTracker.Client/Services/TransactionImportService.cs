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

  public async Task<List<cTransaction>> ImportFromMillenniumCsvAsync(Stream xCsvStream) {
    //funkcja importuje transakcje z pliku CSV Millennium
    //xCsvStream - strumień pliku CSV Millennium

    List<cTransaction> pTransactions = new();
    using StreamReader pReader = new(xCsvStream, Encoding.UTF8, true);

    //pomiń nagłówek
    string? pLine = await pReader.ReadLineAsync();

    //parsuj transakcje
    while ((pLine = await pReader.ReadLineAsync()) != null) {
      if (string.IsNullOrWhiteSpace(pLine)) continue;
      
      string[] pParts = SplitCsvLineWithComma(pLine);
      if (pParts.Length < 11) continue;

      //format: "Numer rachunku","Data transakcji","Data rozliczenia","Rodzaj transakcji","Na konto/Z konta","Odbiorca/Zleceniodawca","Opis","Obciążenia","Uznania","Saldo","Waluta"
      //indeksy: 0-numer, 1-data transakcji, 2-data rozliczenia, 3-rodzaj, 4-konto, 5-odbiorca, 6-opis, 7-obciążenia, 8-uznania, 9-saldo, 10-waluta
      
      if (!DateTime.TryParse(pParts[1].Trim('"'), out DateTime pDate)) continue;

      string pDescription = BuildMillenniumDescription(pParts);
      
      //pobierz kwotę z kolumny obciążenia (7) lub uznania (8)
      decimal pAmount = 0;
      string pDebitStr = pParts[7].Trim('"').Replace(" ", "").Replace(".", ",");
      string pCreditStr = pParts[8].Trim('"').Replace(" ", "").Replace(".", ",");

      if (!string.IsNullOrEmpty(pDebitStr)) {

        if (decimal.TryParse(pDebitStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal pDebit))
          pAmount = -Math.Abs(pDebit); //obciążenie jest ujemne
      }
      else if (!string.IsNullOrEmpty(pCreditStr)) {
        if (decimal.TryParse(pCreditStr, NumberStyles.Any, new CultureInfo("pl-PL"), out decimal pCredit))
          pAmount = Math.Abs(pCredit); //uznanie jest dodatnie
      }

      if (pAmount == 0) continue; //pomiń transakcje bez kwoty

      pTransactions.Add(new cTransaction {
        Date = pDate,
        Description = pDescription,
        Amount = pAmount
      });
    }

    return pTransactions;

  }

  private static string BuildMillenniumDescription(string[] xParts) {
    //funkcja buduje opis transakcji z danych Millennium
    //xParts - tablica części linii CSV

    StringBuilder pSb = new();
    
    //rodzaj transakcji
    string pType = xParts[3].Trim('"');
    if (!string.IsNullOrEmpty(pType))
      pSb.Append(pType);

    //odbiorca/zleceniodawca
    string pCounterparty = xParts[5].Trim('"');
    if (!string.IsNullOrEmpty(pCounterparty)) {
      if (pSb.Length > 0) pSb.Append(" - ");
      pSb.Append(pCounterparty);
    }

    //opis
    string pDescription = xParts[6].Trim('"');
    if (!string.IsNullOrEmpty(pDescription)) {
      if (pSb.Length > 0) pSb.Append(" - ");
      pSb.Append(pDescription);
    }

    return pSb.Length > 0 ? pSb.ToString() : "Transakcja Millennium";

  }

  private static string[] SplitCsvLine(string xLine) {
    //funkcja dzieli linię CSV na części, obsługuje pola w cudzysłowie (separator: średnik)
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

  private static string[] SplitCsvLineWithComma(string xLine) {
    //funkcja dzieli linię CSV na części, obsługuje pola w cudzysłowie (separator: przecinek)
    //xLine - linia tekstu CSV

    List<string> pResult = new();
    StringBuilder pSb = new();
    bool pInQuotes = false;

    foreach (char pC in xLine) {
      if (pC == '"') {
        pInQuotes = !pInQuotes;
        pSb.Append(pC); //zachowaj cudzysłowy dla łatwiejszego trim
        continue;
      }
      if (pC == ',' && !pInQuotes) {
        pResult.Add(pSb.ToString());
        pSb.Clear();
      } else {
        pSb.Append(pC);
      }
    }
    pResult.Add(pSb.ToString());
    return pResult.ToArray();

  }

  public async Task<bool> ImportTransactionsAsync(List<cTransaction> xTransactions, string xBankName = "mbank") {
    //funkcja importuje listę transakcji do API
    //xTransactions - lista transakcji do zaimportowania
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
    var pResponse = await mHttp.PostAsJsonAsync("api/transactions/import", pDtos);

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

    var pResponse = await mHttp.PostAsJsonAsync("api/transactions/import", pDtos);

    return pResponse.IsSuccessStatusCode;

  }
}
