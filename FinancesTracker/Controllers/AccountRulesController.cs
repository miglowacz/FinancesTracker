using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/accountrules")]
public class AccountRulesController : ControllerBase {
  private readonly FinancesTrackerDbContext _context;
  private readonly cAccountRuleService _accountRuleService;

  public AccountRulesController(FinancesTrackerDbContext context, cAccountRuleService accountRuleService) {
    _context = context;
    _accountRuleService = accountRuleService;
  }

  [HttpGet]
  public async Task<ActionResult<cApiResponse<List<cAccountRule_DTO>>>> GetAccountRules() {
    try {
      var rules = await _context.AccountRules
        .Include(r => r.Account)
        .OrderBy(r => r.Keyword)
        .ToListAsync();

      var result = rules.Select(r => new cAccountRule_DTO {
        Id = r.Id,
        Keyword = r.Keyword,
        AccountId = r.AccountId,
        AccountName = r.Account?.Name,
        IsActive = r.IsActive
      }).ToList();

      return Ok(cApiResponse<List<cAccountRule_DTO>>.SuccessResult(result));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<List<cAccountRule_DTO>>.Error("Błąd podczas pobierania reguł", new List<string> { ex.Message }));
    }
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<cApiResponse<cAccountRule_DTO>>> GetAccountRule(int id) {
    try {
      var rule = await _context.AccountRules
        .Include(r => r.Account)
        .FirstOrDefaultAsync(r => r.Id == id);

      if (rule == null)
        return NotFound(cApiResponse<cAccountRule_DTO>.Error("Reguła nie została znaleziona"));

      var dto = new cAccountRule_DTO {
        Id = rule.Id,
        Keyword = rule.Keyword,
        AccountId = rule.AccountId,
        AccountName = rule.Account?.Name,
        IsActive = rule.IsActive
      };

      return Ok(cApiResponse<cAccountRule_DTO>.SuccessResult(dto));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cAccountRule_DTO>.Error("Błąd podczas pobierania reguły", new List<string> { ex.Message }));
    }
  }

  [HttpPost]
  public async Task<ActionResult<cApiResponse<cAccountRule_DTO>>> CreateAccountRule([FromBody] cAccountRule_DTO ruleDto) {
    try {
      var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == ruleDto.AccountId);
      if (account == null)
        return BadRequest(cApiResponse<cAccountRule_DTO>.Error("Wybrane konto nie istnieje"));

      var existingRule = await _context.AccountRules
        .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower());
      if (existingRule != null)
        return BadRequest(cApiResponse<cAccountRule_DTO>.Error("Reguła dla tego słowa kluczowego już istnieje"));

      var rule = new cAccountRule {
        Keyword = ruleDto.Keyword.ToLowerInvariant(),
        AccountId = ruleDto.AccountId,
        IsActive = ruleDto.IsActive
      };

      _context.AccountRules.Add(rule);
      await _context.SaveChangesAsync();
      _accountRuleService.InvalidateCache();

      var createdRule = await _context.AccountRules
        .Include(r => r.Account)
        .FirstAsync(r => r.Id == rule.Id);

      var resultDto = new cAccountRule_DTO {
        Id = createdRule.Id,
        Keyword = createdRule.Keyword,
        AccountId = createdRule.AccountId,
        AccountName = createdRule.Account?.Name,
        IsActive = createdRule.IsActive
      };

      return Ok(cApiResponse<cAccountRule_DTO>.SuccessResult(resultDto, "Reguła została utworzona"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cAccountRule_DTO>.Error("Błąd podczas tworzenia reguły", new List<string> { ex.Message }));
    }
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<cApiResponse<cAccountRule_DTO>>> UpdateAccountRule(int id, [FromBody] cAccountRule_DTO ruleDto) {
    try {
      if (id != ruleDto.Id)
        return BadRequest(cApiResponse<cAccountRule_DTO>.Error("ID reguły nie pasuje"));

      var existingRule = await _context.AccountRules.FindAsync(id);
      if (existingRule == null)
        return NotFound(cApiResponse<cAccountRule_DTO>.Error("Reguła nie została znaleziona"));

      var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == ruleDto.AccountId);
      if (account == null)
        return BadRequest(cApiResponse<cAccountRule_DTO>.Error("Wybrane konto nie istnieje"));

      var duplicateRule = await _context.AccountRules
        .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower() && r.Id != id);
      if (duplicateRule != null)
        return BadRequest(cApiResponse<cAccountRule_DTO>.Error("Reguła dla tego słowa kluczowego już istnieje"));

      existingRule.Keyword = ruleDto.Keyword.ToLowerInvariant();
      existingRule.AccountId = ruleDto.AccountId;
      existingRule.IsActive = ruleDto.IsActive;

      await _context.SaveChangesAsync();
      _accountRuleService.InvalidateCache();

      var updatedRule = await _context.AccountRules
        .Include(r => r.Account)
        .FirstAsync(r => r.Id == id);

      var resultDto = new cAccountRule_DTO {
        Id = updatedRule.Id,
        Keyword = updatedRule.Keyword,
        AccountId = updatedRule.AccountId,
        AccountName = updatedRule.Account?.Name,
        IsActive = updatedRule.IsActive
      };

      return Ok(cApiResponse<cAccountRule_DTO>.SuccessResult(resultDto, "Reguła została zaktualizowana"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cAccountRule_DTO>.Error("Błąd podczas aktualizacji reguły", new List<string> { ex.Message }));
    }
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<cApiResponse>> DeleteAccountRule(int id) {
    try {
      var rule = await _context.AccountRules.FindAsync(id);
      if (rule == null)
        return NotFound(cApiResponse.Error("Reguła nie została znaleziona"));

      _context.AccountRules.Remove(rule);
      await _context.SaveChangesAsync();
      _accountRuleService.InvalidateCache();

      return Ok(cApiResponse.SuccessResult("Reguła została usunięta"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse.Error("Błąd podczas usuwania reguły", new List<string> { ex.Message }));
    }
  }
}
