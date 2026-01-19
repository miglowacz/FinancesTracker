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
    _DB_Context = xContext;
  }

  [HttpGet]
  public async Task<ActionResult<cApiResponse<cPagedResult<cTransaction_DTO>>>> GetTransactions([FromQuery] cTransactionFilter_DTO xFilter) {
    var pQuery = _DB_Context.Transactions
      .Include(t => t.Account)
      .Include(t => t.Category)
      .Include(t => t.Subcategory)
      .Include(t => t.RelatedTransaction).ThenInclude(rt => rt.Account) // Include related account
      .AsQueryable();

    if (xFilter.Year.HasValue)
      pQuery = pQuery.Where(t => t.Year == xFilter.Year.Value);

    if (xFilter.Month.HasValue)
      pQuery = pQuery.Where(t => t.MonthNumber == xFilter.Month.Value);

    if (xFilter.HideTransfers)
      pQuery = pQuery.Where(t => !t.IsTransfer);

    if (xFilter.HasNoCategory)
      pQuery = pQuery.Where(t => t.CategoryId == null);

    if (xFilter.CategoryId.HasValue)
      pQuery = pQuery.Where(t => t.CategoryId == xFilter.CategoryId.Value);

    if (xFilter.SubcategoryId.HasValue)
      pQuery = pQuery.Where(t => t.SubcategoryId == xFilter.SubcategoryId.Value);

    if (xFilter.MinAmount.HasValue)
      pQuery = pQuery.Where(t => t.Amount >= xFilter.MinAmount.Value);

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

    if (xFilter.HideTransfers)
      pQuery = pQuery.Where(t => !t.IsTransfer); // Filtering

    if (!string.IsNullOrEmpty(xFilter.SearchTerm))
      pQuery = pQuery.Where(t => t.Description.Contains(xFilter.SearchTerm));

    if (xFilter.IsInsignificant.HasValue)
      pQuery = pQuery.Where(t => t.IsInsignificant == xFilter.IsInsignificant.Value);
    else if (!xFilter.IncludeInsignificant)
      pQuery = pQuery.Where(t => !t.IsInsignificant);

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

    int pTotalCount = await pQuery.CountAsync();

    var pTransactions = await pQuery
      .Skip((xFilter.PageNumber - 1) * xFilter.PageSize)
      .Take(xFilter.PageSize)
      .ToListAsync();

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
    var pTransaction = await _DB_Context.Transactions
      .Include(t => t.Account)
      .Include(t => t.Category)
      .Include(t => t.Subcategory)
      .FirstOrDefaultAsync(t => t.Id == xId);

    if (pTransaction == null)
      return NotFound(cApiResponse<cTransaction_DTO>.Error("Transakcja nie została znaleziona"));

    return Ok(cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pTransaction)));
  }

  [HttpPost]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> CreateTransaction(cTransaction_DTO xTransactionDto) {
    if (!ModelState.IsValid) {
      var pErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Dane transakcji są nieprawidłowe", pErrors));
    }

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
  public async Task<ActionResult<cApiResponse<object>>> GetSummary(int xYear, int? xMonth = null, bool xIncludeInsignificant = false) {
    var pQuery = _DB_Context.Transactions
      .Include(t => t.Category)
      .Where(t => t.Year == xYear);

    if (xMonth.HasValue)
      pQuery = pQuery.Where(t => t.MonthNumber == xMonth.Value);

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
  [HttpPost("import")]
  public async Task<ActionResult<cApiResponse>> ImportTransactions([FromBody] List<cTransaction_DTO> transactionsCln) {
    if (transactionsCln == null || !transactionsCln.Any())
      return BadRequest(cApiResponse.Error("Brak transakcji do zaimportowania"));

    var ruleService = new Services.cCategoryRuleService(_DB_Context);
    var accountRuleService = new Services.cAccountRuleService(_DB_Context);
    var insignificantDetector = new cInsignificantTransactionDetector();

    var errorsCln = new List<string>();
    int importedCount = 0;
    int insignificantCount = 0;

    // Pobieramy identyfikatory Twoich kont (np. końcówki numerów) do parowania "paradoksów"
    var myAccountIdentifiers = await _DB_Context.Accounts
        .Where(a => !string.IsNullOrEmpty(a.ImportIdentifier))
        .Select(a => a.ImportIdentifier)
        .ToListAsync();

    var addedTransactions = new List<cTransaction>();
    var matchedDbIds = new HashSet<int>();

    using var dbTransaction = await _DB_Context.Database.BeginTransactionAsync();

    try {
      // ETAP 1: Przetwarzanie i dodawanie transakcji
      foreach (var transaction_DTO in transactionsCln) {
        transaction_DTO.Date = transaction_DTO.Date.ToUniversalTime();

        // --- A. Obsługa Konta (Lazy Creation) ---
        if (transaction_DTO.AccountId <= 0) {
          var account = await _DB_Context.Accounts.FirstOrDefaultAsync(a => a.Name == transaction_DTO.AccountName);
          if (account != null) {
            transaction_DTO.AccountId = account.Id;
          } else {
            var matchedId = await accountRuleService.MatchAccountAsync(transaction_DTO.AccountName);
            if (matchedId.HasValue) {
              transaction_DTO.AccountId = matchedId.Value;
            } else if (!string.IsNullOrEmpty(transaction_DTO.AccountName)) {
              // Tworzymy nowe konto automatycznie
              var pNewAcc = new cAccount {
                Name = transaction_DTO.AccountName,
                BankName = "Import Automatyczny",
                InitialBalance = 0,
                IsActive = true,
                Currency = "PLN"
              };
              _DB_Context.Accounts.Add(pNewAcc);
              await _DB_Context.SaveChangesAsync();
              transaction_DTO.AccountId = pNewAcc.Id;
              errorsCln.Add($"Utworzono nowe konto: {pNewAcc.Name}");
            }
          }
        }

        if (transaction_DTO.AccountId <= 0) {
          errorsCln.Add($"Pominięto \"{transaction_DTO.Description}\": brak przypisanego konta.");
          continue;
        }

        // --- B. Sprawdzenie duplikatów ---
        bool exists = await _DB_Context.Transactions.AnyAsync(t =>
            t.Description == transaction_DTO.Description &&
            t.Date == transaction_DTO.Date &&
            t.Amount == transaction_DTO.Amount &&
            t.AccountId == transaction_DTO.AccountId
        );
        if (exists) continue;

        // --- C. Rozpoznawanie Transferu (Rozwiązanie paradoksu FS/ vs Transfer) ---
        bool isTransfer = false;
        string pDesc = transaction_DTO.Description.ToLowerInvariant();

        // 1. Priorytet: Czy w opisie jest numer Twojego innego konta?
        if (myAccountIdentifiers.Any(id => pDesc.Contains(id.ToLower()))) {
          isTransfer = true;
        }
        // 2. Priorytet: Jeśli nie, sprawdź słowa kluczowe (wykluczając FS/FV)
        else {
          string[] forbidden = { "fs/", "fv/", "faktura", "zapłata za" };
          string[] transferWords = { "przelew wewnętrzny", "przelew własny", "transfer" };

          bool hasForbidden = forbidden.Any(k => pDesc.Contains(k));
          bool hasTransferWord = transferWords.Any(k => pDesc.Contains(k));

          if (hasTransferWord && !hasForbidden) isTransfer = true;
        }

        // --- D. Insignificant (Tylko dla zwykłych transakcji) ---
        bool isInsignificant = false;
        if (!isTransfer) {
          isInsignificant = insignificantDetector.IsInsignificant(transaction_DTO, myAccountIdentifiers);
          if (isInsignificant) insignificantCount++;
        }

        // --- E. Mapowanie i Kategorie ---
        cTransaction entity = MappingService.ToEntity(transaction_DTO);
        entity.IsTransfer = isTransfer;
        entity.IsInsignificant = isInsignificant;

        var (catId, subCatId) = await ruleService.MatchCategoryAsync(entity.Description);
        entity.CategoryId = catId;
        entity.SubcategoryId = subCatId;

        _DB_Context.Transactions.Add(entity);
        addedTransactions.Add(entity);
        importedCount++;
      }

      await _DB_Context.SaveChangesAsync();

      // ETAP 2: Parowanie Transferów
      var matchableInSession = new List<cTransaction>();
      foreach (var trans in addedTransactions) {
        if (!trans.IsTransfer || trans.RelatedTransactionId != null) {
          if (trans.IsTransfer) matchableInSession.Add(trans);
          continue;
        }

        // Parowanie w sesji
        var internalMatch = matchableInSession.FirstOrDefault(t =>
            t.RelatedTransactionId == null &&
            t.Amount == -trans.Amount &&
            t.AccountId != trans.AccountId &&
            Math.Abs((t.Date - trans.Date).TotalDays) <= 3);

        if (internalMatch != null) {
          trans.RelatedTransactionId = internalMatch.Id;
          internalMatch.RelatedTransactionId = trans.Id;
        } else {
          // Parowanie z bazą
          var dbMatch = await _DB_Context.Transactions
              .FirstOrDefaultAsync(t => t.IsTransfer && t.RelatedTransactionId == null &&
                  t.Amount == -trans.Amount && t.AccountId != trans.AccountId &&
                  t.Date >= trans.Date.AddDays(-3) && t.Date <= trans.Date.AddDays(3) &&
                  !matchedDbIds.Contains(t.Id) && !addedTransactions.Any(at => at.Id == t.Id));

          if (dbMatch != null) {
            trans.RelatedTransactionId = dbMatch.Id;
            dbMatch.RelatedTransactionId = trans.Id;
            matchedDbIds.Add(dbMatch.Id);
          }
        }
        matchableInSession.Add(trans);
      }

      await _DB_Context.SaveChangesAsync();
      await dbTransaction.CommitAsync();

    } catch (Exception ex) {
      await dbTransaction.RollbackAsync();
      return StatusCode(500, cApiResponse.Error($"Błąd importu: {ex.Message}"));
    }

    string msg = $"Zaimportowano {importedCount} transakcji.";
    if (insignificantCount > 0) msg += $" ({insignificantCount} nieistotnych)";

    return errorsCln.Any()
        ? Ok(cApiResponse.Error($"{msg} Część wpisów wymagała uwagi.", errorsCln))
        : Ok(cApiResponse.SuccessResult(msg));
  }

  private async Task<bool> TransactionExistsAsync(int xId) {
    return await _DB_Context.Transactions.AnyAsync(e => e.Id == xId);
  }
}
