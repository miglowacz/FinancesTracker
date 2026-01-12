using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using System.Net.Http.Json;

namespace FinancesTracker.Client.Services;

public class cAccountService {
  private readonly HttpClient _http;
  private readonly cApiService _apiService;

  public cAccountService(HttpClient http, cApiService apiService) {
    _http = http;
    _apiService = apiService;
  }

  public async Task<cApiResponse<List<cAccount_DTO>>> GetAccountsAsync() {
    return await _apiService.GetAsync<List<cAccount_DTO>>(cAppConstants.ApiEndpoints.Accounts);
  }

  public async Task<List<cAccount_DTO>> GetAllAsync() {
    var response = await _http.GetFromJsonAsync<cApiResponse<List<cAccount_DTO>>>("api/accounts");
    return response?.Data ?? new List<cAccount_DTO>();
  }

  public async Task<cApiResponse<cAccount_DTO>> GetAccountAsync(int id) {
    return await _apiService.GetAsync<cAccount_DTO>($"{cAppConstants.ApiEndpoints.Accounts}/{id}");
  }

  public async Task<cApiResponse<cAccount_DTO>> CreateAccountAsync(cAccount_DTO account) {
    return await _apiService.PostAsync<cAccount_DTO>(cAppConstants.ApiEndpoints.Accounts, account);
  }

  public async Task<cApiResponse<cAccount_DTO>> UpdateAccountAsync(int id, cAccount_DTO account) {
    return await _apiService.PutAsync<cAccount_DTO>($"{cAppConstants.ApiEndpoints.Accounts}/{id}", account);
  }

  public async Task<cApiResponse<cAccount_DTO>> DeactivateAccountAsync(int id) {
    return await _apiService.PatchAsync<cAccount_DTO>($"{cAppConstants.ApiEndpoints.Accounts}/{id}/deactivate", null);
  }
}
