using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.Models;

public class Subcategory
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<CategoryRule> CategoryRules { get; set; } = new List<CategoryRule>();
}