using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.DTOs;

public class cTransaction_DTO {
  public int Id { get; set; }

  [Required(ErrorMessage = "Data jest wymagana")]
  public DateTime Date { get; set; }

  [Required(ErrorMessage = "Opis jest wymagany")]
  [MaxLength(500, ErrorMessage = "Opis nie może być dłuższy niż 500 znaków")]
  public string Description { get; set; } = string.Empty;

  [Required(ErrorMessage = "Kwota jest wymagana")]
  [Range(-999999.99, 999999.99, ErrorMessage = "Kwota musi być między -999999.99 a 999999.99")]
  public decimal Amount { get; set; }

  [Required(ErrorMessage = "Konto jest wymagane")]
  public int AccountId { get; set; }

  public int? CategoryId { get; set; }
  public int? SubcategoryId { get; set; }

  public bool IsInsignificant { get; set; } = false;

  public bool IsTransfer { get; set; } = false;
  public int? RelatedTransactionId { get; set; }
  public int? TargetAccountId { get; set; }
  public string? RelatedAccountName { get; set; }

  // Właściwości tylko do odczytu
  public string? CategoryName { get; set; }
  public string? SubcategoryName { get; set; }
  public string? AccountName { get; set; }
  public int MonthNumber { get; set; }
  public int Year { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}
