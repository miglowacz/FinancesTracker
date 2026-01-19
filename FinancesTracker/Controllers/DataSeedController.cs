using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FinancesTracker.Controllers;

[Route("api/dataseed")]
[ApiController]
public class DataSeedController : ControllerBase {
  private readonly cDataSeedService _dataSeedService;

  public DataSeedController(cDataSeedService dataSeedService) {
    _dataSeedService = dataSeedService;
  }

  [HttpPost("generate")]
  public async Task<ActionResult<cApiResponse>> GenerateData([FromQuery] int transactionCount = 200) {
    try {
      await _dataSeedService.GenerateEverythingAsync(transactionCount);
      return Ok(cApiResponse.SuccessResult($"Pomyślnie wygenerowano {transactionCount} transakcji wraz z kontami i kategoriami"));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse.Error($"Błąd podczas generowania danych: {ex.Message}"));
    }
  }
}