using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[Route("api/accounts")]
[ApiController]
public class AccountsController : ControllerBase {
  private readonly FinancesTrackerDbContext _dbContext;

  public AccountsController(FinancesTrackerDbContext dbContext) {
    _dbContext = dbContext;
  }

  [HttpGet]
  public async Task<ActionResult<cApiResponse<List<cAccount_DTO>>>> GetAccounts() {
    try {
      var accounts = await _dbContext.Accounts
        .Include(a => a.Transactions)
        .OrderBy(a => a.Name)
        .ToListAsync();

      var accountDtos = accounts.Select(a => new cAccount_DTO {
        Id = a.Id,
        Name = a.Name,
        InitialBalance = a.InitialBalance,
        Currency = a.Currency,
        CntAccountType = (AccountTypeEnum)a.CntAccountType,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        CurrentBalance = a.CurrentBalance,
        AccountTypeDisplay = a.CntAccountType.ToString()
      }).ToList();

      return Ok(cApiResponse<List<cAccount_DTO>>.SuccessResult(accountDtos));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<List<cAccount_DTO>>.Error("Błąd podczas pobierania kont", new List<string> { ex.Message }));
    }
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<cApiResponse<cAccount_DTO>>> GetAccount(int id) {
    try {
      var account = await _dbContext.Accounts
        .Include(a => a.Transactions)
        .FirstOrDefaultAsync(a => a.Id == id);

      if (account == null)
        return NotFound(cApiResponse<cAccount_DTO>.Error("Konto nie zostało znalezione"));

      var accountDto = new cAccount_DTO {
        Id = account.Id,
        Name = account.Name,
        InitialBalance = account.InitialBalance,
        Currency = account.Currency,
        CntAccountType = (AccountTypeEnum)account.CntAccountType,
        IsActive = account.IsActive,
        CreatedAt = account.CreatedAt,
        UpdatedAt = account.UpdatedAt,
        CurrentBalance = account.CurrentBalance,
        AccountTypeDisplay = account.CntAccountType.ToString()
      };

      return Ok(cApiResponse<cAccount_DTO>.SuccessResult(accountDto));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cAccount_DTO>.Error("Błąd podczas pobierania konta", new List<string> { ex.Message }));
    }
  }

  [HttpPost]
  public async Task<ActionResult<cApiResponse<cAccount_DTO>>> CreateAccount([FromBody] cAccount_DTO dto) {
    if (!ModelState.IsValid) {
      var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(cApiResponse<cAccount_DTO>.Error("Dane konta są nieprawidłowe", errors));
    }

    try {
      var account = new cAccount {
        Name = dto.Name,
        InitialBalance = dto.InitialBalance,
        Currency = dto.Currency,
        CntAccountType = (AccountTypeEnum)dto.CntAccountType,
        IsActive = dto.IsActive,
        CreatedAt = DateTime.UtcNow
      };

      _dbContext.Accounts.Add(account);
      await _dbContext.SaveChangesAsync();

      dto.Id = account.Id;
      dto.CreatedAt = account.CreatedAt;
      dto.CurrentBalance = account.InitialBalance;

      return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, cApiResponse<cAccount_DTO>.SuccessResult(dto, "Konto zostało utworzone"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cAccount_DTO>.Error("Błąd podczas tworzenia konta", new List<string> { ex.Message }));
    }
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<cApiResponse<cAccount_DTO>>> UpdateAccount(int id, [FromBody] cAccount_DTO dto) {
    if (id != dto.Id)
      return BadRequest(cApiResponse<cAccount_DTO>.Error("ID konta nie pasuje"));

    if (!ModelState.IsValid) {
      var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
      return BadRequest(cApiResponse<cAccount_DTO>.Error("Dane konta są nieprawidłowe", errors));
    }

    try {
      var account = await _dbContext.Accounts.FindAsync(id);
      if (account == null)
        return NotFound(cApiResponse<cAccount_DTO>.Error("Konto nie zostało znalezione"));

      account.Name = dto.Name;
      account.InitialBalance = dto.InitialBalance;
      account.Currency = dto.Currency;
      account.CntAccountType = (AccountTypeEnum)dto.CntAccountType;
      account.IsActive = dto.IsActive;
      account.UpdatedAt = DateTime.UtcNow;

      await _dbContext.SaveChangesAsync();

      dto.UpdatedAt = account.UpdatedAt;
      dto.CurrentBalance = account.CurrentBalance;

      return Ok(cApiResponse<cAccount_DTO>.SuccessResult(dto, "Konto zostało zaktualizowane"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cAccount_DTO>.Error("Błąd podczas aktualizacji konta", new List<string> { ex.Message }));
    }
  }

  [HttpPatch("{id}/deactivate")]
  public async Task<ActionResult<cApiResponse<cAccount_DTO>>> DeactivateAccount(int id) {
    try {
      var account = await _dbContext.Accounts
        .Include(a => a.Transactions)
        .FirstOrDefaultAsync(a => a.Id == id);

      if (account == null)
        return NotFound(cApiResponse<cAccount_DTO>.Error("Konto nie zostało znalezione"));

      account.IsActive = false;
      account.UpdatedAt = DateTime.UtcNow;

      await _dbContext.SaveChangesAsync();

      var accountDto = new cAccount_DTO {
        Id = account.Id,
        Name = account.Name,
        InitialBalance = account.InitialBalance,
        Currency = account.Currency,
        CntAccountType = (AccountTypeEnum)account.CntAccountType,
        IsActive = account.IsActive,
        CreatedAt = account.CreatedAt,
        UpdatedAt = account.UpdatedAt,
        CurrentBalance = account.CurrentBalance,
        AccountTypeDisplay = account.CntAccountType.ToString()
      };

      return Ok(cApiResponse<cAccount_DTO>.SuccessResult(accountDto, "Konto zostało dezaktywowane"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cAccount_DTO>.Error("Błąd podczas dezaktywacji konta", new List<string> { ex.Message }));
    }
  }
}
