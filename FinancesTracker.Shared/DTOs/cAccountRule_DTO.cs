namespace FinancesTracker.Shared.DTOs;

public class cAccountRule_DTO {
  public int Id { get; set; }
  public string Keyword { get; set; } = string.Empty;
  public int AccountId { get; set; }
  public string? AccountName { get; set; }
  public bool IsActive { get; set; }
}
