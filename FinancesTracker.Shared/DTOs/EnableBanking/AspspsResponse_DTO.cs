using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinancesTracker.Shared.DTOs.EnableBanking {
  public class AspspsResponse_DTO {

    [JsonPropertyName("aspsps")]
    public List<Aspsp_DTO> Aspsps { get; set; } = new();

  }
}
