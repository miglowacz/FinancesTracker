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

  public async Task ImportFromCsvAsync(
    Stream csvStream,
    cSubcategoryService subcategoryService,
    cCategoryRuleService ruleService)
  {


    using (var reader = new StreamReader(csvStream)) {
      int lineNo = 0;

      // Pobierz aktualne kategorie i podkategorie do s³owników
      var categories = await GetAllAsync();
      var subcategories = await subcategoryService.GetAllAsync();
      var categoriesDict = categories.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);
      // U¿yj stringa jako klucza: "nazwa_kategorii|nazwa_podkategorii"
      var subcategoriesDict = subcategories.ToDictionary(
          s => $"{s.Name}|{s.CategoryId}", s => s.Id, StringComparer.OrdinalIgnoreCase);
      string? line;
      while ((line = await reader.ReadLineAsync()) != null){
         
        lineNo++;
        if (lineNo == 1) continue; // pomiñ nag³ówek
        if (string.IsNullOrWhiteSpace(line)) continue;

        var parts = line.Split(',');
        if (parts.Length < 3) continue;

        var keyword = parts[0].Trim();
        var categoryName = parts[1].Trim();
        var subcategoryName = parts[2].Trim();

        // Kategoria
        int categoryId;
        if (!categoriesDict.TryGetValue(categoryName, out categoryId))
        {
          var newCat = new cCategory { Name = categoryName };
          var createdCatResp = await CreateCategoryAsync(newCat);
          if (createdCatResp?.Data == null)
            continue; // lub loguj b³¹d

          categoryId = createdCatResp.Data.Id;
          categoriesDict[categoryName] = categoryId;
        }

        // Podkategoria
        int subcategoryId;
        var subKey = $"{subcategoryName}|{categoryId}";
        if (!subcategoriesDict.TryGetValue(subKey, out subcategoryId))
        {
          var newSub = new cSubcategory { Name = subcategoryName, CategoryId = categoryId };
          var createdSub = await subcategoryService.CreateAsync(categoryId, newSub);
          // Za³ó¿, ¿e CreateAsync zwraca cSubcategory lub cSubcategory_DTO z Id
          subcategoryId = createdSub?.Data.Id ?? 0;
          if (subcategoryId == 0)
            continue; // lub loguj b³¹d

          subcategoriesDict[subKey] = subcategoryId;
        }

        // Regu³a
        var newRule = new cCategoryRule
        {
          Keyword = keyword,
          CategoryId = categoryId,
          SubcategoryId = subcategoryId,
          IsActive = true
        };
        await ruleService.AddAsync(newRule);
      }
    }
      
  }
}