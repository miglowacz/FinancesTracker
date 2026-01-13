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

    if (!ModelState.IsValid) {
      var pErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Dane transakcji są nieprawidłowe", pErrors));
    }

    var pExistingTransaction = await _DB_Context.Transactions.FindAsync(id);
    if (pExistingTransaction == null)
      return NotFound(cApiResponse<cTransaction_DTO>.Error("Transakcja nie została znaleziona"));

    var pAccount = await _DB_Context.Accounts.FindAsync(xTransactionDto.AccountId);
    if (pAccount == null)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrane konto nie istnieje"));

    var pCategory = await _DB_Context.Categories.Include(c => c.Subcategories)
      .FirstOrDefaultAsync(c => c.Id == xTransactionDto.CategoryId);

    if (pCategory == null)
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana kategoria nie istnieje"));

    if (!pCategory.Subcategories.Any(s => s.Id == xTransactionDto.SubcategoryId))
      return BadRequest(cApiResponse<cTransaction_DTO>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

    pExistingTransaction.Date = xTransactionDto.Date;
    pExistingTransaction.Description = xTransactionDto.Description;
    pExistingTransaction.Amount = xTransactionDto.Amount;
    pExistingTransaction.AccountId = xTransactionDto.AccountId;
    pExistingTransaction.CategoryId = xTransactionDto.CategoryId;
    pExistingTransaction.SubcategoryId = xTransactionDto.SubcategoryId;
    pExistingTransaction.IsInsignificant = xTransactionDto.IsInsignificant;
    pExistingTransaction.MonthNumber = xTransactionDto.Date.Month;
    pExistingTransaction.Year = xTransactionDto.Date.Year;
    pExistingTransaction.UpdatedAt = DateTime.UtcNow;

    await _DB_Context.SaveChangesAsync();

    var pUpdatedTransaction = await _DB_Context.Transactions
      .Include(t => t.Account)
      .Include(t => t.Category)
      .Include(t => t.Subcategory)
      .FirstAsync(t => t.Id == id);

    return Ok(cApiResponse<cTransaction_DTO>.SuccessResult(MappingService.ToDto(pUpdatedTransaction), "Transakcja została zaktualizowana"));
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<cApiResponse>> DeleteTransaction(int xId) {
    var pTransaction = await _DB_Context.Transactions.FindAsync(xId);
    if (pTransaction == null)
      return NotFound(cApiResponse.Error("Transakcja nie została znaleziona"));

    _DB_Context.Transactions.Remove(pTransaction);
    await _DB_Context.SaveChangesAsync();

    return Ok(cApiResponse.SuccessResult("Transakcja została usunięta"));
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
  public async Task<ActionResult<cApiResponse>> ImportTransactions([FromBody] List<cTransaction_DTO> transactionsCln) {
    if (transactionsCln == null || !transactionsCln.Any())
      return BadRequest(cApiResponse.Error("Brak transakcji do zaimportowania"));

    var ruleService = new Services.cCategoryRuleService(_DB_Context);
    var accountRuleService = new Services.cAccountRuleService(_DB_Context);
    var insignificantDetector = new cInsignificantTransactionDetector();

    var errorsCln = new List<string>();
    int importedCount = 0;
    int insignificantCount = 0;

    // Lista śledząca pomyślnie dodane encje (aby w drugim kroku je sparować)
    var addedTransactions = new List<cTransaction>();
    var matchedDbIds = new HashSet<int>(); 

    // Używamy transakcji DB, aby cały proces (import + linkowanie) był atomowy
    using var dbTransaction = await _DB_Context.Database.BeginTransactionAsync();

    try {
        // ETAP 1: Dodanie wszystkich transakcji (bez linków) i zapis, aby wygenerować ID
        foreach (var transaction_DTO in transactionsCln) {
          transaction_DTO.Date = transaction_DTO.Date.ToUniversalTime();

          // 1. Dopasowanie konta (jak w oryginale)
          if (transaction_DTO.AccountId <= 0) {
            if (!string.IsNullOrEmpty(transaction_DTO.AccountName)) {
               var account = await _DB_Context.Accounts.FirstOrDefaultAsync(a => a.Name == transaction_DTO.AccountName);
               if (account != null) transaction_DTO.AccountId = account.Id;
            }
            if (transaction_DTO.AccountId <= 0) {
              var matchedAccountId = await accountRuleService.MatchAccountAsync(transaction_DTO.AccountName);
              if (matchedAccountId.HasValue) transaction_DTO.AccountId = matchedAccountId.Value;
            }
          }

          var accountExists = await _DB_Context.Accounts.AnyAsync(a => a.Id == transaction_DTO.AccountId);
          if (!accountExists) {
            errorsCln.Add($"Konto o ID {transaction_DTO.AccountId} nie istnieje.");
           // continue; // Opcjonalnie: continue, w oryginale zakomentowane
          }

          // 2. Sprawdzenie duplikatów
          bool exists = await _DB_Context.Transactions.AnyAsync(t =>
            t.Description == transaction_DTO.Description &&
            t.Date == transaction_DTO.Date &&
            t.Amount == transaction_DTO.Amount &&
            t.AccountId == transaction_DTO.AccountId
          );

          if (exists) {
            errorsCln.Add($"Transakcja \"{transaction_DTO.Description}\" z dnia {transaction_DTO.Date:d} o kwocie {transaction_DTO.Amount} już istnieje.");
            continue;
          }

          if (insignificantDetector.IsInsignificant(transaction_DTO)) {
            transaction_DTO.IsInsignificant = true;
            insignificantCount++;
          }

          cTransaction transaction = MappingService.ToEntity(transaction_DTO);

          // 4. Dopasowanie kategorii
          var (categoryId, subcategoryId) = await ruleService.MatchCategoryAsync(transaction.Description);
          transaction.CategoryId = categoryId;
          transaction.SubcategoryId = subcategoryId;

          _DB_Context.Transactions.Add(transaction);
          addedTransactions.Add(transaction); // Dodajemy do lokalnej listy do późniejszego parowania
          importedCount++;
        }

        // ZAPIS 1: Generowanie ID dla nowych transakcji
        await _DB_Context.SaveChangesAsync();

        // ETAP 2: Parowanie transferów (mając już ID)
        var matchablePendingTransfers = new List<cTransaction>(); // Transakcje z bieżącego importu do parowania

        foreach (var transaction in addedTransactions) {
          if (!transaction.IsTransfer) continue;
          if (transaction.RelatedTransactionId != null) continue; // Już sparowany w poprzedniej pętli jako "internalMatch"

            bool paired = false;

            // A. Próba parowania wewnątrz aktualnego importu (korzystamy z addedTransactions, które są śledzone przez kontekst)
            // Szukamy w liście matchablePendingTransfers, tak jak w pierwotnej logice
            var internalMatch = matchablePendingTransfers.FirstOrDefault(t => 
                 t.RelatedTransactionId == null &&      // Nie ma jeszcze pary
                 t.Amount == -transaction.Amount &&   // Przeciwna kwota
                 t.AccountId != transaction.AccountId && // Inne konto
                 Math.Abs((t.Date - transaction.Date).TotalDays) <= 3 // Zbliżona data
            );

            if (internalMatch != null) {
                // Teraz bezpiecznie ustawiamy ID, bo oba obiekty mają ID po SaveChanges
                transaction.RelatedTransactionId = internalMatch.Id;
                internalMatch.RelatedTransactionId = transaction.Id;
                paired = true;
            }

            // B. Jeśli nie sparowano lokalnie, szukamy w bazie danych
            if (!paired) {
                 decimal targetAmount = -transaction.Amount;
                 DateTime dMin = transaction.Date.AddDays(-3);
                 DateTime dMax = transaction.Date.AddDays(3);

                 // Pobieramy kandydatów z bazy, ale wykluczamy te z obecnego importu (id są w addedTransactions)
                 // aby uniknąć błędnego parowania "na krzyż" z niesparowanymi jeszcze elementami pętli
                 var dbCandidates = await _DB_Context.Transactions
                    .Where(t => t.IsTransfer && t.RelatedTransactionId == null)
                    .Where(t => t.Amount == targetAmount)
                    .Where(t => t.AccountId != transaction.AccountId)
                    .Where(t => t.Date >= dMin && t.Date <= dMax)
                    .ToListAsync();

                 foreach (var candidate in dbCandidates) {
                     // Pomijamy jeśli kandydat to ta sama transakcja (teoretycznie niemożliwe przez filtry, ale dla pewności)
                     if (candidate.Id == transaction.Id) continue;
                     
                     // Pomijamy jeśli kandydat pochodzi z tego samego importu (zostanie obsłużony przez internalMatch)
                     if (addedTransactions.Any(at => at.Id == candidate.Id)) continue;

                     if (!matchedDbIds.Contains(candidate.Id)) {
                         transaction.RelatedTransactionId = candidate.Id;
                         candidate.RelatedTransactionId = transaction.Id;
                         
                         matchedDbIds.Add(candidate.Id); 
                         paired = true;
                         break; 
                     }
                 }
            }

            // Dodaj do lokalnej listy "oczekujących", aby kolejne transakcje w pętli mogły się z nią sparować (punkt A)
            matchablePendingTransfers.Add(transaction);
        }

        // ZAPIS 2: Aktualizacja powiązań
        if (importedCount > 0) {
           await _DB_Context.SaveChangesAsync();
        }

        await dbTransaction.CommitAsync();

    } catch (Exception) {
        await dbTransaction.RollbackAsync();
        throw; // Rzucamy dalej, aby klient dostał info o błędzie (500)
    }

    string successMessage = $"Zaimportowano {importedCount} transakcji";
    if (insignificantCount > 0)
      successMessage += $" ({insignificantCount} oznaczono jako nieistotne)";

    if (errorsCln.Any())
      return Ok(cApiResponse.Error($"{successMessage}. Część transakcji nie została zaimportowana", errorsCln));

    return Ok(cApiResponse.SuccessResult(successMessage));
  }

  private async Task<bool> TransactionExistsAsync(int xId) {
    return await _DB_Context.Transactions.AnyAsync(e => e.Id == xId);
  }
}
