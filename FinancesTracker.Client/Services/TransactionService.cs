using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Client.Services;

public class TransactionService
{
    private readonly ApiService _apiService;

    public TransactionService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<PagedResult<TransactionDto>>> GetTransactionsAsync(TransactionFilterDto filter)
    {
        var queryParams = BuildQueryString(filter);
        return await _apiService.GetAsync<PagedResult<TransactionDto>>($"{AppConstants.ApiEndpoints.Transactions}?{queryParams}");
    }

    public async Task<ApiResponse<TransactionDto>> GetTransactionAsync(int id)
    {
        return await _apiService.GetAsync<TransactionDto>($"{AppConstants.ApiEndpoints.Transactions}/{id}");
    }

    public async Task<ApiResponse<TransactionDto>> CreateTransactionAsync(TransactionDto transaction)
    {
        return await _apiService.PostAsync<TransactionDto>(AppConstants.ApiEndpoints.Transactions, transaction);
    }

    public async Task<ApiResponse<TransactionDto>> UpdateTransactionAsync(int id, TransactionDto transaction)
    {
        return await _apiService.PutAsync<TransactionDto>($"{AppConstants.ApiEndpoints.Transactions}/{id}", transaction);
    }

    public async Task<ApiResponse> DeleteTransactionAsync(int id)
    {
        return await _apiService.DeleteAsync($"{AppConstants.ApiEndpoints.Transactions}/{id}");
    }

    public async Task<ApiResponse<object>> GetSummaryAsync(int year, int? month = null)
    {
        var endpoint = $"{AppConstants.ApiEndpoints.Transactions}/summary?year={year}";
        if (month.HasValue)
            endpoint += $"&month={month.Value}";
        
        return await _apiService.GetAsync<object>(endpoint);
    }

    private static string BuildQueryString(TransactionFilterDto filter)
    {
        var queryParams = new List<string>();

        if (filter.Year.HasValue)
            queryParams.Add($"year={filter.Year.Value}");
        
        if (filter.Month.HasValue)
            queryParams.Add($"month={filter.Month.Value}");
        
        if (filter.CategoryId.HasValue)
            queryParams.Add($"categoryId={filter.CategoryId.Value}");
        
        if (filter.SubcategoryId.HasValue)
            queryParams.Add($"subcategoryId={filter.SubcategoryId.Value}");
        
        if (filter.MinAmount.HasValue)
            queryParams.Add($"minAmount={filter.MinAmount.Value}");
        
        if (filter.MaxAmount.HasValue)
            queryParams.Add($"maxAmount={filter.MaxAmount.Value}");
        
        if (filter.StartDate.HasValue)
            queryParams.Add($"startDate={filter.StartDate.Value:yyyy-MM-dd}");
        
        if (filter.EndDate.HasValue)
            queryParams.Add($"endDate={filter.EndDate.Value:yyyy-MM-dd}");
        
        if (!string.IsNullOrEmpty(filter.SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");
        
        if (!string.IsNullOrEmpty(filter.BankName))
            queryParams.Add($"bankName={Uri.EscapeDataString(filter.BankName)}");
        
        queryParams.Add($"pageNumber={filter.PageNumber}");
        queryParams.Add($"pageSize={filter.PageSize}");
        queryParams.Add($"sortBy={filter.SortBy}");
        queryParams.Add($"sortDescending={filter.SortDescending}");

        return string.Join("&", queryParams);
    }
}