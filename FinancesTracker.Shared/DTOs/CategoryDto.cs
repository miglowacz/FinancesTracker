namespace FinancesTracker.Shared.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SubcategoryDto> Subcategories { get; set; } = new();
}

public class SubcategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}