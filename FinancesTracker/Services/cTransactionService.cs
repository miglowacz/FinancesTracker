using FinancesTracker.Client.Services;
using FinancesTracker.Data;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Services;

public class cTransactionService : ITransactionService {
  private readonly FinancesTrackerDbContext _context;
  private readonly cCategoryRuleService _ruleService;
  private readonly cAccountRuleService _accountRuleService;

  public cTransactionService(FinancesTrackerDbContext context) {
    _context = context;
    _ruleService = new cCategoryRuleService(context);
    _accountRuleService = new cAccountRuleService(context);
  }

  public async Task<cPagedResult<cTransaction_DTO>> GetTransactionsAsync(cTransactionFilter_DTO xFilter) {
    var pQuery = _context.Transactions
        .AsNoTracking()
        .Include(t => t.Account)
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .Include(t => t.RelatedTransaction).ThenInclude(rt => rt.Account)
        .AsQueryable();

    // Filtrowanie
    if (xFilter.Year.HasValue) pQuery = pQuery.Where(t => t.Year == xFilter.Year.Value);
    if (xFilter.Month.HasValue) pQuery = pQuery.Where(t => t.MonthNumber == xFilter.Month.Value);
    if (xFilter.HideTransfers) pQuery = pQuery.Where(t => !t.IsTransfer);
    if (xFilter.HasNoCategory) pQuery = pQuery.Where(t => t.CategoryId == null);
    if (xFilter.CategoryId.HasValue) pQuery = pQuery.Where(t => t.CategoryId == xFilter.CategoryId.Value);
    if (xFilter.SubcategoryId.HasValue) pQuery = pQuery.Where(t => t.SubcategoryId == xFilter.SubcategoryId.Value);
    if (xFilter.MinAmount.HasValue) pQuery = pQuery.Where(t => t.Amount >= xFilter.MinAmount.Value);
    if (xFilter.MaxAmount.HasValue) pQuery = pQuery.Where(t => t.Amount <= xFilter.MaxAmount.Value);

    if (xFilter.StartDate.HasValue) pQuery = pQuery.Where(t => t.Date >= xFilter.StartDate.Value.ToUniversalTime());
    if (xFilter.EndDate.HasValue) pQuery = pQuery.Where(t => t.Date <= xFilter.EndDate.Value.ToUniversalTime());
    if (!string.IsNullOrEmpty(xFilter.SearchTerm)) pQuery = pQuery.Where(t => t.Description.Contains(xFilter.SearchTerm));

    if (xFilter.IsInsignificant.HasValue)
      pQuery = pQuery.Where(t => t.IsInsignificant == xFilter.IsInsignificant.Value);
    else if (!xFilter.IncludeInsignificant)
      pQuery = pQuery.Where(t => !t.IsInsignificant);

    // Sortowanie
    pQuery = xFilter.SortBy?.ToLower() switch {
      "description" => xFilter.SortDescending ? pQuery.OrderByDescending(t => t.Description) : pQuery.OrderBy(t => t.Description),
      "amount" => xFilter.SortDescending ? pQuery.OrderByDescending(t => t.Amount) : pQuery.OrderBy(t => t.Amount),
      "category" => xFilter.SortDescending ? pQuery.OrderByDescending(t => t.Category.Name) : pQuery.OrderBy(t => t.Category.Name),
      _ => xFilter.SortDescending ? pQuery.OrderByDescending(t => t.Date) : pQuery.OrderBy(t => t.Date)
    };

    int pTotalCount = await pQuery.CountAsync();
    var pTransactions = await pQuery.Skip((xFilter.PageNumber - 1) * xFilter.PageSize).Take(xFilter.PageSize).ToListAsync();

    return new cPagedResult<cTransaction_DTO> {
      Items = pTransactions.Select(MappingService.ToDto).ToList(),
      TotalCount = pTotalCount,
      PageNumber = xFilter.PageNumber,
      PageSize = xFilter.PageSize
    };
  }

