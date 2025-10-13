using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Client.Services;

public class CategoryService
{
    private readonly ApiService _apiService;

    public CategoryService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync()
    {
        return await _apiService.GetAsync<List<CategoryDto>>(AppConstants.ApiEndpoints.Categories);
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryAsync(int id)
    {
        return await _apiService.GetAsync<CategoryDto>($"{AppConstants.ApiEndpoints.Categories}/{id}");
    }

    public async Task<ApiResponse<List<SubcategoryDto>>> GetSubcategoriesAsync(int categoryId)
    {
        return await _apiService.GetAsync<List<SubcategoryDto>>($"{AppConstants.ApiEndpoints.Categories}/{categoryId}/subcategories");
    }
}