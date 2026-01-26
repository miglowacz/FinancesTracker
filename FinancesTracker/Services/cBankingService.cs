using System.Net.Http.Json;

using FinancesTracker.Data;
using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Services {

  public interface IBankingService {
    Task<string> GetAuthUrlAsync(string bankId); // Link do logowania w banku
    Task HandleCallbackAsync(string code);      // Zamiana kodu na token
    Task SyncTransactionsAsync(int connectionId); // Pobranie danych do Twojej bazy
  }

  public class cBankingService : IBankingService {
    private readonly HttpClient _httpClient;
    private readonly ITransactionService _transactionService;
    private readonly FinancesTrackerDbContext _context;

    public cBankingService(HttpClient httpClient, ITransactionService transactionService, FinancesTrackerDbContext context) {
      _httpClient = httpClient;
      _transactionService = transactionService;
      _context = context;
    }

    public async Task<string> GetAuthUrlAsync(string bankId) {
      // Logika uderzenia do https://api.enablebanking.com/sessions
      // Musisz wysłać Client ID i poprosić o dostęp do "accounts" i "transactions"
      return "https://api.enablebanking.com/auth?...";
    }

    public async Task SyncTransactionsAsync(int connectionId) {
      var connection = await _context.BankConnections.FindAsync(connectionId);

      // 1. Pobierz transakcje z Enable Banking (format JSON)
      // GET https://api.enablebanking.com/accounts/{id}/transactions
      var rawTransactions = await _httpClient.GetFromJsonAsync<List<EnableBankingTx>>("...");

      // 2. Mapowanie na Twoje cTransaction_DTO
      var dtos = rawTransactions.Select(t => new cTransaction_DTO {
        Date = t.BookingDate,
        Description = t.RemittanceInformationUnstructured,
        Amount = t.TransactionAmount.Amount,
        AccountId = connection.AccountId,
        AccountName = "Synchronizacja Bankowa"
      }).ToList();

      // 3. Wykorzystaj istniejący mechanizm importu!
      // Dzięki temu zadziałają Twoje reguły kategoryzacji i parowanie transferów.
      await _transactionService.ImportTransactionsAsync(dtos);
    }
  }
}
