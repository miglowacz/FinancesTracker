using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Client.Services;

public class CategoryRuleService
{
    private readonly ApiService _apiService;

    public CategoryRuleService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<CategoryRuleDto>>> GetCategoryRulesAsync()
    {
        return await _apiService.GetAsync<List<CategoryRuleDto>>(AppConstants.ApiEndpoints.CategoryRules);
    }

    public async Task<ApiResponse<CategoryRuleDto>> GetCategoryRuleAsync(int id)
    {
        return await _apiService.GetAsync<CategoryRuleDto>($"{AppConstants.ApiEndpoints.CategoryRules}/{id}");
    }

    public async Task<ApiResponse<CategoryRuleDto>> CreateCategoryRuleAsync(CategoryRuleDto rule)
    {
        return await _apiService.PostAsync<CategoryRuleDto>(AppConstants.ApiEndpoints.CategoryRules, rule);
    }

    public async Task<ApiResponse<CategoryRuleDto>> UpdateCategoryRuleAsync(int id, CategoryRuleDto rule)
    {
        return await _apiService.PutAsync<CategoryRuleDto>($"{AppConstants.ApiEndpoints.CategoryRules}/{id}", rule);
    }

    public async Task<ApiResponse> DeleteCategoryRuleAsync(int id)
    {
        return await _apiService.DeleteAsync($"{AppConstants.ApiEndpoints.CategoryRules}/{id}");
    }

    public async Task<ApiResponse<object>> CategorizeTransactionAsync(string description)
    {
        return await _apiService.PostAsync<object>($"{AppConstants.ApiEndpoints.CategoryRules}/categorize", description);
    }
}