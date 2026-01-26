using FinancesTracker.Shared.Constants;
using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Client.Services;

public class cTransactionService {
  private readonly cApiService _apiService;

  public cTransactionService(cApiService apiService) {
    _apiService = apiService;
  }

  public async Task<cApiResponse<cPagedResult<cTransaction_DTO>>> GetTransactionsAsync(cTransactionFilter_DTO filter) {
    var queryParams = BuildQueryString(filter);
    return await _apiService.GetAsync<cPagedResult<cTransaction_DTO>>($"{cAppConstants.ApiEndpoints.Transactions}?{queryParams}");
  }

  public async Task<cApiResponse<cTransaction_DTO>> GetTransactionAsync(int id) {
    return await _apiService.GetAsync<cTransaction_DTO>($"{cAppConstants.ApiEndpoints.Transactions}/{id}");
  }

  public async Task<cApiResponse<cTransaction_DTO>> CreateTransactionAsync(cTransaction_DTO transaction) {
    return await _apiService.PostAsync<cTransaction_DTO>(cAppConstants.ApiEndpoints.Transactions, transaction);
  }

  public async Task<cApiResponse<cTransaction_DTO>> CreateTransferAsync(cTransaction_DTO transfer) {
    // Upewniamy się, że to transfer
    transfer.IsTransfer = true;
    
    if (!transfer.TargetAccountId.HasValue)
      throw new InvalidOperationException("Transfer musi mieć ustawione konto docelowe");

    if (transfer.TargetAccountId.Value == transfer.AccountId)
      throw new InvalidOperationException("Konto źródłowe i docelowe muszą być różne");

    return await _apiService.PostAsync<cTransaction_DTO>(cAppConstants.ApiEndpoints.Transactions, transfer);
  }

  public async Task<cApiResponse<cTransaction_DTO>> UpdateTransactionAsync(int id, cTransaction_DTO transaction) {
    return await _apiService.PutAsync<cTransaction_DTO>($"{cAppConstants.ApiEndpoints.Transactions}/{id}", transaction);
  }

  public async Task<cApiResponse> DeleteTransactionAsync(int id) {
    return await _apiService.DeleteAsync($"{cAppConstants.ApiEndpoints.Transactions}/{id}");
  }

  public async Task<cApiResponse<cTransaction_DTO>> ToggleInsignificantAsync(int id) {
    return await _apiService.PatchAsync<cTransaction_DTO>($"{cAppConstants.ApiEndpoints.Transactions}/{id}/toggle-insignificant", null);
  }

  public async Task<cApiResponse<cSummary_DTO>> GetSummaryAsync(int year, int month) {
    var endpoint = $"{cAppConstants.ApiEndpoints.Transactions}/summary?year={year}&month={month}";
    return await _apiService.GetAsync<cSummary_DTO>(endpoint);
  }

  public async Task<cApiResponse<cSummary_DTO>> GetSummaryAsync(int year, int? month = null, bool includeInsignificant = false) {
    var endpoint = $"{cAppConstants.ApiEndpoints.Transactions}/summary?year={year}&includeInsignificant={includeInsignificant}";
    if (month.HasValue)
      endpoint += $"&month={month.Value}";

    return await _apiService.GetAsync<cSummary_DTO>(endpoint);
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

    if (filter.HideTransfers)
        queryParams.Add($"hideTransfers={filter.HideTransfers}");

    queryParams.Add($"includeInsignificant={filter.IncludeInsignificant}");
    queryParams.Add($"pageNumber={filter.PageNumber}");
    queryParams.Add($"pageSize={filter.PageSize}");
    queryParams.Add($"sortBy={filter.SortBy}");
    queryParams.Add($"sortDescending={filter.SortDescending}");

    return string.Join("&", queryParams);
  }
}
