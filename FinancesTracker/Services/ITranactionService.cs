using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

namespace FinancesTracker.Services;

public interface ITransactionService {
  Task<cPagedResult<cTransaction_DTO>> GetTransactionsAsync(cTransactionFilter_DTO xFilter);
  Task<cTransaction_DTO?> GetTransactionByIdAsync(int xId);
  Task<cTransaction_DTO> CreateTransactionAsync(cTransaction_DTO xDto);
  Task<cTransaction_DTO> CreateTransferAsync(cTransaction_DTO xDto);
  Task<cTransaction_DTO> UpdateTransactionAsync(int id, cTransaction_DTO xDto);
  Task<bool> DeleteTransactionAsync(int xId);
  Task<cTransaction_DTO?> ToggleInsignificantAsync(int id);
  Task<cSummary_DTO> GetSummaryAsync(int xYear, int? xMonth, bool xIncludeInsignificant);
  Task<(int ImportedCount, int InsignificantCount, List<string> Errors)> ImportTransactionsAsync(List<cTransaction_DTO> transactionsCln);
}
