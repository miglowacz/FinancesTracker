using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

namespace FinancesTracker.Services;

public class MappingService
{
    public static TransactionDto ToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            Date = transaction.Date,
            Description = transaction.Description,
            Amount = transaction.Amount,
            CategoryId = transaction.CategoryId,
            SubcategoryId = transaction.SubcategoryId,
            BankName = transaction.BankName,
            CategoryName = transaction.Category?.Name,
            SubcategoryName = transaction.Subcategory?.Name,
            MonthNumber = transaction.MonthNumber,
            Year = transaction.Year,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }

    public static Transaction ToEntity(TransactionDto dto)
    {
        return new Transaction
        {
            Id = dto.Id,
            Date = dto.Date,
            Description = dto.Description,
            Amount = dto.Amount,
            CategoryId = dto.CategoryId,
            SubcategoryId = dto.SubcategoryId,
            BankName = dto.BankName,
            MonthNumber = dto.Date.Month,
            Year = dto.Date.Year,
            CreatedAt = dto.Id == 0 ? DateTime.UtcNow : dto.CreatedAt,
            UpdatedAt = dto.Id != 0 ? DateTime.UtcNow : null
        };
    }

    public static CategoryDto ToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Subcategories = category.Subcategories?.Select(s => new SubcategoryDto
            {
                Id = s.Id,
                Name = s.Name,
                CategoryId = s.CategoryId
            }).ToList() ?? new List<SubcategoryDto>()
        };
    }

    public static CategoryRuleDto ToDto(CategoryRule rule)
    {
        return new CategoryRuleDto
        {
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

    public static CategoryRule ToEntity(CategoryRuleDto dto)
    {
        return new CategoryRule
        {
            Id = dto.Id,
            Keyword = dto.Keyword.ToLowerInvariant(),
            CategoryId = dto.CategoryId,
            SubcategoryId = dto.SubcategoryId,
            IsActive = dto.IsActive,
            CreatedAt = dto.Id == 0 ? DateTime.UtcNow : dto.CreatedAt
        };
    }
}