using System;

namespace FinancesTracker.Shared.DTOs.EnableBanking;

public class BankTransaction_DTO {
  public string Id { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public decimal Amount { get; set; }
  public DateTime Date { get; set; }
  public string Currency { get; set; } = string.Empty;
  public string CreditorName { get; set; } = string.Empty;
  public string DebtorName { get; set; } = string.Empty;
}
