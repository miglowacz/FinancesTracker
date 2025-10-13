using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public virtual ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<CategoryRule> CategoryRules { get; set; } = new List<CategoryRule>();
}