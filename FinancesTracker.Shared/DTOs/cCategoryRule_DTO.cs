using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.DTOs;

public class cCategoryRule_DTO
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Słowo kluczowe jest wymagane")]
    [MaxLength(200, ErrorMessage = "Słowo kluczowe nie może być dłuższe niż 200 znaków")]
    public string Keyword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Kategoria jest wymagana")]
    public int CategoryId { get; set; }
    
    [Required(ErrorMessage = "Podkategoria jest wymagana")]
    public int SubcategoryId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Właściwości tylko do odczytu
    public string? CategoryName { get; set; }
    public string? SubcategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
}
