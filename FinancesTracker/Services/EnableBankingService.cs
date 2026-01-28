using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FinancesTracker.Models;
using FinancesTracker.Shared.DTOs.EnableBanking;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinancesTracker.Services;

public interface IEnableBankingService {
  Task<List<Aspsp_DTO>> GetAspspsAsync(string country);
  Task<AuthResponse_DTO> StartAuthorizationAsync(string aspspName, string country, string redirectUrl);
  Task<SessionResponse_DTO> CreateSessionAsync(string code, string state);
  Task<List<BankTransaction_DTO>> GetTransactionsAsync(string sessionId, string accountId, DateTime? dateFrom = null, DateTime? dateTo = null);
}

public class EnableBankingService : IEnableBankingService {
  private readonly EnableBankingSettings _settings;
  private readonly HttpClient _httpClient;
  private readonly ILogger<EnableBankingService> _logger;
  private readonly Dictionary<string, string> _stateStorage = new();

  public EnableBankingService(
    IOptions<EnableBankingSettings> settings,
    HttpClient httpClient,
    ILogger<EnableBankingService> logger) {
    _settings = settings.Value;
    _httpClient = httpClient;
    _logger = logger;
    _httpClient.BaseAddress = new Uri(_settings.ApiOrigin);
  }

  private string CreateToken() {
    try {
      using RSA rsa = RSA.Create();
      rsa.ImportFromPem(File.ReadAllText(_settings.KeyPath));

      var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256) {
        CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
      };

      var now = DateTime.Now;
      var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

      var jwt = new JwtSecurityToken(
        audience: _settings.JwtAudience,
        issuer: _settings.JwtIssuer,
        claims: new Claim[] {
          new Claim(JwtRegisteredClaimNames.Iat, unixTimeSeconds.ToString(), ClaimValueTypes.Integer64)
        },
        expires: now.AddMinutes(30),
        signingCredentials: signingCredentials
      );
      
      jwt.Header.Add("kid", _settings.ApplicationId);
      
      return new JwtSecurityTokenHandler().WriteToken(jwt);
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to create JWT token");
      throw new InvalidOperationException("Failed to create authentication token", ex);
    }
  }

  private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string endpoint) {
    var request = new HttpRequestMessage(method, endpoint);
    request.Headers.Add("Authorization", $"Bearer {CreateToken()}");
    return request;
  }

  public async Task<List<Aspsp_DTO>> GetAspspsAsync(string country) {
    try {
      var request = CreateAuthenticatedRequest(HttpMethod.Get, $"/aspsps?country={country}");
      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<AspspsResponse_DTO>(content);

      return result?.Aspsps ?? new List<Aspsp_DTO>();

    
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to get ASPSPs for country {Country}", country);
      throw new InvalidOperationException($"Failed to retrieve banks for country {country}", ex);
    }
  }

  public async Task<AuthResponse_DTO> StartAuthorizationAsync(string aspspName, string country, string redirectUrl) {
    try {
      // 1. Budowanie body zgodnie z wymaganiami Enable Banking
      var state = Guid.NewGuid().ToString();
      var requestBody = new StartAuthorizationRequest_DTO {
        Aspsp = new StartAuthorizationRequest_DTO.AspspInfo {
          Name = aspspName,
          Country = country
        },
        RedirectUrl = redirectUrl,
        State = state,
        Access = new StartAuthorizationRequest_DTO.AccessInfo {
          ValidUntil = DateTime.UtcNow.AddDays(90).ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
      };

      // Zapisz state do weryfikacji przy callback
      _stateStorage[state] = aspspName;

      // 2. Użyj tej samej metody co w GetAspspsAsync (która działa)
      var request = CreateAuthenticatedRequest(HttpMethod.Post, "/auth");
      request.Content = new StringContent(
        JsonSerializer.Serialize(requestBody),
        Encoding.UTF8,
        "application/json"
      );

      // 3. Wysłanie zapytania
      var response = await _httpClient.SendAsync(request);

      if (!response.IsSuccessStatusCode) {
        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError("Enable Banking API Error. Status: {Status}, Body: {Error}", response.StatusCode, errorContent);

        // Tutaj zobaczysz w konsoli, dlaczego dokładnie link nie powstał
        throw new InvalidOperationException($"API error: {errorContent}");
      }

      var result = await response.Content.ReadFromJsonAsync<AuthResponse_DTO>();
      _logger.LogInformation("Authorization URL generated: {Url}", result?.Url);

      return result;

    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to start authorization for {AspspName}", aspspName);
      throw;
    }
  }

  public async Task<SessionResponse_DTO> CreateSessionAsync(string code, string state) {
    try {
      // Validate state
      if (!_stateStorage.ContainsKey(state)) {
        throw new InvalidOperationException("Invalid state parameter");
      }

      var requestBody = new {
        code = code
      };

      var request = CreateAuthenticatedRequest(HttpMethod.Post, "/sessions");
      request.Content = new StringContent(
        JsonSerializer.Serialize(requestBody),
        Encoding.UTF8,
        "application/json"
      );

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var sessionResult = JsonSerializer.Deserialize<JsonElement>(content);

      var sessionId = sessionResult.GetProperty("session_id").GetString() ?? throw new InvalidOperationException("No session ID returned");

      // Fetch accounts for this session
      var accounts = await GetAccountsAsync(sessionId);

      _stateStorage.Remove(state); // Clean up state

      return new SessionResponse_DTO {
        SessionId = sessionId,
        Accounts = accounts
      };
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to create session with code {Code}", code);
      throw new InvalidOperationException("Failed to create session", ex);
    }
  }

  private async Task<List<BankAccount_DTO>> GetAccountsAsync(string sessionId) {
    try {
      var request = CreateAuthenticatedRequest(HttpMethod.Get, $"/sessions/{sessionId}/accounts");
      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var accountsResult = JsonSerializer.Deserialize<JsonElement>(content);

      var accounts = new List<BankAccount_DTO>();
      if (accountsResult.TryGetProperty("accounts", out var accountsArray)) {
        foreach (var account in accountsArray.EnumerateArray()) {
          accounts.Add(new BankAccount_DTO {
            AccountId = account.GetProperty("account_id").GetString() ?? string.Empty,
            Iban = account.TryGetProperty("iban", out var iban) ? iban.GetString() ?? string.Empty : string.Empty,
            Name = account.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
            Balance = account.TryGetProperty("balance", out var balance) ? balance.TryGetDecimal(out var bal) ? bal : null : null,
            Currency = account.TryGetProperty("currency", out var currency) ? currency.GetString() ?? string.Empty : string.Empty
          });
        }
      }

      return accounts;
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to get accounts for session {SessionId}", sessionId);
      throw new InvalidOperationException("Failed to retrieve accounts", ex);
    }
  }

  public async Task<List<BankTransaction_DTO>> GetTransactionsAsync(
    string sessionId,
    string accountId,
    DateTime? dateFrom = null,
    DateTime? dateTo = null) {
    try {
      var fromDate = dateFrom?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
      var toDate = dateTo?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

      var endpoint = $"/sessions/{sessionId}/accounts/{accountId}/transactions?date_from={fromDate}&date_to={toDate}";
      var request = CreateAuthenticatedRequest(HttpMethod.Get, endpoint);
      
      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var transactionsResult = JsonSerializer.Deserialize<JsonElement>(content);

      var transactions = new List<BankTransaction_DTO>();
      if (transactionsResult.TryGetProperty("transactions", out var transactionsArray)) {
        foreach (var transaction in transactionsArray.EnumerateArray()) {
          transactions.Add(new BankTransaction_DTO {
            Id = transaction.TryGetProperty("transaction_id", out var id) ? id.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
            Description = transaction.TryGetProperty("remittance_information", out var desc) ? desc.GetString() ?? string.Empty : string.Empty,
            Amount = transaction.TryGetProperty("amount", out var amt) ? amt.GetDecimal() : 0,
            Date = transaction.TryGetProperty("booking_date", out var date) ? DateTime.Parse(date.GetString() ?? DateTime.UtcNow.ToString()) : DateTime.UtcNow,
            Currency = transaction.TryGetProperty("currency", out var currency) ? currency.GetString() ?? string.Empty : string.Empty,
            CreditorName = transaction.TryGetProperty("creditor_name", out var creditor) ? creditor.GetString() ?? string.Empty : string.Empty,
            DebtorName = transaction.TryGetProperty("debtor_name", out var debtor) ? debtor.GetString() ?? string.Empty : string.Empty
          });
        }
      }

      return transactions;
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to get transactions for session {SessionId} and account {AccountId}", sessionId, accountId);
      throw new InvalidOperationException("Failed to retrieve transactions", ex);
    }
  }
}
