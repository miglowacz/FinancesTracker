using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.Models;

public class CategoryRule
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Keyword { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    public int SubcategoryId { get; set; }
    public virtual Subcategory Subcategory { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}