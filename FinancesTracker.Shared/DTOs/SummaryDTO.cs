public class SummaryDTO
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public List<CategorySummaryDTO> Categories { get; set; }
}

public class CategorySummaryDTO
{
    public string CategoryName { get; set; }
    public decimal Expenses { get; set; }
}