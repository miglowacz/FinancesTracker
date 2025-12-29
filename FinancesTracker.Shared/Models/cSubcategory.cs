using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.Models;

public class cSubcategory
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    public virtual cCategory Category { get; set; } = null!;
    
    public virtual ICollection<cTransaction> Transactions { get; set; } = new List<cTransaction>();
    public virtual ICollection<cCategoryRule> CategoryRules { get; set; } = new List<cCategoryRule>();
}