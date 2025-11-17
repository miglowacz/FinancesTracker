using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using System.Net.Http.Json;

public class cCategoryRuleService {
  private readonly HttpClient _http;

  public cCategoryRuleService(HttpClient http) {
    _http = http;
  }

  public async Task<List<cCategoryRule>> GetAllAsync() {
    var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryRuleDto>>>("api/categoryrules");
    var dtos = response?.Data ?? new List<CategoryRuleDto>();
    return dtos.Select(dto => new cCategoryRule {
      Id = dto.Id,
      Keyword = dto.Keyword,
      CategoryId = dto.CategoryId,
      SubcategoryId = dto.SubcategoryId,
      IsActive = dto.IsActive
    }).ToList();
  }

  public async Task<cCategoryRule?> GetByIdAsync(int id) {
    var response = await _http.GetFromJsonAsync<ApiResponse<CategoryRuleDto>>($"api/categoryrules/{id}");
    var dto = response?.Data;
    if (dto == null) return null;
    return new cCategoryRule {
      Id = dto.Id,
      Keyword = dto.Keyword,
      CategoryId = dto.CategoryId,
      SubcategoryId = dto.SubcategoryId,
      IsActive = dto.IsActive
    };
  }

  public async Task<ApiResponse<CategoryRuleDto>> AddAsync(cCategoryRule rule) {
    var dto = new CategoryRuleDto {
      Id = rule.Id,
      Keyword = rule.Keyword,
      CategoryId = rule.CategoryId,
      SubcategoryId = rule.SubcategoryId,
      IsActive = rule.IsActive
    };
    var response = await _http.PostAsJsonAsync("api/categoryrules", dto);
    return await response.Content.ReadFromJsonAsync<ApiResponse<CategoryRuleDto>>()
           ?? ApiResponse<CategoryRuleDto>.Error("Brak odpowiedzi z serwera");
  }

  public async Task<ApiResponse<CategoryRuleDto>> UpdateAsync(cCategoryRule rule) {
    var dto = new CategoryRuleDto {
      Id = rule.Id,
      Keyword = rule.Keyword,
      CategoryId = rule.CategoryId,
      SubcategoryId = rule.SubcategoryId,
      IsActive = rule.IsActive
    };
    var response = await _http.PutAsJsonAsync($"api/categoryrules/{rule.Id}", dto);
    return await response.Content.ReadFromJsonAsync<ApiResponse<CategoryRuleDto>>()
           ?? ApiResponse<CategoryRuleDto>.Error("Brak odpowiedzi z serwera");
  }

  public async Task<ApiResponse> DeleteAsync(int id) {
    var response = await _http.DeleteAsync($"api/categoryrules/{id}");
    return await response.Content.ReadFromJsonAsync<ApiResponse>()
           ?? ApiResponse.Error("Brak odpowiedzi z serwera");
  }
}
