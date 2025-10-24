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
            return StatusCode(500, ApiResponse<List<CategoryRuleDto>>.ErrorResult("B³¹d podczas pobierania regu³", new List<string> { ex.Message }));
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
                return NotFound(ApiResponse<CategoryRuleDto>.ErrorResult("Regu³a nie zosta³a znaleziona"));

            return Ok(ApiResponse<CategoryRuleDto>.SuccessResult(MappingService.ToDto(rule)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryRuleDto>.ErrorResult("B³¹d podczas pobierania regu³y", new List<string> { ex.Message }));
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
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == ruleDto.SubcategoryId))
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana podkategoria nie nale¿y do wybranej kategorii"));

            var existingRule = await _context.CategoryRules
                .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower());

            if (existingRule != null)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Regu³a dla tego s³owa kluczowego ju¿ istnieje"));

            var rule = MappingService.ToEntity(ruleDto);
            _context.CategoryRules.Add(rule);
            await _context.SaveChangesAsync();

            var createdRule = await _context.CategoryRules
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .FirstAsync(r => r.Id == rule.Id);

            return Ok(ApiResponse<CategoryRuleDto>.SuccessResult(MappingService.ToDto(createdRule), "Regu³a zosta³a utworzona"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryRuleDto>.ErrorResult("B³¹d podczas tworzenia regu³y", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryRuleDto>>> UpdateCategoryRule(int id, [FromBody] CategoryRuleDto ruleDto)
    {
        try
        {
            if (id != ruleDto.Id)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("ID regu³y nie pasuje"));

            var existingRule = await _context.CategoryRules.FindAsync(id);
            if (existingRule == null)
                return NotFound(ApiResponse<CategoryRuleDto>.ErrorResult("Regu³a nie zosta³a znaleziona"));

            var category = await _context.Categories.Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == ruleDto.CategoryId);

            if (category == null)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == ruleDto.SubcategoryId))
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana podkategoria nie nale¿y do wybranej kategorii"));

            var duplicateRule = await _context.CategoryRules
                .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower() && r.Id != id);

            if (duplicateRule != null)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Regu³a dla tego s³owa kluczowego ju¿ istnieje"));

            existingRule.Keyword = ruleDto.Keyword.ToLowerInvariant();
            existingRule.CategoryId = ruleDto.CategoryId;
            existingRule.SubcategoryId = ruleDto.SubcategoryId;
            existingRule.IsActive = ruleDto.IsActive;

            await _context.SaveChangesAsync();

            var updatedRule = await _context.CategoryRules
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .FirstAsync(r => r.Id == id);

            return Ok(ApiResponse<CategoryRuleDto>.SuccessResult(MappingService.ToDto(updatedRule), "Regu³a zosta³a zaktualizowana"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryRuleDto>.ErrorResult("B³¹d podczas aktualizacji regu³y", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCategoryRule(int id)
    {
        try
        {
            var rule = await _context.CategoryRules.FindAsync(id);
            if (rule == null)
                return NotFound(ApiResponse.Error("Regu³a nie zosta³a znaleziona"));

            _context.CategoryRules.Remove(rule);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Success("Regu³a zosta³a usuniêta"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Error("B³¹d podczas usuwania regu³y", new List<string> { ex.Message }));
        }
    }
}