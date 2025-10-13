namespace FinancesTracker.Shared.DTOs;

public class TransactionFilterDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public string? BankName { get; set; }
    
    // Paginacja
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    
    // Sortowanie
    public string SortBy { get; set; } = "Date";
    public bool SortDescending { get; set; } = true;
}