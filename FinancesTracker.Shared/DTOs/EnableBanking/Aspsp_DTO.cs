using System.Text.Json.Serialization;

namespace FinancesTracker.Shared.DTOs.EnableBanking;

public class Aspsp_DTO {
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("country")]
  public string Country { get; set; } = string.Empty;

  [JsonPropertyName("logo")]
  public string? Logo { get; set; }

  [JsonPropertyName("services")]
  public List<string> Services { get; set; } = new();

  [JsonPropertyName("psu_types")]
  public List<string> PsuTypes { get; set; } = new(); // np. ["personal", "business"]
}
