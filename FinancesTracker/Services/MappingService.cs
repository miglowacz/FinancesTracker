using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

namespace FinancesTracker.Services;

public class MappingService {
  public static cTransaction_DTO ToDto(cTransaction transaction) {
    return new cTransaction_DTO {
      Id = transaction.Id,
      Date = transaction.Date,
      Description = transaction.Description,
      Amount = transaction.Amount,
      AccountId = transaction.AccountId ?? 0,
      CategoryId = transaction?.CategoryId,
      SubcategoryId = transaction?.SubcategoryId,
      //BankName = transaction.BankName,
      AccountName = transaction?.AccountName ?? transaction?.Account?.Name,
      IsInsignificant = transaction.IsInsignificant,
      
      IsTransfer = transaction.IsTransfer,
      RelatedTransactionId = transaction.RelatedTransactionId,
      RelatedAccountName = transaction.RelatedTransaction?.Account?.Name,
      
      CategoryName = transaction.Category?.Name,
      SubcategoryName = transaction.Subcategory?.Name,
      MonthNumber = transaction.MonthNumber,
      Year = transaction.Year,
      CreatedAt = transaction.CreatedAt,
      UpdatedAt = transaction.UpdatedAt
    };
  }

  public static cTransaction ToEntity(cTransaction_DTO dto) {
    return new cTransaction {
      Id = dto.Id,
      Date = dto.Date,
      Description = dto.Description,
      Amount = dto.Amount,
      AccountId = dto.AccountId,
      CategoryId = dto?.CategoryId,
      SubcategoryId = dto?.SubcategoryId,
      //BankName = dto.BankName,
      //AccountName = dto.AccountName,
      IsInsignificant = dto.IsInsignificant,
      IsTransfer = dto.IsTransfer,
      RelatedTransactionId = dto.RelatedTransactionId,
      MonthNumber = dto.Date.Month,
      Year = dto.Date.Year,
      CreatedAt = dto.Id == 0 ? DateTime.UtcNow : dto.CreatedAt,
      UpdatedAt = dto.Id != 0 ? DateTime.UtcNow : null
    };
  }

  public static cCategory_DTO ToDto(cCategory category) {
    return new cCategory_DTO {
      Id = category.Id,
      Name = category.Name,
      Subcategories = category.Subcategories?.Select(s => new cSubcategory_DTO {
        Id = s.Id,
        Name = s.Name,
        CategoryId = s.CategoryId
      }).ToList() ?? new List<cSubcategory_DTO>()
    };
  }

  public static cCategoryRule_DTO ToDto(cCategoryRule rule) {
    return new cCategoryRule_DTO {
      Id = rule.Id,
      Keyword = rule.Keyword,
      CategoryId = rule.CategoryId,
      SubcategoryId = rule.SubcategoryId,
      IsActive = rule.IsActive,
      CategoryName = rule.Category?.Name,
      SubcategoryName = rule.Subcategory?.Name,
      CreatedAt = rule.CreatedAt
    };
  }

  public static cCategoryRule ToEntity(cCategoryRule_DTO dto) {
    return new cCategoryRule {
      Id = dto.Id,
      Keyword = dto.Keyword.ToLowerInvariant(),
      CategoryId = dto.CategoryId,
      SubcategoryId = dto.SubcategoryId,
      IsActive = dto.IsActive,
      CreatedAt = dto.Id == 0 ? DateTime.UtcNow : dto.CreatedAt
    };
  }
}
