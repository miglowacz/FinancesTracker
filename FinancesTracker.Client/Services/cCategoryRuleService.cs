using System.Net.Http.Json;
using FinancesTracker.Shared.Models;

public class cCategoryRuleService {
  private readonly HttpClient _http;

  public cCategoryRuleService(HttpClient http) {
    _http = http;
  }

  public async Task<List<cCategoryRule>> GetAllAsync()
      => await _http.GetFromJsonAsync<List<cCategoryRule>>("api/categoryrule");

  public async Task<cCategoryRule?> GetByIdAsync(int id)
      => await _http.GetFromJsonAsync<cCategoryRule>($"api/categoryrule/{id}");

  public async Task AddAsync(cCategoryRule rule)
      => await _http.PostAsJsonAsync("api/categoryrule", rule);

  public async Task UpdateAsync(cCategoryRule rule)
      => await _http.PutAsJsonAsync($"api/categoryrule/{rule.Id}", rule);

  public async Task DeleteAsync(int id)
      => await _http.DeleteAsync($"api/categoryrule/{id}");
}