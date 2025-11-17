using System.Net.Http.Json;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

public class cSubcategoryService {
  private readonly HttpClient _http;

  public cSubcategoryService(HttpClient http) {
    _http = http;
  }

  public async Task<List<cSubcategory>> GetAllAsync() {
    var response = await _http.GetFromJsonAsync<ApiResponse<List<cSubcategory_DTO>>>($"api/categories/subcategories");
    var dtos = response?.Data ?? new List<cSubcategory_DTO>();

    var subcategories = dtos.Select(dto => new cSubcategory {
      Id = dto.Id,
      Name = dto.Name,
      CategoryId = dto.CategoryId
      // Dodaj mapowanie innych właściwości, jeśli są w DTO
    }).ToList();

    return subcategories;
  }

  public async Task<ApiResponse<cSubcategory_DTO>?> GetByIdAsync(int id) {
    // Jeśli chcesz pobierać subkategorię po ID, musisz dodać odpowiedni endpoint w API.
    // Przykład (jeśli endpoint istnieje):
    return await _http.GetFromJsonAsync<ApiResponse<cSubcategory_DTO>>($"api/categories/subcategories/{id}");
  }

  public async Task<ApiResponse<cSubcategory_DTO>> CreateAsync(int categoryId, cSubcategory subcategory) {
    var dto = new cSubcategory_DTO {
      Id = subcategory.Id,
      Name = subcategory.Name,
      CategoryId = subcategory.CategoryId
    };
    var response = await _http.PostAsJsonAsync($"api/categories/{categoryId}/subcategories", dto);
    return await response.Content.ReadFromJsonAsync<ApiResponse<cSubcategory_DTO>>()
           ?? ApiResponse<cSubcategory_DTO>.Error("Brak odpowiedzi z serwera");
  }

  public async Task<ApiResponse<cSubcategory_DTO>> UpdateAsync(int subcategoryId, cSubcategory subcategory) {
    var dto = new cSubcategory_DTO {
      Id = subcategory.Id,
      Name = subcategory.Name,
      CategoryId = subcategory.CategoryId
    };
    var response = await _http.PutAsJsonAsync($"api/categories/subcategories/{subcategoryId}", dto);
    return await response.Content.ReadFromJsonAsync<ApiResponse<cSubcategory_DTO>>()
           ?? ApiResponse<cSubcategory_DTO>.Error("Brak odpowiedzi z serwera");
  }

  public async Task<ApiResponse> DeleteAsync(int subcategoryId) {
    var response = await _http.DeleteAsync($"api/categories/subcategories/{subcategoryId}");
    return await response.Content.ReadFromJsonAsync<ApiResponse>()
           ?? ApiResponse.Error("Brak odpowiedzi z serwera");
  }
}
