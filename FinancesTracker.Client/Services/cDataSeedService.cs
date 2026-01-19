using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Client.Services;

public class cDataSeedService {
  private readonly cApiService _apiService;

  public cDataSeedService(cApiService apiService) {
    _apiService = apiService;
  }

  public async Task<cApiResponse> GenerateDataAsync(int transactionCount = 200) {
    var response = await _apiService.PostAsync<object>($"api/dataseed/generate?transactionCount={transactionCount}", new { });

    return new cApiResponse {
      Success = response.Success,
      Message = response.Message,
      Errors = response.Errors
    };
  }
}
