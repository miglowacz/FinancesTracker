using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FinancesTracker.Shared.DTOs;

namespace FinancesTracker.Shared.Models;

public class cAccount {
  public int Id { get; set; }

  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = string.Empty;
 
  [MaxLength(100)]
  public string? BankName { get; set; }

  [MaxLength(50)]
  public string? ImportIdentifier { get; set; }

  [Required]
  [Column(TypeName = "decimal(18,2)")]
  public decimal InitialBalance { get; set; }

  [Required]
  [MaxLength(3)]
  public string Currency { get; set; } = "PLN";

  [Required]
  public AccountTypeEnum CntAccountType { get; set; }

  [Required]
  public bool IsActive { get; set; } = true;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }

  // Relacja jeden-do-wielu
  public virtual ICollection<cTransaction> Transactions { get; set; } = new List<cTransaction>();
  
  // Relacja jeden-do-wielu z AccountRules
  public virtual ICollection<cAccountRule> AccountRules { get; set; } = new List<cAccountRule>();

  // Właściwość obliczeniowa - saldo na dzień dzisiejszy
  [NotMapped]
  public decimal CurrentBalance => CalculateCurrentBalance();

  private decimal CalculateCurrentBalance() {
    decimal income = Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
    decimal expenses = Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
    return InitialBalance + income + expenses;
  }

  [NotMapped]
  public string FormattedCurrency => $"{Currency}";

  [NotMapped]
  public string FormattedCurrentBalance => CurrentBalance.ToString("C", new System.Globalization.CultureInfo("pl-PL"));
}
