namespace FinancesTracker.Shared.DTOs;

public class cCategory_DTO {
 
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public List<cSubcategory_DTO> Subcategories { get; set; } = new();

}

public class cSubcategory_DTO {

  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public int CategoryId { get; set; }

}