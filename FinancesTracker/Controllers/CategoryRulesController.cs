using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<ApiResponse<CategoryRuleDto>>> CreateCategoryRule(CategoryRuleDto ruleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Dane regu³y s¹ nieprawid³owe", errors));
            }

            // SprawdŸ czy kategoria i podkategoria istniej¹
            var category = await _context.Categories.Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == ruleDto.CategoryId);

            if (category == null)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == ruleDto.SubcategoryId))
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana podkategoria nie nale¿y do wybranej kategorii"));

            // SprawdŸ czy regu³a ju¿ istnieje
            var existingRule = await _context.CategoryRules
                .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower());

            if (existingRule != null)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Regu³a dla tego s³owa kluczowego ju¿ istnieje"));

            var rule = MappingService.ToEntity(ruleDto);
            _context.CategoryRules.Add(rule);
            await _context.SaveChangesAsync();

            // Pobierz utworzon¹ regu³ê z relacjami
            var createdRule = await _context.CategoryRules
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .FirstAsync(r => r.Id == rule.Id);

            return CreatedAtAction(nameof(GetCategoryRule),
                new { id = rule.Id },
                ApiResponse<CategoryRuleDto>.SuccessResult(MappingService.ToDto(createdRule), "Regu³a zosta³a utworzona"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryRuleDto>.ErrorResult("B³¹d podczas tworzenia regu³y", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryRuleDto>>> UpdateCategoryRule(int id, CategoryRuleDto ruleDto)
    {
        try
        {
            if (id != ruleDto.Id)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("ID regu³y nie pasuje"));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Dane regu³y s¹ nieprawid³owe", errors));
            }

            var existingRule = await _context.CategoryRules.FindAsync(id);
            if (existingRule == null)
                return NotFound(ApiResponse<CategoryRuleDto>.ErrorResult("Regu³a nie zosta³a znaleziona"));

            // SprawdŸ czy kategoria i podkategoria istniej¹
            var category = await _context.Categories.Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == ruleDto.CategoryId);

            if (category == null)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana kategoria nie istnieje"));

            if (!category.Subcategories.Any(s => s.Id == ruleDto.SubcategoryId))
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Wybrana podkategoria nie nale¿y do wybranej kategorii"));

            // SprawdŸ czy regu³a ju¿ istnieje (poza aktualn¹)
            var duplicateRule = await _context.CategoryRules
                .FirstOrDefaultAsync(r => r.Keyword.ToLower() == ruleDto.Keyword.ToLower() && r.Id != id);

            if (duplicateRule != null)
                return BadRequest(ApiResponse<CategoryRuleDto>.ErrorResult("Regu³a dla tego s³owa kluczowego ju¿ istnieje"));

            // Aktualizuj w³aœciwoœci
            existingRule.Keyword = ruleDto.Keyword.ToLowerInvariant();
            existingRule.CategoryId = ruleDto.CategoryId;
            existingRule.SubcategoryId = ruleDto.SubcategoryId;
            existingRule.IsActive = ruleDto.IsActive;

            await _context.SaveChangesAsync();

            // Pobierz zaktualizowan¹ regu³ê z relacjami
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

    [HttpPost("categorize")]
    public async Task<ActionResult<ApiResponse<object>>> CategorizeTransaction([FromBody] string description)
    {
        try
        {
            var activeRules = await _context.CategoryRules
                .Where(r => r.IsActive)
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .ToListAsync();

            var matchedRule = activeRules
                .FirstOrDefault(r => description.ToLowerInvariant().Contains(r.Keyword));

            if (matchedRule == null)
            {
                return Ok(ApiResponse<object>.SuccessResult(null, "Nie znaleziono pasuj¹cej regu³y"));
            }

            var result = new
            {
                CategoryId = matchedRule.CategoryId,
                CategoryName = matchedRule.Category.Name,
                SubcategoryId = matchedRule.SubcategoryId,
                SubcategoryName = matchedRule.Subcategory.Name,
                MatchedKeyword = matchedRule.Keyword
            };

            return Ok(ApiResponse<object>.SuccessResult(result, "Znaleziono pasuj¹c¹ regu³ê"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("B³¹d podczas kategoryzacji", new List<string> { ex.Message }));
        }
    }
}