  public async Task<cTransaction_DTO?> GetTransactionByIdAsync(int xId) {
    var pTransaction = await _context.Transactions
        .AsNoTracking()
        .Include(t => t.Account).Include(t => t.Category).Include(t => t.Subcategory)
        .FirstOrDefaultAsync(t => t.Id == xId);
    return pTransaction != null ? MappingService.ToDto(pTransaction) : null;
  }

  public async Task<cTransaction_DTO> CreateTransactionAsync(cTransaction_DTO xDto) {
    var entity = MappingService.ToEntity(xDto);

    // Automatyczna kategoryzacja przy ręcznym dodawaniu
    var (catId, subCatId) = await _ruleService.MatchCategoryAsync(entity.Description);
    if (entity.CategoryId == null) {
      entity.CategoryId = catId;
      entity.SubcategoryId = subCatId;
    }

    _context.Transactions.Add(entity);
    await _context.SaveChangesAsync();

    return await GetTransactionByIdAsync(entity.Id) ?? MappingService.ToDto(entity);
  }

  public async Task<cTransaction_DTO> CreateTransferAsync(cTransaction_DTO xDto) {
    if (xDto.AccountId == xDto.TargetAccountId)
      throw new ArgumentException("Konto źródłowe i docelowe muszą być różne.");

    using var transaction = await _context.Database.BeginTransactionAsync();

    var pSource = MappingService.ToEntity(xDto);
    pSource.Amount = -Math.Abs(pSource.Amount);
    pSource.IsTransfer = true;

    var pTarget = new cTransaction {
      Date = pSource.Date,
      Description = pSource.Description,
      Amount = Math.Abs(pSource.Amount),
      AccountId = xDto.TargetAccountId!.Value,
      IsTransfer = true,
      MonthNumber = pSource.MonthNumber,
      Year = pSource.Year,
      CreatedAt = DateTime.UtcNow
    };

    _context.Transactions.AddRange(pSource, pTarget);
    await _context.SaveChangesAsync();

    pSource.RelatedTransactionId = pTarget.Id;
    pTarget.RelatedTransactionId = pSource.Id;

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();

    return MappingService.ToDto(pSource);
  }

  public async Task<cTransaction_DTO> UpdateTransactionAsync(int id, cTransaction_DTO xDto) {
    var existing = await _context.Transactions.FindAsync(id);
    if (existing == null) return null;

    existing.Date = xDto.Date;
    existing.Description = xDto.Description;
    existing.Amount = xDto.Amount;
    existing.AccountId = xDto.AccountId;
    existing.CategoryId = xDto.CategoryId;
    existing.SubcategoryId = xDto.SubcategoryId;
    existing.IsInsignificant = xDto.IsInsignificant;
    existing.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return MappingService.ToDto(existing);
  }

