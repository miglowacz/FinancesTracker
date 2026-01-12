using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancesTracker.Shared.Models;

public class cTransaction {
  public int Id { get; set; }

  [Required]
  public DateTime Date { get; set; }

  [Required]
  [MaxLength(500)]
  public string Description { get; set; } = string.Empty;

  [Required]
  [Column(TypeName = "decimal(18,2)")]
  public decimal Amount { get; set; }

  public int? AccountId { get; set; }
  public virtual cAccount Account { get; set; } = null!;

  public int? CategoryId { get; set; }
  public virtual cCategory Category { get; set; } = null!;

  public int? SubcategoryId { get; set; }
  public virtual cSubcategory Subcategory { get; set; } = null!;

  public int MonthNumber { get; set; }
  public int Year { get; set; }

  public bool IsInsignificant { get; set; } = false;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }

  // Właściwości pomocnicze dla Blazor (bez mapowania w EF)
  [NotMapped]
  public string MonthName => GetPolishMonthName(MonthNumber);

  [NotMapped]
  public string FormattedAmount => Amount.ToString("C", new System.Globalization.CultureInfo("pl-PL"));

  [NotMapped]
  public string FormattedDate => Date.ToString("dd.MM.yyyy");

  private static string GetPolishMonthName(int month) => month switch {
    1 => "Styczeń",
    2 => "Luty",
    3 => "Marzec",
    4 => "Kwiecień",
    5 => "Maj",
    6 => "Czerwiec",
    7 => "Lipiec",
    8 => "Sierpień",
    9 => "Wrzesień",
    10 => "Październik",
    11 => "Listopad",
    12 => "Grudzień",
    _ => "Nieznany"
  };
}
