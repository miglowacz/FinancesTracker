using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

using Microsoft.AspNetCore.Mvc;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase {
  private readonly ITransactionService _service;

  public TransactionsController(ITransactionService service) {
    _service = service;
  }

  [HttpGet]
  public async Task<ActionResult<cApiResponse<cPagedResult<cTransaction_DTO>>>> GetTransactions([FromQuery] cTransactionFilter_DTO xFilter) {
    var result = await _service.GetTransactionsAsync(xFilter);
    return Ok(cApiResponse<cPagedResult<cTransaction_DTO>>.SuccessResult(result));
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> GetTransaction(int xId) {
    var result = await _service.GetTransactionByIdAsync(xId);
    return result == null
        ? NotFound(cApiResponse<cTransaction_DTO>.Error("Nie znaleziono"))
        : Ok(cApiResponse<cTransaction_DTO>.SuccessResult(result));
  }

  [HttpPost]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> CreateTransaction(cTransaction_DTO dto) {
    if (!ModelState.IsValid) return BadRequest(cApiResponse<cTransaction_DTO>.Error("Błędne dane"));

    // Obsługa transferów
    if (xTransactionDto.IsTransfer && xTransactionDto.TargetAccountId.HasValue) {
        if (xTransactionDto.TargetAccountId.Value == xTransactionDto.AccountId)
             return BadRequest(cApiResponse<cTransaction_DTO>.Error("Konto źródłowe i docelowe muszą być różne"));

        var pTargetAccount = await _DB_Context.Accounts.FindAsync(xTransactionDto.TargetAccountId.Value);
        if (pTargetAccount == null)
             return BadRequest(cApiResponse<cTransaction_DTO>.Error("Konto docelowe nie istnieje"));

        // Tworzenie transakcji wychodzącej (z konta źródłowego)
        var pSourceTransaction = MappingService.ToEntity(xTransactionDto);
        pSourceTransaction.Amount = -Math.Abs(pSourceTransaction.Amount); // Upewniamy się, że jest ujemna
        pSourceTransaction.IsTransfer = true;
        
        // Tworzenie transakcji przychodzącej (na konto docelowe)
        var pTargetTransaction = new cTransaction {
             Date = pSourceTransaction.Date,
             Description = pSourceTransaction.Description, // Można dodać dopisek "(Transfer)"
             Amount = Math.Abs(pSourceTransaction.Amount), // Dodatnia kwota
             AccountId = xTransactionDto.TargetAccountId.Value,
             CategoryId = pSourceTransaction.CategoryId,
             SubcategoryId = pSourceTransaction.SubcategoryId,
             IsTransfer = true,
             IsInsignificant = pSourceTransaction.IsInsignificant,
             MonthNumber = pSourceTransaction.MonthNumber,
             Year = pSourceTransaction.Year,
             CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _DB_Context.Database.BeginTransactionAsync();
        try {
            _DB_Context.Transactions.Add(pSourceTransaction);
            _DB_Context.Transactions.Add(pTargetTransaction);
            await _DB_Context.SaveChangesAsync();

            // Ustawienie powiązań ID po zapisie
            pSourceTransaction.RelatedTransactionId = pTargetTransaction.Id;
            pTargetTransaction.RelatedTransactionId = pSourceTransaction.Id;

            await _DB_Context.SaveChangesAsync();
            await transaction.CommitAsync();

            var pCreatedTransaction = await _DB_Context.Transactions
              .Include(t => t.Account)
              .Include(t => t.Category)
              .Include(t => t.Subcategory)
              .FirstAsync(t => t.Id == pSourceTransaction.Id);

            return CreatedAtAction(nameof(GetTransaction),
              new { id = pSourceTransaction.Id },
              cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pCreatedTransaction), "Transfer został utworzony"));

        } catch (Exception ex) {
            await transaction.RollbackAsync();
            return StatusCode(500, cApiResponse<cTransaction_DTO>.Error($"Błąd podczas tworzenia transferu: {ex.Message}"));
        }
    }

    var pAccount = await _DB_Context.Accounts.FindAsync(xTransactionDto.AccountId);
    if (pAccount == null)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrane konto nie istnieje"));

    var pCategory = await _DB_Context.Categories.Include(c => c.Subcategories)
      .FirstOrDefaultAsync(c => c.Id == xTransactionDto.CategoryId);

    if (pCategory == null)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana kategoria nie istnieje"));

    if (!pCategory.Subcategories.Any(s => s.Id == xTransactionDto.SubcategoryId))
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

    var pTransaction = MappingService.ToEntity(xTransactionDto);
    _DB_Context.Transactions.Add(pTransaction);
    await _DB_Context.SaveChangesAsync();

    var pCreatedTransactionSimple = await _DB_Context.Transactions
      .Include(t => t.Account)
      .Include(t => t.Category)
      .Include(t => t.Subcategory)  
      .FirstAsync(t => t.Id == pTransaction.Id);

    return CreatedAtAction(nameof(GetTransaction),
      new { id = pTransaction.Id },
      cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pCreatedTransactionSimple), "Transakcja została utworzona"));
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> UpdateTransaction(int id, cTransaction_DTO xTransactionDto) {
    if (id != xTransactionDto.Id)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("ID transakcji nie pasuje"));

    var pExistingTransaction = await _DB_Context.Transactions
        .Include(t => t.Account)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (pExistingTransaction == null)
      return NotFound(cApiResponse<cTransaction_DTO>.Error("Transakcja nie została znaleziona"));

    using var dbTransaction = await _DB_Context.Database.BeginTransactionAsync();
    try {
      // 1. Podstawowa aktualizacja pól
      pExistingTransaction.Date = xTransactionDto.Date.ToUniversalTime();
      pExistingTransaction.Description = xTransactionDto.Description;
      pExistingTransaction.Amount = xTransactionDto.Amount;
      pExistingTransaction.AccountId = xTransactionDto.AccountId;
      pExistingTransaction.CategoryId = xTransactionDto.CategoryId;
      pExistingTransaction.SubcategoryId = xTransactionDto.SubcategoryId;
      pExistingTransaction.IsInsignificant = xTransactionDto.IsInsignificant;
      pExistingTransaction.MonthNumber = xTransactionDto.Date.Month;
      pExistingTransaction.Year = xTransactionDto.Date.Year;
      pExistingTransaction.UpdatedAt = DateTime.UtcNow;

      // 2. Obsługa powiązanego transferu (jeśli istnieje)
      if (pExistingTransaction.RelatedTransactionId.HasValue) {
        var pRelated = await _DB_Context.Transactions.FindAsync(pExistingTransaction.RelatedTransactionId.Value);
        if (pRelated != null) {
          // Synchronizujemy kluczowe dane, ale odwracamy kwotę
          pRelated.Date = pExistingTransaction.Date;
          pRelated.Amount = -pExistingTransaction.Amount; // Ważne: przeciwny znak
          pRelated.Description = pExistingTransaction.Description;
          pRelated.MonthNumber = pExistingTransaction.MonthNumber;
          pRelated.Year = pExistingTransaction.Year;
          pRelated.UpdatedAt = DateTime.UtcNow;

          // Kategorie w transferach zazwyczaj są takie same (lub puste)
          pRelated.CategoryId = pExistingTransaction.CategoryId;
          pRelated.SubcategoryId = pExistingTransaction.SubcategoryId;
        }
      }

      await _DB_Context.SaveChangesAsync();
      await dbTransaction.CommitAsync();

      // Ponowne pobranie z includami do DTO
      var pResult = await _DB_Context.Transactions
          .Include(t => t.Account)
          .Include(t => t.Category)
          .Include(t => t.Subcategory)
          .FirstAsync(t => t.Id == id);

      return Ok(cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pResult), "Zaktualizowano pomyślnie"));
    } catch (Exception ex) {
      await dbTransaction.RollbackAsync();
      return StatusCode(500, cApiResponse<cTransaction_DTO>.Error($"Błąd aktualizacji: {ex.Message}"));
    }

  }
  [HttpDelete("{id}")]
  public async Task<ActionResult<cApiResponse>> DeleteTransaction(int id) {

    var pTransaction = await _DB_Context.Transactions.FindAsync(id);

    if (pTransaction == null)
      return NotFound(cApiResponse.Error("Transakcja nie została znaleziona"));

    using var dbTransaction = await _DB_Context.Database.BeginTransactionAsync();

    try {
      // Jeśli to transfer, musimy usunąć też powiązaną transakcję
      if (pTransaction.RelatedTransactionId.HasValue) {
        var pRelated = await _DB_Context.Transactions.FindAsync(pTransaction.RelatedTransactionId.Value);
        if (pRelated != null) {
          _DB_Context.Transactions.Remove(pRelated);
        }
      }

      _DB_Context.Transactions.Remove(pTransaction);
      await _DB_Context.SaveChangesAsync();
      await dbTransaction.CommitAsync();

      return Ok(cApiResponse.SuccessResult("Transakcja (i ewentualny powiązany transfer) została usunięta"));
    } catch (Exception ex) {
      await dbTransaction.RollbackAsync();
      return StatusCode(500, cApiResponse.Error($"Błąd podczas usuwania: {ex.Message}"));
    }

  }

  [HttpPatch("{id}/toggle-insignificant")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> ToggleInsignificant(int id) {
    var pTransaction = await _DB_Context.Transactions
      .Include(t => t.Account)
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
  public async Task<ActionResult<cApiResponse<cSummary_DTO>>> GetSummary(int xYear, int? xMonth = null, bool xIncludeInsignificant = false) {
    var pQuery = _DB_Context.Transactions
      .Include(t => t.Category)
      .Where(t => t.Year == xYear);

    if (xMonth.HasValue)
      pQuery = pQuery.Where(t => t.MonthNumber == xMonth.Value);

    if (!xIncludeInsignificant)
      pQuery = pQuery.Where(t => !t.IsInsignificant);

    var pSummary = await pQuery
      .GroupBy(t => new { t.CategoryId, t.Category.Name })
      .Select(g => new CategorySummaryDTO {
        CategoryName = g.Key.Name ?? "Bez kategorii",
        Expenses = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
      })
      .Where(s => s.Expenses < 0)
      .OrderBy(s => s.Expenses)
      .ToListAsync();

    decimal pTotalIncome = await pQuery.Where(t => t.Amount > 0).SumAsync(t => t.Amount);
    decimal pTotalExpenses = Math.Abs(await pQuery.Where(t => t.Amount < 0).SumAsync(t => t.Amount));
    decimal pBalance = pTotalIncome - pTotalExpenses;

    var pResult = new cSummary_DTO {
      TotalIncome = pTotalIncome,
      TotalExpenses = pTotalExpenses,
      Balance = pBalance,
      Categories = pSummary
    };

    return Ok(cApiResponse<cSummary_DTO>.SuccessResult(pResult));
  }

  [HttpPost("import")]
  public async Task<ActionResult<cApiResponse>> ImportTransactions([FromBody] List<cTransaction_DTO> transactions) {
    var result = await _service.ImportTransactionsAsync(transactions);
    return Ok(cApiResponse.SuccessResult($"Zaimportowano {result.ImportedCount}"));
  }
}