  public async Task<bool> DeleteTransactionAsync(int xId) {
    var pTransaction = await _context.Transactions.FindAsync(xId);
    if (pTransaction == null) return false;
    _context.Transactions.Remove(pTransaction);
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<cTransaction_DTO?> ToggleInsignificantAsync(int id) {
    var pTransaction = await _context.Transactions.FindAsync(id);
    if (pTransaction == null) return null;

    pTransaction.IsInsignificant = !pTransaction.IsInsignificant;
    pTransaction.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return MappingService.ToDto(pTransaction);
  }

  public async Task<cSummary_DTO> GetSummaryAsync(int xYear, int? xMonth, bool xIncludeInsignificant) {
    var pQuery = _context.Transactions.Include(t => t.Category).Where(t => t.Year == xYear);
    if (xMonth.HasValue) pQuery = pQuery.Where(t => t.MonthNumber == xMonth.Value);
    if (!xIncludeInsignificant) pQuery = pQuery.Where(t => !t.IsInsignificant);

    var pSummary = await pQuery
        .GroupBy(t => new { t.CategoryId, CategoryName = t.Category != null ? t.Category.Name : "Brak kategorii" })
        .Select(g => new CategorySummaryDTO {
          CategoryId = g.Key.CategoryId,
          CategoryName = g.Key.CategoryName,
          TotalAmount = g.Sum(t => t.Amount),
          Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
          Expenses = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
        }).ToListAsync();

    return new cSummary_DTO {
      TotalIncome = pSummary.Sum(s => s.Income),
      TotalExpenses = Math.Abs(pSummary.Sum(s => s.Expenses)),
      Balance = pSummary.Sum(s => s.Income) + pSummary.Sum(s => s.Expenses),
      Categories = pSummary
    };
  }

  public async Task<(int ImportedCount, int InsignificantCount, List<string> Errors)> ImportTransactionsAsync(List<cTransaction_DTO> transactionsCln) {
    var ruleService = new cCategoryRuleService(_context);
    var accountRuleService = new cAccountRuleService(_context);
    var insignificantDetector = new cInsignificantTransactionDetector();

    var errorsCln = new List<string>();
    int importedCount = 0;
    int insignificantCount = 0;
    var addedTransactions = new List<cTransaction>();

    var myAccountIdentifiers = await _context.Accounts.Where(a => !string.IsNullOrEmpty(a.ImportIdentifier))
        .Select(a => a.ImportIdentifier).ToListAsync();

    using var dbTransaction = await _context.Database.BeginTransactionAsync();
    try {
      foreach (var dto in transactionsCln) {
        dto.Date = dto.Date.ToUniversalTime();

        // Obsługa konta
        if (dto.AccountId <= 0) {
          var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Name == dto.AccountName);
          if (account != null) dto.AccountId = account.Id;
          else {
            var matchedId = await accountRuleService.MatchAccountAsync(dto.AccountName);
            if (matchedId.HasValue) dto.AccountId = matchedId.Value;
            else if (!string.IsNullOrEmpty(dto.AccountName)) {
              var pNewAcc = new cAccount { Name = dto.AccountName, BankName = "Import Automatyczny", IsActive = true, Currency = "PLN" };
              _context.Accounts.Add(pNewAcc);
              await _context.SaveChangesAsync();
              dto.AccountId = pNewAcc.Id;
              errorsCln.Add($"Utworzono konto: {pNewAcc.Name}");
            }
          }
        }

        if (dto.AccountId <= 0) continue;

        // Duplikaty
        bool exists = await _context.Transactions.AnyAsync(t => t.Description == dto.Description && t.Date == dto.Date && t.Amount == dto.Amount && t.AccountId == dto.AccountId);
        if (exists) continue;

        // Logika transferu
        bool isTransfer = false;
        string pDesc = dto.Description.ToLower();
        if (myAccountIdentifiers.Any(id => pDesc.Contains(id.ToLower()))) isTransfer = true;

        // Insignificant
        bool isInsignificant = !isTransfer && insignificantDetector.IsInsignificant(dto, myAccountIdentifiers);
        if (isInsignificant) insignificantCount++;

        var entity = MappingService.ToEntity(dto);
        entity.IsTransfer = isTransfer;
        entity.IsInsignificant = isInsignificant;

        var (catId, subCatId) = await ruleService.MatchCategoryAsync(entity.Description);
        entity.CategoryId = catId;
        entity.SubcategoryId = subCatId;

        _context.Transactions.Add(entity);
        addedTransactions.Add(entity);
        importedCount++;
      }

      await _context.SaveChangesAsync();

      // Parowanie transferów
      foreach (var trans in addedTransactions.Where(t => t.IsTransfer)) {
        var dbMatch = await _context.Transactions.FirstOrDefaultAsync(t =>
            t.IsTransfer && t.RelatedTransactionId == null && t.Amount == -trans.Amount &&
            t.AccountId != trans.AccountId && Math.Abs((t.Date - trans.Date).TotalDays) <= 3);

        if (dbMatch != null) {
          trans.RelatedTransactionId = dbMatch.Id;
          dbMatch.RelatedTransactionId = trans.Id;
        }
      }

      await _context.SaveChangesAsync();
      await dbTransaction.CommitAsync();
    } catch (Exception ex) {
      await dbTransaction.RollbackAsync();
      throw;
    }

    return (importedCount, insignificantCount, errorsCln);
  }
}
