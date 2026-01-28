namespace FinancesTracker.Models;

public class EnableBankingSettings {
  public string KeyPath { get; set; } = string.Empty;
  public string ApplicationId { get; set; } = string.Empty;
  public string ApiOrigin { get; set; } = string.Empty;
  public string JwtAudience { get; set; } = string.Empty;
  public string JwtIssuer { get; set; } = string.Empty;
}
