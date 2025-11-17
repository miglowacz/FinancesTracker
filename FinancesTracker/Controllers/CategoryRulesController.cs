using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/categoryrules")]
public class CategoryRulesController : ControllerBase
{
    private readonly FinancesTrackerDbContext _context;

    public CategoryRulesController(FinancesTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryRuleDto>>>> GetCategoryRules()
    {
        try
        {
            var rules = await _context.CategoryRules
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .OrderBy(r => r.Keyword)
                .ToListAsync();

            var result = rules.Select(MappingService.ToDto).ToList();
            return Ok(ApiResponse<List<CategoryRuleDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CategoryRuleDto>>.Error("Błąd podczas pobierania reguł", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryRuleDto>>> GetCategoryRule(int id)
    {
        try
        {
            var rule = await _context.CategoryRules
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null)
                return NotFound(ApiResponse<CategoryRuleDto>.Error("Reguła nie została znaleziona"));

            return Ok(ApiResponse<CategoryRuleDto>.SuccessResult(MappingService.ToDto(rule)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryRuleDto>.Error("Błąd podczas pobierania reguły", new List<string> { ex.Message }));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryRuleDto>>> CreateCategoryRule([FromBody] CategoryRuleDto ruleDto)
    {
        try
        {
            var category = await _context.Categories.Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == ruleDto.CategoryId);

            if (category == null)
                return BadRequest(ApiResponse<CategoryRuleDto>.Error("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == ruleDto.SubcategoryId))
                return BadRequest(ApiResponse<CategoryRuleDto>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

            var existingRule = await _context.CategoryRules
                .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower());

            if (existingRule != null)
                return BadRequest(ApiResponse<CategoryRuleDto>.Error("Reguła dla tego słowa kluczowego już istnieje"));

            var rule = MappingService.ToEntity(ruleDto);
            _context.CategoryRules.Add(rule);
            await _context.SaveChangesAsync();

            var createdRule = await _context.CategoryRules
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .FirstAsync(r => r.Id == rule.Id);

            return Ok(ApiResponse<CategoryRuleDto>.SuccessResult(MappingService.ToDto(createdRule), "Reguła została utworzona"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryRuleDto>.Error("Błąd podczas tworzenia reguły", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryRuleDto>>> UpdateCategoryRule(int id, [FromBody] CategoryRuleDto ruleDto)
    {
        try
        {
            if (id != ruleDto.Id)
                return BadRequest(ApiResponse<CategoryRuleDto>.Error("ID reguły nie pasuje"));

            var existingRule = await _context.CategoryRules.FindAsync(id);
            if (existingRule == null)
                return NotFound(ApiResponse<CategoryRuleDto>.Error("Reguła nie została znaleziona"));

            var category = await _context.Categories.Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == ruleDto.CategoryId);

            if (category == null)
                return BadRequest(ApiResponse<CategoryRuleDto>.Error("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == ruleDto.SubcategoryId))
                return BadRequest(ApiResponse<CategoryRuleDto>.Error("Wybrana podkategoria nie należy do wybranej kategorii"));

            var duplicateRule = await _context.CategoryRules
                .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower() && r.Id != id);

            if (duplicateRule != null)
                return BadRequest(ApiResponse<CategoryRuleDto>.Error("Reguła dla tego słowa kluczowego już istnieje"));

            existingRule.Keyword = ruleDto.Keyword.ToLowerInvariant();
            existingRule.CategoryId = ruleDto.CategoryId;
            existingRule.SubcategoryId = ruleDto.SubcategoryId;
            existingRule.IsActive = ruleDto.IsActive;

            await _context.SaveChangesAsync();

            var updatedRule = await _context.CategoryRules
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .FirstAsync(r => r.Id == id);

            return Ok(ApiResponse<CategoryRuleDto>.SuccessResult(MappingService.ToDto(updatedRule), "Reguła została zaktualizowana"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryRuleDto>.Error("Błąd podczas aktualizacji reguły", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCategoryRule(int id)
    {
        try
        {
            var rule = await _context.CategoryRules.FindAsync(id);
            if (rule == null)
                return NotFound(ApiResponse.Error("Reguła nie została znaleziona"));

            _context.CategoryRules.Remove(rule);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResult("Reguła została usunięta"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Error("Błąd podczas usuwania reguły", new List<string> { ex.Message }));
        }
    }
}
