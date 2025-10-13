using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly FinancesTrackerDbContext _context;

    public TransactionsController(FinancesTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetTransactions([FromQuery] TransactionFilterDto filter)
    {
        try
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .AsQueryable();

            // Filtrowanie
            if (filter.Year.HasValue)
                query = query.Where(t => t.Year == filter.Year.Value);

            if (filter.Month.HasValue)
                query = query.Where(t => t.MonthNumber == filter.Month.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

            if (filter.SubcategoryId.HasValue)
                query = query.Where(t => t.SubcategoryId == filter.SubcategoryId.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(t => t.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(t => t.Date <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(t => t.Description.Contains(filter.SearchTerm));

            if (!string.IsNullOrEmpty(filter.BankName))
                query = query.Where(t => t.BankName == filter.BankName);

            // Sortowanie
            query = filter.SortBy?.ToLower() switch
            {
                "description" => filter.SortDescending 
                    ? query.OrderByDescending(t => t.Description)
                    : query.OrderBy(t => t.Description),
                "amount" => filter.SortDescending
                    ? query.OrderByDescending(t => t.Amount)
                    : query.OrderBy(t => t.Amount),
                "category" => filter.SortDescending
                    ? query.OrderByDescending(t => t.Category.Name)
                    : query.OrderBy(t => t.Category.Name),
                _ => filter.SortDescending
                    ? query.OrderByDescending(t => t.Date)
                    : query.OrderBy(t => t.Date)
            };

            // Paginacja
            var totalCount = await query.CountAsync();
            var transactions = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new PagedResult<TransactionDto>
            {
                Items = transactions.Select(MappingService.ToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return Ok(ApiResponse<PagedResult<TransactionDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<TransactionDto>>.ErrorResult("B³¹d podczas pobierania transakcji", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction(int id)
    {
        try
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound(ApiResponse<TransactionDto>.ErrorResult("Transakcja nie zosta³a znaleziona"));

            return Ok(ApiResponse<TransactionDto>.SuccessResult(MappingService.ToDto(transaction)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TransactionDto>.ErrorResult("B³¹d podczas pobierania transakcji", new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction(TransactionDto transactionDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransactionDto>.ErrorResult("Dane transakcji s¹ nieprawid³owe", errors));
            }

            // SprawdŸ czy kategoria i podkategoria istniej¹
            var category = await _context.Categories.Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == transactionDto.CategoryId);

            if (category == null)
                return BadRequest(ApiResponse<TransactionDto>.ErrorResult("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == transactionDto.SubcategoryId))
                return BadRequest(ApiResponse<TransactionDto>.ErrorResult("Wybrana podkategoria nie nale¿y do wybranej kategorii"));

            var transaction = MappingService.ToEntity(transactionDto);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Pobierz utworzon¹ transakcjê z relacjami
            var createdTransaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .FirstAsync(t => t.Id == transaction.Id);

            return CreatedAtAction(nameof(GetTransaction), 
                new { id = transaction.Id }, 
                ApiResponse<TransactionDto>.SuccessResult(MappingService.ToDto(createdTransaction), "Transakcja zosta³a utworzona"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TransactionDto>.ErrorResult("B³¹d podczas tworzenia transakcji", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> UpdateTransaction(int id, TransactionDto transactionDto)
    {
        try
        {
            if (id != transactionDto.Id)
                return BadRequest(ApiResponse<TransactionDto>.ErrorResult("ID transakcji nie pasuje"));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransactionDto>.ErrorResult("Dane transakcji s¹ nieprawid³owe", errors));
            }

            var existingTransaction = await _context.Transactions.FindAsync(id);
            if (existingTransaction == null)
                return NotFound(ApiResponse<TransactionDto>.ErrorResult("Transakcja nie zosta³a znaleziona"));

            // SprawdŸ czy kategoria i podkategoria istniej¹
            var category = await _context.Categories.Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == transactionDto.CategoryId);

            if (category == null)
                return BadRequest(ApiResponse<TransactionDto>.ErrorResult("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == transactionDto.SubcategoryId))
                return BadRequest(ApiResponse<TransactionDto>.ErrorResult("Wybrana podkategoria nie nale¿y do wybranej kategorii"));

            // Aktualizuj w³aœciwoœci
            existingTransaction.Date = transactionDto.Date;
            existingTransaction.Description = transactionDto.Description;
            existingTransaction.Amount = transactionDto.Amount;
            existingTransaction.CategoryId = transactionDto.CategoryId;
            existingTransaction.SubcategoryId = transactionDto.SubcategoryId;
            existingTransaction.BankName = transactionDto.BankName;
            existingTransaction.MonthNumber = transactionDto.Date.Month;
            existingTransaction.Year = transactionDto.Date.Year;
            existingTransaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Pobierz zaktualizowan¹ transakcjê z relacjami
            var updatedTransaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Subcategory)
                .FirstAsync(t => t.Id == id);

            return Ok(ApiResponse<TransactionDto>.SuccessResult(MappingService.ToDto(updatedTransaction), "Transakcja zosta³a zaktualizowana"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TransactionExistsAsync(id))
                return NotFound(ApiResponse<TransactionDto>.ErrorResult("Transakcja nie zosta³a znaleziona"));
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<TransactionDto>.ErrorResult("B³¹d podczas aktualizacji transakcji", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteTransaction(int id)
    {
        try
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound(ApiResponse.Error("Transakcja nie zosta³a znaleziona"));

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Success("Transakcja zosta³a usuniêta"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Error("B³¹d podczas usuwania transakcji", new List<string> { ex.Message }));
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<object>>> GetSummary(int year, int? month = null)
    {
        try
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Year == year);

            if (month.HasValue)
                query = query.Where(t => t.MonthNumber == month.Value);

            var summary = await query
                .GroupBy(t => new { t.CategoryId, t.Category.Name })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalAmount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                    Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
                })
                .OrderByDescending(s => Math.Abs(s.TotalAmount))
                .ToListAsync();

            var totalIncome = summary.Sum(s => s.Income);
            var totalExpenses = Math.Abs(summary.Sum(s => s.Expenses));
            var balance = totalIncome - totalExpenses;

            var result = new
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Balance = balance,
                Categories = summary
            };

            return Ok(ApiResponse<object>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("B³¹d podczas pobierania podsumowania", new List<string> { ex.Message }));
        }
    }

    private async Task<bool> TransactionExistsAsync(int id)
    {
        return await _context.Transactions.AnyAsync(e => e.Id == id);
    }
}