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
  public async Task<ActionResult<cApiResponse<List<cCategory_DTO>>>> GetCategories() {

    try {
      var pCln_Categories = await mDBContext.Categories
          .Include(c => c.Subcategories)
          .OrderBy(c => c.Name)
          .ToListAsync();

      var pCln_Categories_DTO = pCln_Categories.Select(MappingService.ToDto).ToList();
      return Ok(cApiResponse<List<cCategory_DTO>>.SuccessResult(pCln_Categories_DTO));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<List<cCategory_DTO>>.Error("Błąd podczas pobierania kategorii", new List<string> { ex.Message }));
    }

  }

  [HttpGet("{id}")]
  public async Task<ActionResult<cApiResponse<cCategory_DTO>>> GetCategory(int xCategoryId) {

    try {
      var category = await mDBContext.Categories
          .Include(c => c.Subcategories)
          .FirstOrDefaultAsync(c => c.Id == xCategoryId);

      if (category == null)
        return NotFound(cApiResponse<cCategory_DTO>.Error("Kategoria nie została znaleziona"));

      return Ok(cApiResponse<cCategory_DTO>.SuccessResult(MappingService.ToDto(category)));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cCategory_DTO>.Error("Błąd podczas pobierania kategorii", new List<string> { ex.Message }));
    }

  }

  [HttpGet("{id}/subcategories")]
  public async Task<ActionResult<cApiResponse<List<cSubcategory_DTO>>>> GetSubcategories(int xCategoryId) {

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

      return Ok(cApiResponse<List<cSubcategory_DTO>>.SuccessResult(subcategories));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<List<cSubcategory_DTO>>.Error("Błąd podczas pobierania podkategorii", new List<string> { ex.Message }));
    }



  }

  [HttpGet("subcategories")]
  public async Task<ActionResult<cApiResponse<List<cSubcategory_DTO>>>> GetAllSubcategories() {
    try {
      var subcategories = await mDBContext.Subcategories
          .OrderBy(s => s.Name)
          .Select(s => new cSubcategory_DTO {
            Id = s.Id,
            Name = s.Name,
            CategoryId = s.CategoryId
          })
          .ToListAsync();

      return Ok(cApiResponse<List<cSubcategory_DTO>>.SuccessResult(subcategories));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<List<cSubcategory_DTO>>.Error(
          "Błąd podczas pobierania wszystkich podkategorii", new List<string> { ex.Message }));
    }
  }
  // --- CATEGORY CRUD ---

  [HttpPost]
  public async Task<ActionResult<cApiResponse<cCategory_DTO>>> CreateCategory([FromBody] cCategory_DTO dto) {
    try {
      var category = new cCategory {
        Name = dto.Name
      };
      mDBContext.Categories.Add(category);
      await mDBContext.SaveChangesAsync();
      return Ok(cApiResponse<cCategory_DTO>.SuccessResult(MappingService.ToDto(category), "Kategoria utworzona"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cCategory_DTO>.Error("Błąd podczas tworzenia kategorii", new List<string> { ex.Message }));
    }
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<cApiResponse<cCategory_DTO>>> UpdateCategory(int id, [FromBody] cCategory_DTO dto) {
    try {
      var category = await mDBContext.Categories.FindAsync(id);
      if (category == null)
        return NotFound(cApiResponse<cCategory_DTO>.Error("Kategoria nie znaleziona"));

      category.Name = dto.Name;
      await mDBContext.SaveChangesAsync();
      return Ok(cApiResponse<cCategory_DTO>.SuccessResult(MappingService.ToDto(category), "Kategoria zaktualizowana"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cCategory_DTO>.Error("Błąd podczas aktualizacji kategorii", new List<string> { ex.Message }));
    }
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<cApiResponse>> DeleteCategory(int id) {
    try {
      var category = await mDBContext.Categories
        .Include(c => c.Subcategories)
        .FirstOrDefaultAsync(c => c.Id == id);
      if (category == null)
        return NotFound(cApiResponse.Error("Kategoria nie znaleziona"));

      // Usuwanie powiązanych podkategorii
      mDBContext.Subcategories.RemoveRange(category.Subcategories);
      mDBContext.Categories.Remove(category);
      await mDBContext.SaveChangesAsync();
      return Ok(cApiResponse.SuccessResult("Kategoria usunięta"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse.Error("Błąd podczas usuwania kategorii", new List<string> { ex.Message }));
    }
  }

  // --- SUBCATEGORY CRUD ---

  [HttpPost("{categoryId}/subcategories")]
  public async Task<ActionResult<cApiResponse<cSubcategory_DTO>>> CreateSubcategory(int categoryId, [FromBody] cSubcategory_DTO dto) {
    try {
      var category = await mDBContext.Categories.FindAsync(categoryId);
      if (category == null)
        return NotFound(cApiResponse<cSubcategory_DTO>.Error("Kategoria nie znaleziona"));

      var subcategory = new cSubcategory {
        Name = dto.Name,
        CategoryId = categoryId
      };
      mDBContext.Subcategories.Add(subcategory);
      await mDBContext.SaveChangesAsync();
      return Ok(cApiResponse<cSubcategory_DTO>.SuccessResult(new cSubcategory_DTO {
        Id = subcategory.Id,
        Name = subcategory.Name,
        CategoryId = subcategory.CategoryId
      }, "Podkategoria utworzona"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cSubcategory_DTO>.Error("Błąd podczas tworzenia podkategorii", new List<string> { ex.Message }));
    }
  }

  [HttpPut("subcategories/{id}")]
  public async Task<ActionResult<cApiResponse<cSubcategory_DTO>>> UpdateSubcategory(int id, [FromBody] cSubcategory_DTO dto) {
    try {
      var subcategory = await mDBContext.Subcategories.FindAsync(id);
      if (subcategory == null)
        return NotFound(cApiResponse<cSubcategory_DTO>.Error("Podkategoria nie znaleziona"));

      subcategory.Name = dto.Name;
      await mDBContext.SaveChangesAsync();
      return Ok(cApiResponse<cSubcategory_DTO>.SuccessResult(new cSubcategory_DTO {
        Id = subcategory.Id,
        Name = subcategory.Name,
        CategoryId = subcategory.CategoryId
      }, "Podkategoria zaktualizowana"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cSubcategory_DTO>.Error("Błąd podczas aktualizacji podkategorii", new List<string> { ex.Message }));
    }
  }

  [HttpDelete("subcategories/{id}")]
  public async Task<ActionResult<cApiResponse>> DeleteSubcategory(int id) {
    try {
      var subcategory = await mDBContext.Subcategories.FindAsync(id);
      if (subcategory == null)
        return NotFound(cApiResponse.Error("Podkategoria nie znaleziona"));

      mDBContext.Subcategories.Remove(subcategory);
      await mDBContext.SaveChangesAsync();
      return Ok(cApiResponse.SuccessResult("Podkategoria usunięta"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse.Error("Błąd podczas usuwania podkategorii", new List<string> { ex.Message }));
    }
  }
}
