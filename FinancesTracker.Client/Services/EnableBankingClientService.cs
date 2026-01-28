using System.Net.Http.Json;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.DTOs.EnableBanking;

namespace FinancesTracker.Client.Services;

public interface IEnableBankingClientService {
  Task<List<Aspsp_DTO>> GetAspspsAsync(string country);
  Task<AuthResponse_DTO> StartAuthorizationAsync(string aspspName, string country);
  Task<List<BankTransaction_DTO>> GetTransactionsAsync(string sessionId, string accountId, DateTime? dateFrom = null, DateTime? dateTo = null);
}

public class EnableBankingClientService : IEnableBankingClientService {
  private readonly HttpClient _httpClient;

  public EnableBankingClientService(HttpClient httpClient) {
    _httpClient = httpClient;
  }

  public async Task<List<Aspsp_DTO>> GetAspspsAsync(string country) {
    var response = await _httpClient.GetFromJsonAsync<cApiResponse<List<Aspsp_DTO>>>($"/api/enablebanking/aspsps/{country}");
    return response?.Data ?? new List<Aspsp_DTO>();
  }

  public async Task<AuthResponse_DTO> StartAuthorizationAsync(string aspspName, string country) {
    var request = new AuthRequest_DTO { AspspName = aspspName, Country = country };
    var response = await _httpClient.PostAsJsonAsync("/api/enablebanking/start-auth", request);
    response.EnsureSuccessStatusCode();
    
    var result = await response.Content.ReadFromJsonAsync<cApiResponse<AuthResponse_DTO>>();
    return result?.Data ?? throw new InvalidOperationException("Failed to start authorization");
  }

  public async Task<List<BankTransaction_DTO>> GetTransactionsAsync(
    string sessionId,
    string accountId,
    DateTime? dateFrom = null,
    DateTime? dateTo = null) {
    var queryParams = new List<string>();
    if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
    if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
    
    var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
    var response = await _httpClient.GetFromJsonAsync<cApiResponse<List<BankTransaction_DTO>>>($"/api/enablebanking/transactions/{sessionId}/{accountId}{query}");
    
    return response?.Data ?? new List<BankTransaction_DTO>();
  }
}
