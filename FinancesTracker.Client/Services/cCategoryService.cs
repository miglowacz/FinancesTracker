using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;
using System.Net.Http.Json;
using FinancesTracker.Shared.Models;

namespace FinancesTracker.Client.Services;

public class cCategoryService {
  private readonly ApiService mApiService; // serwis do komunikacji z API
  private readonly HttpClient _http;

  public cCategoryService(ApiService xApiService, HttpClient http) {
    mApiService = xApiService;
    _http = http;
  }

  public async Task<ApiResponse<List<cCategory_DTO>>> GetCategoriesAsync() {
    return await mApiService.GetAsync<List<cCategory_DTO>>(AppConstants.ApiEndpoints.Categories);
  }

  public async Task<ApiResponse<cCategory_DTO>> GetCategoryAsync(int xId) {
    return await mApiService.GetAsync<cCategory_DTO>($"{AppConstants.ApiEndpoints.Categories}/{xId}");
  }

  public async Task<ApiResponse<List<cSubcategory_DTO>>> GetSubcategoriesAsync(int xCategoryId) {
    return await mApiService.GetAsync<List<cSubcategory_DTO>>($"{AppConstants.ApiEndpoints.Categories}/{xCategoryId}/subcategories");
  }

  public async Task<List<cCategory>> GetAllAsync() {
    var pResponse = await _http.GetFromJsonAsync<ApiResponse<List<cCategory_DTO>>>("api/categories");
    var pCln_DTOs = pResponse?.Data ?? new List<cCategory_DTO>();

    // Konwersja DTO -> Model
    var pCln = pCln_DTOs.Select(dto => new cCategory {
      Id = dto.Id,
      Name = dto.Name,
      Subcategories = dto.Subcategories?.Select(subDto => new cSubcategory {
        Id = subDto.Id,
        Name = subDto.Name,
        CategoryId = subDto.CategoryId
      }).ToList() ?? new List<cSubcategory>()
    }).ToList();

    return pCln;
  }

  public async Task<cCategory?> GetByIdAsync(int id) {
  
    var response = await _http.GetFromJsonAsync<ApiResponse<cCategory_DTO>>($"api/categories/{id}");
    var dto = response?.Data;
    if (dto == null) return null;

    return new cCategory {
      Id = dto.Id,
      Name = dto.Name,
      Subcategories = dto.Subcategories?.Select(subDto => new cSubcategory {
        Id = subDto.Id,
        Name = subDto.Name,
        CategoryId = subDto.CategoryId
      }).ToList() ?? new List<cSubcategory>()
    };
  
  }


  public async Task<ApiResponse<cCategory_DTO>> CreateCategoryAsync(cCategory category) {
    var dto = new cCategory_DTO {
      Id = category.Id,
      Name = category.Name,
      Subcategories = category.Subcategories?.Select(s => new cSubcategory_DTO {
        Id = s.Id,
        Name = s.Name,
        CategoryId = s.CategoryId
      }).ToList() ?? new List<cSubcategory_DTO>()
    };

    var response = await _http.PostAsJsonAsync("api/categories", dto);
    return await response.Content.ReadFromJsonAsync<ApiResponse<cCategory_DTO>>()
           ?? ApiResponse<cCategory_DTO>.ErrorResult("Brak odpowiedzi z serwera");
  }

  public async Task<ApiResponse<cCategory_DTO>> UpdateCategoryAsync(int id, cCategory category) {
    var dto = new cCategory_DTO {
      Id = category.Id,
      Name = category.Name,
      Subcategories = category.Subcategories?.Select(s => new cSubcategory_DTO {
        Id = s.Id,
        Name = s.Name,
        CategoryId = s.CategoryId
      }).ToList() ?? new List<cSubcategory_DTO>()
    };

    var response = await _http.PutAsJsonAsync($"api/categories/{id}", dto);
    return await response.Content.ReadFromJsonAsync<ApiResponse<cCategory_DTO>>()
           ?? ApiResponse<cCategory_DTO>.ErrorResult("Brak odpowiedzi z serwera");
  }

  public async Task<ApiResponse> DeleteCategoryAsync(int id) {
    var response = await _http.DeleteAsync($"api/categories/{id}");
    return await response.Content.ReadFromJsonAsync<ApiResponse>()
           ?? ApiResponse.Error("Brak odpowiedzi z serwera");
  }


}