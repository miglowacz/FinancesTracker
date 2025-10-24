using FinancesTracker.Data;
using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancesTracker.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoriesController : ControllerBase {
  private readonly FinancesTrackerDbContext mDBContext;

  public CategoriesController(FinancesTrackerDbContext xDBContext) {

    mDBContext = xDBContext;

  }

  [HttpGet]
  public async Task<ActionResult<ApiResponse<List<cCategory_DTO>>>> GetCategories() {

    try {
      var pCln_Categories = await mDBContext.Categories
          .Include(c => c.Subcategories)
          .OrderBy(c => c.Name)
          .ToListAsync();

      var pCln_Categories_DTO = pCln_Categories.Select(MappingService.ToDto).ToList();
      return Ok(ApiResponse<List<cCategory_DTO>>.SuccessResult(pCln_Categories_DTO));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<List<cCategory_DTO>>.ErrorResult("B³¹d podczas pobierania kategorii", new List<string> { ex.Message }));
    }

  }

  [HttpGet("{id}")]
  public async Task<ActionResult<ApiResponse<cCategory_DTO>>> GetCategory(int xCategoryId) {

    try {
      var category = await mDBContext.Categories
          .Include(c => c.Subcategories)
          .FirstOrDefaultAsync(c => c.Id == xCategoryId);

      if (category == null)
        return NotFound(ApiResponse<cCategory_DTO>.ErrorResult("Kategoria nie zosta³a znaleziona"));

      return Ok(ApiResponse<cCategory_DTO>.SuccessResult(MappingService.ToDto(category)));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<cCategory_DTO>.ErrorResult("B³¹d podczas pobierania kategorii", new List<string> { ex.Message }));
    }

  }

  [HttpGet("{id}/subcategories")]
  public async Task<ActionResult<ApiResponse<List<cSubcategory_DTO>>>> GetSubcategories(int xCategoryId) {

    try {
      var subcategories = await mDBContext.Subcategories
          .Where(s => s.CategoryId == xCategoryId)
          .OrderBy(s => s.Name)
          .Select(s => new cSubcategory_DTO {
            Id = s.Id,
            Name = s.Name,
            CategoryId = s.CategoryId
          })
          .ToListAsync();

      return Ok(ApiResponse<List<cSubcategory_DTO>>.SuccessResult(subcategories));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<List<cSubcategory_DTO>>.ErrorResult("B³¹d podczas pobierania podkategorii", new List<string> { ex.Message }));
    }



  }

  [HttpGet("subcategories")]
  public async Task<ActionResult<ApiResponse<List<cSubcategory_DTO>>>> GetAllSubcategories() {
    try {
      var subcategories = await mDBContext.Subcategories
          .OrderBy(s => s.Name)
          .Select(s => new cSubcategory_DTO {
            Id = s.Id,
            Name = s.Name,
            CategoryId = s.CategoryId
          })
          .ToListAsync();

      return Ok(ApiResponse<List<cSubcategory_DTO>>.SuccessResult(subcategories));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<List<cSubcategory_DTO>>.ErrorResult(
          "B³¹d podczas pobierania wszystkich podkategorii", new List<string> { ex.Message }));
    }
  }
  // --- CATEGORY CRUD ---

  [HttpPost]
  public async Task<ActionResult<ApiResponse<cCategory_DTO>>> CreateCategory([FromBody] cCategory_DTO dto) {
    try {
      var category = new cCategory {
        Name = dto.Name
      };
      mDBContext.Categories.Add(category);
      await mDBContext.SaveChangesAsync();
      return Ok(ApiResponse<cCategory_DTO>.SuccessResult(MappingService.ToDto(category), "Kategoria utworzona"));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<cCategory_DTO>.ErrorResult("B³¹d podczas tworzenia kategorii", new List<string> { ex.Message }));
    }
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<ApiResponse<cCategory_DTO>>> UpdateCategory(int id, [FromBody] cCategory_DTO dto) {
    try {
      var category = await mDBContext.Categories.FindAsync(id);
      if (category == null)
        return NotFound(ApiResponse<cCategory_DTO>.ErrorResult("Kategoria nie znaleziona"));

      category.Name = dto.Name;
      await mDBContext.SaveChangesAsync();
      return Ok(ApiResponse<cCategory_DTO>.SuccessResult(MappingService.ToDto(category), "Kategoria zaktualizowana"));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<cCategory_DTO>.ErrorResult("B³¹d podczas aktualizacji kategorii", new List<string> { ex.Message }));
    }
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<ApiResponse>> DeleteCategory(int id) {
    try {
      var category = await mDBContext.Categories
        .Include(c => c.Subcategories)
        .FirstOrDefaultAsync(c => c.Id == id);
      if (category == null)
        return NotFound(ApiResponse.Error("Kategoria nie znaleziona"));

      // Usuwanie powi¹zanych podkategorii
      mDBContext.Subcategories.RemoveRange(category.Subcategories);
      mDBContext.Categories.Remove(category);
      await mDBContext.SaveChangesAsync();
      return Ok(ApiResponse.Success("Kategoria usuniêta"));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse.Error("B³¹d podczas usuwania kategorii", new List<string> { ex.Message }));
    }
  }

  // --- SUBCATEGORY CRUD ---

  [HttpPost("{categoryId}/subcategories")]
  public async Task<ActionResult<ApiResponse<cSubcategory_DTO>>> CreateSubcategory(int categoryId, [FromBody] cSubcategory_DTO dto) {
    try {
      var category = await mDBContext.Categories.FindAsync(categoryId);
      if (category == null)
        return NotFound(ApiResponse<cSubcategory_DTO>.ErrorResult("Kategoria nie znaleziona"));

      var subcategory = new cSubcategory {
        Name = dto.Name,
        CategoryId = categoryId
      };
      mDBContext.Subcategories.Add(subcategory);
      await mDBContext.SaveChangesAsync();
      return Ok(ApiResponse<cSubcategory_DTO>.SuccessResult(new cSubcategory_DTO {
        Id = subcategory.Id,
        Name = subcategory.Name,
        CategoryId = subcategory.CategoryId
      }, "Podkategoria utworzona"));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<cSubcategory_DTO>.ErrorResult("B³¹d podczas tworzenia podkategorii", new List<string> { ex.Message }));
    }
  }

  [HttpPut("subcategories/{id}")]
  public async Task<ActionResult<ApiResponse<cSubcategory_DTO>>> UpdateSubcategory(int id, [FromBody] cSubcategory_DTO dto) {
    try {
      var subcategory = await mDBContext.Subcategories.FindAsync(id);
      if (subcategory == null)
        return NotFound(ApiResponse<cSubcategory_DTO>.ErrorResult("Podkategoria nie znaleziona"));

      subcategory.Name = dto.Name;
      await mDBContext.SaveChangesAsync();
      return Ok(ApiResponse<cSubcategory_DTO>.SuccessResult(new cSubcategory_DTO {
        Id = subcategory.Id,
        Name = subcategory.Name,
        CategoryId = subcategory.CategoryId
      }, "Podkategoria zaktualizowana"));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse<cSubcategory_DTO>.ErrorResult("B³¹d podczas aktualizacji podkategorii", new List<string> { ex.Message }));
    }
  }

  [HttpDelete("subcategories/{id}")]
  public async Task<ActionResult<ApiResponse>> DeleteSubcategory(int id) {
    try {
      var subcategory = await mDBContext.Subcategories.FindAsync(id);
      if (subcategory == null)
        return NotFound(ApiResponse.Error("Podkategoria nie znaleziona"));

      mDBContext.Subcategories.Remove(subcategory);
      await mDBContext.SaveChangesAsync();
      return Ok(ApiResponse.Success("Podkategoria usuniêta"));
    } catch (Exception ex) {
      return StatusCode(500, ApiResponse.Error("B³¹d podczas usuwania podkategorii", new List<string> { ex.Message }));
    }
  }
}