using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.Models;

public class cCategory {

  public int Id { get; set; }

  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = string.Empty;

  public virtual ICollection<cSubcategory> Subcategories { get; set; } = new List<cSubcategory>();
  public virtual ICollection<cTransaction> Transactions { get; set; } = new List<cTransaction>();
  public virtual ICollection<cCategoryRule> CategoryRules { get; set; } = new List<cCategoryRule>();

}