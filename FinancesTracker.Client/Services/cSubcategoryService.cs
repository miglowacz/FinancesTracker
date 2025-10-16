using System.Net.Http.Json;
using FinancesTracker.Shared.Models;

public class cSubcategoryService {
  private readonly HttpClient _http;

  public cSubcategoryService(HttpClient http) {
    _http = http;
  }

  public async Task<List<cSubcategory>> GetAllAsync()
      => await _http.GetFromJsonAsync<List<cSubcategory>>("api/subcategory");

  public async Task<cSubcategory?> GetByIdAsync(int id)
      => await _http.GetFromJsonAsync<cSubcategory>($"api/subcategory/{id}");

  public async Task AddAsync(cSubcategory subcategory)
      => await _http.PostAsJsonAsync("api/subcategory", subcategory);

  public async Task UpdateAsync(cSubcategory subcategory)
      => await _http.PutAsJsonAsync($"api/subcategory/{subcategory.Id}", subcategory);

  public async Task DeleteAsync(int id)
      => await _http.DeleteAsync($"api/subcategory/{id}");
}