using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using System.Net.Http.Json;

namespace FinancesTracker.Client.Services;

public class cAccountRuleService {
  private readonly HttpClient _http;

  public cAccountRuleService(HttpClient http) {
    _http = http;
  }

  public async Task<List<cAccountRule_DTO>> GetAllAsync() {
    var response = await _http.GetFromJsonAsync<cApiResponse<List<cAccountRule_DTO>>>("api/accountrules");
    return response?.Data ?? new List<cAccountRule_DTO>();
  }

  public async Task<cAccountRule_DTO?> GetByIdAsync(int id) {
    var response = await _http.GetFromJsonAsync<cApiResponse<cAccountRule_DTO>>($"api/accountrules/{id}");
    return response?.Data;
  }

  public async Task<cApiResponse<cAccountRule_DTO>> AddAsync(cAccountRule_DTO rule) {
    var response = await _http.PostAsJsonAsync("api/accountrules", rule);
    return await response.Content.ReadFromJsonAsync<cApiResponse<cAccountRule_DTO>>()
           ?? cApiResponse<cAccountRule_DTO>.Error("Brak odpowiedzi z serwera");
  }

  public async Task<cApiResponse<cAccountRule_DTO>> UpdateAsync(cAccountRule_DTO rule) {
    var response = await _http.PutAsJsonAsync($"api/accountrules/{rule.Id}", rule);
    return await response.Content.ReadFromJsonAsync<cApiResponse<cAccountRule_DTO>>()
           ?? cApiResponse<cAccountRule_DTO>.Error("Brak odpowiedzi z serwera");
  }

  public async Task<cApiResponse> DeleteAsync(int id) {
    var response = await _http.DeleteAsync($"api/accountrules/{id}");
    return await response.Content.ReadFromJsonAsync<cApiResponse>()
           ?? cApiResponse.Error("Brak odpowiedzi z serwera");
  }
}
