using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.DTOs;

public enum AccountTypeEnum {
  [Display(Name = "Konto osobiste")]
  Personal = 0,

  [Display(Name = "Konto oszczędnościowe")]
  Savings = 1,

  [Display(Name = "Gotówka / Portfel")]
  Cash = 2,

  [Display(Name = "Karta kredytowa")]
  CreditCard = 3,

  [Display(Name = "Inwestycje")]
  Investment = 4
}

public class cAccount_DTO {
  public int Id { get; set; }

  [Required(ErrorMessage = "Nazwa konta jest wymagana")]
  [MaxLength(100, ErrorMessage = "Nazwa konta nie może być dłuższa niż 100 znaków")]
  public string Name { get; set; } = string.Empty;

  [MaxLength(100, ErrorMessage = "Nazwa banku nie może być dłuższa niż 100 znaków")]
  public string? BankName { get; set; }

  [Required(ErrorMessage = "Saldo początkowe jest wymagane")]
  [Range(-999999.99, 999999.99, ErrorMessage = "Saldo musi być między -999999.99 a 999999.99")]
  public decimal InitialBalance { get; set; }

  [Required]
  [MaxLength(3)]
  public string Currency { get; set; } = "PLN";

  [Required(ErrorMessage = "Typ konta jest wymagany")]
  public AccountTypeEnum CntAccountType { get; set; }

  [Required]
  public bool IsActive { get; set; } = true;

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }

  public decimal CurrentBalance { get; set; }
  public string? AccountTypeDisplay { get; set; }
}
