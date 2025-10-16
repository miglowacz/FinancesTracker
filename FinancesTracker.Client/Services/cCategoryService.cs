using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;
using System.Net.Http.Json;
using FinancesTracker.Shared.Models;

namespace FinancesTracker.Client.Services;

public class cCategoryService {

  private readonly ApiService mApiService;//serwis do komunikacji z API
  private readonly HttpClient _http;

  public cCategoryService(ApiService xApiService, HttpClient http) {
    //funkcja inicjalizuj¹ca serwis kategorii
    //xApiService - instancja serwisu API
    
    mApiService = xApiService;
    _http = http;
  }

  public async Task<ApiResponse<List<cCategory_DTO>>> GetCategoriesAsync() {
    //funkcja pobieraj¹ca listê kategorii
    //brak parametrów

    return await mApiService.GetAsync<List<cCategory_DTO>>(AppConstants.ApiEndpoints.Categories);

  }


  public async Task<ApiResponse<cCategory_DTO>> GetCategoryAsync(int xId) {
    //funkcja pobieraj¹ca pojedyncz¹ kategoriê
    //xId - identyfikator kategorii

    return await mApiService.GetAsync<cCategory_DTO>($"{AppConstants.ApiEndpoints.Categories}/{xId}");

  }


  public async Task<ApiResponse<List<cSubcategory_DTO>>> GetSubcategoriesAsync(int xCategoryId) {
    //funkcja pobieraj¹ca listê podkategorii dla danej kategorii
    //xCategoryId - identyfikator kategorii

    return await mApiService.GetAsync<List<cSubcategory_DTO>>($"{AppConstants.ApiEndpoints.Categories}/{xCategoryId}/subcategories");

  }

  public async Task<List<cCategory>> GetAllAsync()
    => await _http.GetFromJsonAsync<List<cCategory>>("api/category");

  public async Task<cCategory?> GetByIdAsync(int id)
    => await _http.GetFromJsonAsync<cCategory>($"api/category/{id}");

  public async Task AddAsync(cCategory category)
    => await _http.PostAsJsonAsync("api/category", category);

  public async Task UpdateAsync(cCategory category)
    => await _http.PutAsJsonAsync($"api/category/{category.Id}", category);

  public async Task DeleteAsync(int id)
    => await _http.DeleteAsync($"api/category/{id}");
}