namespace FinancesTracker.Shared.DTOs;

public class CsvImportDto {

  public string BankName { get; set; } = string.Empty;
  public List<cCSV_Row_DTO> Rows { get; set; } = new();
  public cCSV_Mapping_DTO Mapping { get; set; } = new();

}

public class cCSV_Row_DTO {

  public Dictionary<string, string> Columns { get; set; } = new();
  public bool IsSelected { get; set; } = true;
  public cTransaction_DTO? ParsedTransaction { get; set; }

}

public class cCSV_Mapping_DTO {

  public string DateColumn { get; set; } = string.Empty;
  public string DescriptionColumn { get; set; } = string.Empty;
  public string AmountColumn { get; set; } = string.Empty;
  public string DateFormat { get; set; } = "yyyy-MM-dd";
  public string AmountFormat { get; set; } = "standard"; // standard, comma-decimal

}
