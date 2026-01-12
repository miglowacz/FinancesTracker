using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.Models;

public class cAccountRule {
  public int Id { get; set; }

  [Required]
  [MaxLength(200)]
  public string Keyword { get; set; } = string.Empty;

  public int AccountId { get; set; }
  public virtual cAccount Account { get; set; } = null!;

  public bool IsActive { get; set; } = true;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
