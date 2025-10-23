using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[Route("api/category")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly FinancesTrackerDbContext _context;

    public CategoriesController(FinancesTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<cCategory_DTO>>>> GetCategories()
    {
        try
        {
            var categories = await _context.Categories
                .Include(c => c.Subcategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var result = categories.Select(MappingService.ToDto).ToList();
            return Ok(ApiResponse<List<cCategory_DTO>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<cCategory_DTO>>.ErrorResult("B³¹d podczas pobierania kategorii", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<cCategory_DTO>>> GetCategory(int id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(ApiResponse<cCategory_DTO>.ErrorResult("Kategoria nie zosta³a znaleziona"));

            return Ok(ApiResponse<cCategory_DTO>.SuccessResult(MappingService.ToDto(category)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<cCategory_DTO>.ErrorResult("B³¹d podczas pobierania kategorii", new List<string> { ex.Message }));
        }
    }

    [HttpGet("{id}/subcategories")]
    public async Task<ActionResult<ApiResponse<List<cSubcategory_DTO>>>> GetSubcategories(int id)
    {
        try
        {
            var subcategories = await _context.Subcategories
                .Where(s => s.CategoryId == id)
                .OrderBy(s => s.Name)
                .Select(s => new cSubcategory_DTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId
                })
                .ToListAsync();

            return Ok(ApiResponse<List<cSubcategory_DTO>>.SuccessResult(subcategories));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<cSubcategory_DTO>>.ErrorResult("B³¹d podczas pobierania podkategorii", new List<string> { ex.Message }));
        }
    }
}