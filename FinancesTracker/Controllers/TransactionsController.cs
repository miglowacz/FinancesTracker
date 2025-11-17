using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase {
  private readonly FinancesTrackerDbContext mContext;

  public TransactionsController(FinancesTrackerDbContext xContext) {
    //konstruktor kontrolera transakcji
    //xContext - kontekst bazy danych

    mContext = xContext;

  }

  [HttpGet]
  public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetTransactions([FromQuery] TransactionFilterDto xFilter) {
    //funkcja pobiera listę transakcji z filtrowaniem, sortowaniem i paginacją
    //xFilter - obiekt filtrujący transakcje

    var pQuery = mContext.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .AsQueryable();

    //filtrowanie po roku
    if (xFilter.Year.HasValue)
      pQuery = pQuery.Where(t => t.Year == xFilter.Year.Value);

    //filtrowanie po miesiącu
    if (xFilter.Month.HasValue)
      pQuery = pQuery.Where(t => t.MonthNumber == xFilter.Month.Value);

    //filtrowanie po kategorii
    if (xFilter.CategoryId.HasValue)
      pQuery = pQuery.Where(t => t.CategoryId == xFilter.CategoryId.Value);

    //filtrowanie po podkategorii
    if (xFilter.SubcategoryId.HasValue)
      pQuery = pQuery.Where(t => t.SubcategoryId == xFilter.SubcategoryId.Value);

    //filtrowanie po minimalnej kwocie
    if (xFilter.MinAmount.HasValue)
      pQuery = pQuery.Where(t => t.Amount >= xFilter.MinAmount.Value);

    //filtrowanie po maksymalnej kwocie
    if (xFilter.MaxAmount.HasValue)
      pQuery = pQuery.Where(t => t.Amount <= xFilter.MaxAmount.Value);

    if (xFilter.StartDate.HasValue) {
      var s = xFilter.StartDate.Value.ToUniversalTime();
      pQuery = pQuery.Where(t => t.Date >= s);
    }

    if (xFilter.EndDate.HasValue) {
      var e = xFilter.EndDate.Value.ToUniversalTime();
      pQuery = pQuery.Where(t => t.Date <= e);
    }

    //filtrowanie po opisie
    if (!string.IsNullOrEmpty(xFilter.SearchTerm))
      pQuery = pQuery.Where(t => t.Description.Contains(xFilter.SearchTerm));

    //filtrowanie po nazwie banku
    if (!string.IsNullOrEmpty(xFilter.BankName))
      pQuery = pQuery.Where(t => t.BankName == xFilter.BankName);

    //sortowanie według wybranego pola
    pQuery = xFilter.SortBy?.ToLower() switch {
      "description" => xFilter.SortDescending
          ? pQuery.OrderByDescending(t => t.Description)
          : pQuery.OrderBy(t => t.Description),
      "amount" => xFilter.SortDescending
          ? pQuery.OrderByDescending(t => t.Amount)
          : pQuery.OrderBy(t => t.Amount),
      "category" => xFilter.SortDescending
          ? pQuery.OrderByDescending(t => t.Category.Name)
          : pQuery.OrderBy(t => t.Category.Name),
      _ => xFilter.SortDescending
          ? pQuery.OrderByDescending(t => t.Date)
          : pQuery.OrderBy(t => t.Date)
    };

    //pobiera liczbę wszystkich pasujących transakcji
    int pTotalCount = await pQuery.CountAsync();

    //pobiera transakcje z paginacją
    var pTransactions = await pQuery
        .Skip((xFilter.PageNumber - 1) * xFilter.PageSize)
        .Take(xFilter.PageSize)
        .ToListAsync();

    //tworzy wynik z transakcjami i informacjami o paginacji
    var pResult = new PagedResult<TransactionDto> {
      Items = pTransactions.Select(MappingService.ToDto).ToList(),
      TotalCount = pTotalCount,
      PageNumber = xFilter.PageNumber,
      PageSize = xFilter.PageSize
    };

    return Ok(ApiResponse<PagedResult<TransactionDto>>.SuccessResult(pResult));

  }

  [HttpGet("{id}")]
  public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction(int xId) {
    //funkcja pobiera pojedynczą transakcję po id
    //xId - identyfikator transakcji

    var pTransaction = await mContext.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .FirstOrDefaultAsync(t => t.Id == xId);

    if (pTransaction == null)
      return NotFound(ApiResponse<TransactionDto>.Error("Transakcja nie została znaleziona"));

    return Ok(ApiResponse<TransactionDto>.SuccessResult(MappingService.ToDto(pTransaction)));

  }

  [HttpPost]
  public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction(TransactionDto xTransactionDto) {
    //funkcja tworzy nową transakcję
    //xTransactionDto - dane nowej transakcji

    if (!ModelState.IsValid) {
      var pErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(ApiResponse<TransactionDto>.Error("Dane transakcji są nieprawidłowe", pErrors));
    }

    //sprawdza czy kategoria i podkategoria istnieją
    var pCategory = await mContext.Categories.Include(c => c.Subcategories)
        .FirstOrDefaultAsync(c => c.Id == xTransactionDto.CategoryId);

    if (pCategory == null)
      return BadRequest(ApiResponse<TransactionDto>.Error("Wybrana kategoria nie istnieje"));

    if (!pCategory.Subcategories.Any(s => s.Id == xTransactionDto.SubcategoryId))
      return BadRequest(ApiResponse<TransactionDto>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

    var pTransaction = MappingService.ToEntity(xTransactionDto);
    mContext.Transactions.Add(pTransaction);
    await mContext.SaveChangesAsync();

    //pobiera utworzoną transakcję z relacjami
    var pCreatedTransaction = await mContext.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .FirstAsync(t => t.Id == pTransaction.Id);

    return CreatedAtAction(nameof(GetTransaction),
        new { id = pTransaction.Id },
        ApiResponse<TransactionDto>.SuccessResult(MappingService.ToDto(pCreatedTransaction), "Transakcja została utworzona"));

  }

  [HttpPut("{id}")]
  public async Task<ActionResult<ApiResponse<TransactionDto>>> UpdateTransaction(int id, TransactionDto xTransactionDto) {
    //funkcja aktualizuje istniejącą transakcję
    //xId - identyfikator transakcji
    //xTransactionDto - dane do aktualizacji transakcji

    if (id != xTransactionDto.Id)
      return BadRequest(ApiResponse<TransactionDto>.Error("ID transakcji nie pasuje"));

    if (!ModelState.IsValid) {
      var pErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(ApiResponse<TransactionDto>.Error("Dane transakcji są nieprawidłowe", pErrors));
    }

    var pExistingTransaction = await mContext.Transactions.FindAsync(id);
    if (pExistingTransaction == null)
      return NotFound(ApiResponse<TransactionDto>.Error("Transakcja nie została znaleziona"));

    //sprawdza czy kategoria i podkategoria istnieją
    var pCategory = await mContext.Categories.Include(c => c.Subcategories)
        .FirstOrDefaultAsync(c => c.Id == xTransactionDto.CategoryId);

    if (pCategory == null)
      return BadRequest(ApiResponse<TransactionDto>.Error("Wybrana kategoria nie istnieje"));

    if (!pCategory.Subcategories.Any(s => s.Id == xTransactionDto.SubcategoryId))
      return BadRequest(ApiResponse<TransactionDto>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

    //aktualizuje właściwości transakcji
    pExistingTransaction.Date = xTransactionDto.Date;
    pExistingTransaction.Description = xTransactionDto.Description;
    pExistingTransaction.Amount = xTransactionDto.Amount;
    pExistingTransaction.CategoryId = xTransactionDto.CategoryId;
    pExistingTransaction.SubcategoryId = xTransactionDto.SubcategoryId;
    pExistingTransaction.BankName = xTransactionDto.BankName;
    pExistingTransaction.MonthNumber = xTransactionDto.Date.Month;
    pExistingTransaction.Year = xTransactionDto.Date.Year;
    pExistingTransaction.UpdatedAt = DateTime.UtcNow;

    await mContext.SaveChangesAsync();

    //pobiera zaktualizowaną transakcję z relacjami
    var pUpdatedTransaction = await mContext.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .FirstAsync(t => t.Id == id);

    return Ok(ApiResponse<TransactionDto>.SuccessResult(MappingService.ToDto(pUpdatedTransaction), "Transakcja została zaktualizowana"));

  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<ApiResponse>> DeleteTransaction(int xId) {
    //funkcja usuwa transakcję po id
    //xId - identyfikator transakcji do usunięcia

    var pTransaction = await mContext.Transactions.FindAsync(xId);
    if (pTransaction == null)
      return NotFound(ApiResponse.Error("Transakcja nie została znaleziona"));

    mContext.Transactions.Remove(pTransaction);
    await mContext.SaveChangesAsync();

    return Ok(ApiResponse.SuccessResult("Transakcja została usunięta"));

  }

  [HttpGet("summary")]
  public async Task<ActionResult<ApiResponse<object>>> GetSummary(int xYear, int? xMonth = null) {
    //funkcja pobiera podsumowanie transakcji dla danego roku i opcjonalnie miesiąca
    //xYear - rok podsumowania
    //xMonth - opcjonalny miesiąc podsumowania

    var pQuery = mContext.Transactions
        .Include(t => t.Category)
        .Where(t => t.Year == xYear);

    if (xMonth.HasValue)
      pQuery = pQuery.Where(t => t.MonthNumber == xMonth.Value);

    var pSummary = await pQuery
        .GroupBy(t => new { t.CategoryId, t.Category.Name })
        .Select(g => new {
          CategoryId = g.Key.CategoryId,
          CategoryName = g.Key.Name,
          TotalAmount = g.Sum(t => t.Amount),
          TransactionCount = g.Count(),
          Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
          Expenses = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
        })
        .OrderByDescending(s => Math.Abs(s.TotalAmount))
        .ToListAsync();

    decimal pTotalIncome = pSummary.Sum(s => s.Income);
    decimal pTotalExpenses = Math.Abs(pSummary.Sum(s => s.Expenses));
    decimal pBalance = pTotalIncome - pTotalExpenses;

    var pResult = new {
      TotalIncome = pTotalIncome,
      TotalExpenses = pTotalExpenses,
      Balance = pBalance,
      Categories = pSummary
    };

    return Ok(ApiResponse<object>.SuccessResult(pResult));

  }

  [HttpPost("import")]
  public async Task<ActionResult<ApiResponse>> ImportTransactions([FromBody] List<TransactionDto> xTransactions) {
    //funkcja importuje listę transakcji
    //xTransactions - lista transakcji do zaimportowania

    if (xTransactions == null || !xTransactions.Any())
      return BadRequest(ApiResponse.Error("Brak transakcji do zaimportowania"));

    var ruleService = new FinancesTracker.Services.cCategoryRuleService(mContext);

    var pErrors = new List<string>();
    int importedCount = 0;

    foreach (var pDto in xTransactions) {
      pDto.Date = pDto.Date.ToUniversalTime(); //konwertuje datę na UTC

      // Sprawdź, czy istnieje już transakcja z tą samą nazwą, datą i kwotą
      bool exists = await mContext.Transactions.AnyAsync(t =>
        t.Description == pDto.Description &&
        t.Date == pDto.Date &&
        t.Amount == pDto.Amount
      );

      if (exists) {
        pErrors.Add($"Transakcja \"{pDto.Description}\" z dnia {pDto.Date:d} o kwocie {pDto.Amount} już istnieje.");
        continue;
      }

      //mapuje dto na encję i dodaje do kontekstu
      cTransaction pTransaction = MappingService.ToEntity(pDto);

      var (categoryId, subcategoryId) = await ruleService.MatchCategoryAsync(pTransaction.Description);
      pTransaction.CategoryId = categoryId;
      pTransaction.SubcategoryId = subcategoryId;

      mContext.Transactions.Add(pTransaction);
      importedCount++;
    }

    await mContext.SaveChangesAsync();

    if (pErrors.Any())
      return Ok(ApiResponse.Error($"Zaimportowano {importedCount} transakcji. Część transakcji nie została zaimportowana", pErrors));

    return Ok(ApiResponse.SuccessResult("Transakcje zostały zaimportowane"));

  }

  private async Task<bool> TransactionExistsAsync(int xId) {
    //funkcja sprawdza czy transakcja o podanym id istnieje
    //xId - identyfikator transakcji

    return await mContext.Transactions.AnyAsync(e => e.Id == xId);

  }
}
