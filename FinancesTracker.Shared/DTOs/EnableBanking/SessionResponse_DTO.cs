namespace FinancesTracker.Shared.DTOs.EnableBanking;

public class SessionResponse_DTO {
  public string SessionId { get; set; } = string.Empty;
  public List<BankAccount_DTO> Accounts { get; set; } = new();
}

public class BankAccount_DTO {
  public string AccountId { get; set; } = string.Empty;
  public string Iban { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public decimal? Balance { get; set; }
  public string Currency { get; set; } = string.Empty;
}
