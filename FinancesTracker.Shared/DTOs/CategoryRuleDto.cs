using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.DTOs;

public class CategoryRuleDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "S³owo kluczowe jest wymagane")]
    [MaxLength(200, ErrorMessage = "S³owo kluczowe nie mo¿e byæ d³u¿sze ni¿ 200 znaków")]
    public string Keyword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Kategoria jest wymagana")]
    public int CategoryId { get; set; }
    
    [Required(ErrorMessage = "Podkategoria jest wymagana")]
    public int SubcategoryId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // W³aœciwoœci tylko do odczytu
    public string? CategoryName { get; set; }
    public string? SubcategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
}