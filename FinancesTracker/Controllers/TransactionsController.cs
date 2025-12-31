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

  private readonly FinancesTrackerDbContext _DB_Context;

  public TransactionsController(FinancesTrackerDbContext xContext) {
    //konstruktor kontrolera transakcji
    //xContext - kontekst bazy danych

    _DB_Context = xContext;

  }

  [HttpGet]
  public async Task<ActionResult<cApiResponse<cPagedResult<cTransaction_DTO>>>> GetTransactions([FromQuery] cTransactionFilter_DTO xFilter) {
    //funkcja pobiera listę transakcji z filtrowaniem, sortowaniem i paginacją
    //xFilter - obiekt filtrujący transakcje

    var pQuery = _DB_Context.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .AsQueryable();

    //filtrowanie po roku
    if (xFilter.Year.HasValue)
      pQuery = pQuery.Where(t => t.Year == xFilter.Year.Value);

    //filtrowanie po miesiącu
    if (xFilter.Month.HasValue)
      pQuery = pQuery.Where(t => t.MonthNumber == xFilter.Month.Value);

    //filtrowanie po transakcjach bez kategorii
    if (xFilter.HasNoCategory)
      pQuery = pQuery.Where(t => t.CategoryId == null);

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

    //filtrowanie po statusie nieistotności
    if (xFilter.IsInsignificant.HasValue)
      pQuery = pQuery.Where(t => t.IsInsignificant == xFilter.IsInsignificant.Value);
    else if (!xFilter.IncludeInsignificant)
      pQuery = pQuery.Where(t => !t.IsInsignificant);

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
    var pResult = new cPagedResult<cTransaction_DTO> {
      Items = pTransactions.Select(MappingService.ToDto).ToList(),
      TotalCount = pTotalCount,
      PageNumber = xFilter.PageNumber,
      PageSize = xFilter.PageSize
    };

    return Ok(cApiResponse<cPagedResult<cTransaction_DTO>>.SuccessResult(pResult));

  }

  [HttpGet("{id}")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> GetTransaction(int xId) {
    //funkcja pobiera pojedynczą transakcję po id
    //xId - identyfikator transakcji

    var pTransaction = await _DB_Context.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .FirstOrDefaultAsync(t => t.Id == xId);

    if (pTransaction == null)
      return NotFound(cApiResponse<cTransaction_DTO>.Error("Transakcja nie została znaleziona"));

    return Ok(cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pTransaction)));

  }

  [HttpPost]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> CreateTransaction(cTransaction_DTO xTransactionDto) {
    //funkcja tworzy nową transakcję
    //xTransactionDto - dane nowej transakcji

    if (!ModelState.IsValid) {
      var pErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Dane transakcji są nieprawidłowe", pErrors));
    }

    //sprawdza czy kategoria i podkategoria istnieją
    var pCategory = await _DB_Context.Categories.Include(c => c.Subcategories)
        .FirstOrDefaultAsync(c => c.Id == xTransactionDto.CategoryId);

    if (pCategory == null)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana kategoria nie istnieje"));

    if (!pCategory.Subcategories.Any(s => s.Id == xTransactionDto.SubcategoryId))
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

    var pTransaction = MappingService.ToEntity(xTransactionDto);
    _DB_Context.Transactions.Add(pTransaction);
    await _DB_Context.SaveChangesAsync();

    //pobiera utworzoną transakcję z relacjami
    var pCreatedTransaction = await _DB_Context.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .FirstAsync(t => t.Id == pTransaction.Id);

    return CreatedAtAction(nameof(GetTransaction),
        new { id = pTransaction.Id },
        cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pCreatedTransaction), "Transakcja została utworzona"));

  }

  [HttpPut("{id}")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> UpdateTransaction(int id, cTransaction_DTO xTransactionDto) {
    //funkcja aktualizuje istniejącą transakcję
    //xId - identyfikator transakcji
    //xTransactionDto - dane do aktualizacji transakcji

    if (id != xTransactionDto.Id)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("ID transakcji nie pasuje"));

    if (!ModelState.IsValid) {
      var pErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Dane transakcji są nieprawidłowe", pErrors));
    }

    var pExistingTransaction = await _DB_Context.Transactions.FindAsync(id);
    if (pExistingTransaction == null)
      return NotFound(cApiResponse<cTransaction_DTO>.Error("Transakcja nie została znaleziona"));

    //sprawdza czy kategoria i podkategoria istnieją
    var pCategory = await _DB_Context.Categories.Include(c => c.Subcategories)
        .FirstOrDefaultAsync(c => c.Id == xTransactionDto.CategoryId);

    if (pCategory == null)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana kategoria nie istnieje"));

    if (!pCategory.Subcategories.Any(s => s.Id == xTransactionDto.SubcategoryId))
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

    //aktualizuje właściwości transakcji
    pExistingTransaction.Date = xTransactionDto.Date;
    pExistingTransaction.Description = xTransactionDto.Description;
    pExistingTransaction.Amount = xTransactionDto.Amount;
    pExistingTransaction.CategoryId = xTransactionDto.CategoryId;
    pExistingTransaction.SubcategoryId = xTransactionDto.SubcategoryId;
    pExistingTransaction.BankName = xTransactionDto.BankName;
    pExistingTransaction.IsInsignificant = xTransactionDto.IsInsignificant;
    pExistingTransaction.MonthNumber = xTransactionDto.Date.Month;
    pExistingTransaction.Year = xTransactionDto.Date.Year;
    pExistingTransaction.UpdatedAt = DateTime.UtcNow;

    await _DB_Context.SaveChangesAsync();

    //pobiera zaktualizowaną transakcję z relacjami
    var pUpdatedTransaction = await _DB_Context.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .FirstAsync(t => t.Id == id);

    return Ok(cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pUpdatedTransaction), "Transakcja została zaktualizowana"));

  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<cApiResponse>> DeleteTransaction(int xId) {
    //funkcja usuwa transakcję po id
    //xId - identyfikator transakcji do usunięcia

    var pTransaction = await _DB_Context.Transactions.FindAsync(xId);
    if (pTransaction == null)
      return NotFound(cApiResponse.Error("Transakcja nie została znaleziona"));

    _DB_Context.Transactions.Remove(pTransaction);
    await _DB_Context.SaveChangesAsync();

    return Ok(cApiResponse.SuccessResult("Transakcja została usunięta"));

  }

  [HttpPatch("{id}/toggle-insignificant")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> ToggleInsignificant(int id) {
    //funkcja przełącza status nieistotności transakcji
    //id - identyfikator transakcji

    var pTransaction = await _DB_Context.Transactions
        .Include(t => t.Category)
        .Include(t => t.Subcategory)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (pTransaction == null)
      return NotFound(cApiResponse<cTransaction_DTO>.Error("Transakcja nie została znaleziona"));

    pTransaction.IsInsignificant = !pTransaction.IsInsignificant;
    pTransaction.UpdatedAt = DateTime.UtcNow;

    await _DB_Context.SaveChangesAsync();

    return Ok(cApiResponse<cTransaction_DTO>.SuccessResult(
        MappingService.ToDto(pTransaction),
        $"Transakcja została oznaczona jako {(pTransaction.IsInsignificant ? "nieistotna" : "istotna")}"));

  }

  [HttpGet("summary")]
  public async Task<ActionResult<cApiResponse<object>>> GetSummary(int xYear, int? xMonth = null, bool xIncludeInsignificant = false) {
    //funkcja pobiera podsumowanie transakcji dla danego roku i opcjonalnie miesiąca
    //xYear - rok podsumowania
    //xMonth - opcjonalny miesiąc podsumowania
    //xIncludeInsignificant - czy uwzględnić transakcje nieistotne

    var pQuery = _DB_Context.Transactions
        .Include(t => t.Category)
        .Where(t => t.Year == xYear);

    if (xMonth.HasValue)
      pQuery = pQuery.Where(t => t.MonthNumber == xMonth.Value);

    //filtruj nieistotne transakcje jeśli nie powinny być uwzględnione
    if (!xIncludeInsignificant)
      pQuery = pQuery.Where(t => !t.IsInsignificant);

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
      Categories = pSummary,
      IncludesInsignificant = xIncludeInsignificant
    };

    return Ok(cApiResponse<object>.SuccessResult(pResult));

  }

  [HttpPost("import")]
  public async Task<ActionResult<cApiResponse>> ImportTransactions([FromBody] List<cTransaction_DTO> transactionsCln) {
    //funkcja importuje listę transakcji
    //xTransactions - lista transakcji do zaimportowania

    if (transactionsCln == null || !transactionsCln.Any())
      return BadRequest(cApiResponse.Error("Brak transakcji do zaimportowania"));

    var ruleService = new Services.cCategoryRuleService(_DB_Context);
    var insignificantDetector = new cInsignificantTransactionDetector();

    var errorsCln = new List<string>();
    int importedCount = 0;
    int insignificantCount = 0;

    foreach (var transaction_DTO in transactionsCln) {
      transaction_DTO.Date = transaction_DTO.Date.ToUniversalTime(); //konwertuje datę na UTC

      // Sprawdź, czy istnieje już transakcja z tą samą nazwą, datą i kwotą
      bool exists = await _DB_Context.Transactions.AnyAsync(t =>
        t.Description == transaction_DTO.Description &&
        t.Date == transaction_DTO.Date &&
        t.Amount == transaction_DTO.Amount
      );

      if (exists) {
        errorsCln.Add($"Transakcja \"{transaction_DTO.Description}\" z dnia {transaction_DTO.Date:d} o kwocie {transaction_DTO.Amount} już istnieje.");
        continue;
      }

      //sprawdź czy transakcja powinna być oznaczona jako nieistotna
      if (insignificantDetector.IsInsignificant(transaction_DTO)) {
        transaction_DTO.IsInsignificant = true;
        insignificantCount++;
      }

      //mapuje dto na encję i dodaje do kontekstu
      cTransaction transaction = MappingService.ToEntity(transaction_DTO);

      var (categoryId, subcategoryId) = await ruleService.MatchCategoryAsync(transaction.Description);
      transaction.CategoryId = categoryId;
      transaction.SubcategoryId = subcategoryId;
      transaction.AccountName = cAccountNameDetector.GetAccountType(transaction_DTO.AccountName);

      _DB_Context.Transactions.Add(transaction);
      importedCount++;
    }

    await _DB_Context.SaveChangesAsync();

    string successMessage = $"Zaimportowano {importedCount} transakcji";
    if (insignificantCount > 0)
      successMessage += $" ({insignificantCount} oznaczono jako nieistotne)";

    if (errorsCln.Any())
      return Ok(cApiResponse.Error($"{successMessage}. Część transakcji nie została zaimportowana", errorsCln));

    return Ok(cApiResponse.SuccessResult(successMessage));

  }

  private async Task<bool> TransactionExistsAsync(int xId) {
    //funkcja sprawdza czy transakcja o podanym id istnieje
    //xId - identyfikator transakcji

    return await _DB_Context.Transactions.AnyAsync(e => e.Id == xId);

  }
}
