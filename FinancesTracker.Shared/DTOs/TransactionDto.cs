using System.ComponentModel.DataAnnotations;

namespace FinancesTracker.Shared.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Data jest wymagana")]
    public DateTime Date { get; set; }
    
    [Required(ErrorMessage = "Opis jest wymagany")]
    [MaxLength(500, ErrorMessage = "Opis nie może być dłuższy niż 500 znaków")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Kwota jest wymagana")]
    [Range(-999999.99, 999999.99, ErrorMessage = "Kwota musi być między -999999.99 a 999999.99")]
    public decimal Amount { get; set; }
    
    //[Required(ErrorMessage = "Kategoria jest wymagana")]
    public int? CategoryId { get; set; }
    
    //[Required(ErrorMessage = "Podkategoria jest wymagana")]
    public int? SubcategoryId { get; set; }
    
    [MaxLength(50)]
    public string? BankName { get; set; }
    
    public bool IsInsignificant { get; set; } = false;
    
    // Właściwości tylko do odczytu
    public string? CategoryName { get; set; }
    public string? SubcategoryName { get; set; }
    public int MonthNumber { get; set; }
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
