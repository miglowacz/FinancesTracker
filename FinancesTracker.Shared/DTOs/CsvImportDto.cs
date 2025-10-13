namespace FinancesTracker.Shared.DTOs;

public class CsvImportDto
{
    public string BankName { get; set; } = string.Empty;
    public List<CsvRowDto> Rows { get; set; } = new();
    public CsvMappingDto Mapping { get; set; } = new();
}

public class CsvRowDto
{
    public Dictionary<string, string> Columns { get; set; } = new();
    public bool IsSelected { get; set; } = true;
    public TransactionDto? ParsedTransaction { get; set; }
}

public class CsvMappingDto
{
    public string DateColumn { get; set; } = string.Empty;
    public string DescriptionColumn { get; set; } = string.Empty;
    public string AmountColumn { get; set; } = string.Empty;
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string AmountFormat { get; set; } = "standard"; // standard, comma-decimal
}