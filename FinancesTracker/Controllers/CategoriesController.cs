using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly FinancesTrackerDbContext _context;

    public CategoriesController(FinancesTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
    {
        try
        {
            var categories = await _context.Categories
                .Include(c => c.Subcategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var result = categories.Select(MappingService.ToDto).ToList();
            return Ok(ApiResponse<List<CategoryDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CategoryDto>>.ErrorResult("B³¹d podczas pobierania kategorii", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(ApiResponse<CategoryDto>.ErrorResult("Kategoria nie zosta³a znaleziona"));

            return Ok(ApiResponse<CategoryDto>.SuccessResult(MappingService.ToDto(category)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult("B³¹d podczas pobierania kategorii", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}/subcategories")]
    public async Task<ActionResult<ApiResponse<List<SubcategoryDto>>>> GetSubcategories(int id)
    {
        try
        {
            var subcategories = await _context.Subcategories
                .Where(s => s.CategoryId == id)
                .OrderBy(s => s.Name)
                .Select(s => new SubcategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId
                })
                .ToListAsync();

            return Ok(ApiResponse<List<SubcategoryDto>>.SuccessResult(subcategories));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<SubcategoryDto>>.ErrorResult("B³¹d podczas pobierania podkategorii", new List<string> { ex.Message }));
        }
    }
}