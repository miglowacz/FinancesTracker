using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinancesTracker.Shared.DTOs.EnableBanking {
  public class StartAuthorizationRequest_DTO {
    [JsonPropertyName("aspsp")]
    public AspspInfo Aspsp { get; set; }

    [JsonPropertyName("redirect_url")]
    public string RedirectUrl { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("access")]
    public AccessInfo Access { get; set; }

    [JsonPropertyName("psu_type")]
    public string PsuType { get; set; } = "personal";

    public class AspspInfo {
      [JsonPropertyName("name")]
      public string Name { get; set; }
      [JsonPropertyName("country")]
      public string Country { get; set; }
    }

    public class AccessInfo {
      [JsonPropertyName("valid_until")]
      public string ValidUntil { get; set; }
    }
  }
}
