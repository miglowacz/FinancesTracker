using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Client.Services;

public class cTransactionService {
  private readonly ApiService _apiService;

  public cTransactionService(ApiService apiService) {
    _apiService = apiService;
  }

  public async Task<ApiResponse<PagedResult<cTransaction_DTO>>> GetTransactionsAsync(cTransactionFilter_DTO filter) {
    var queryParams = BuildQueryString(filter);
    return await _apiService.GetAsync<PagedResult<cTransaction_DTO>>($"{AppConstants.ApiEndpoints.Transactions}?{queryParams}");
  }

  public async Task<ApiResponse<cTransaction_DTO>> GetTransactionAsync(int id) {
    return await _apiService.GetAsync<cTransaction_DTO>($"{AppConstants.ApiEndpoints.Transactions}/{id}");
  }

  public async Task<ApiResponse<cTransaction_DTO>> CreateTransactionAsync(cTransaction_DTO transaction) {
    return await _apiService.PostAsync<cTransaction_DTO>(AppConstants.ApiEndpoints.Transactions, transaction);
  }

  public async Task<ApiResponse<cTransaction_DTO>> UpdateTransactionAsync(int id, cTransaction_DTO transaction) {
    return await _apiService.PutAsync<cTransaction_DTO>($"{AppConstants.ApiEndpoints.Transactions}/{id}", transaction);
  }

  public async Task<ApiResponse> DeleteTransactionAsync(int id) {
    return await _apiService.DeleteAsync($"{AppConstants.ApiEndpoints.Transactions}/{id}");
  }

  public async Task<ApiResponse<cTransaction_DTO>> ToggleInsignificantAsync(int id) {
    return await _apiService.PatchAsync<cTransaction_DTO>($"{AppConstants.ApiEndpoints.Transactions}/{id}/toggle-insignificant", null);
  }

  public async Task<ApiResponse<object>> GetSummaryAsync(int year, int? month = null, bool includeInsignificant = false) {
    var endpoint = $"{AppConstants.ApiEndpoints.Transactions}/summary?year={year}&includeInsignificant={includeInsignificant}";
    if (month.HasValue)
      endpoint += $"&month={month.Value}";

    return await _apiService.GetAsync<object>(endpoint);
  }

  public async Task<ApiResponse<SummaryDTO>> GetSummaryAsync(int year, int month) {
    var endpoint = $"{AppConstants.ApiEndpoints.Transactions}/summary?year={year}&month={month}";
    return await _apiService.GetAsync<SummaryDTO>(endpoint);
  }

  private static string BuildQueryString(cTransactionFilter_DTO filter) {
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

    if (filter.IsInsignificant.HasValue)
      queryParams.Add($"isInsignificant={filter.IsInsignificant.Value}");

    queryParams.Add($"includeInsignificant={filter.IncludeInsignificant}");
    queryParams.Add($"pageNumber={filter.PageNumber}");
    queryParams.Add($"pageSize={filter.PageSize}");
    queryParams.Add($"sortBy={filter.SortBy}");
    queryParams.Add($"sortDescending={filter.SortDescending}");

    return string.Join("&", queryParams);
  }
}
