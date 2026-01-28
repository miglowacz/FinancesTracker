using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.DTOs.EnableBanking;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EnableBankingController : ControllerBase {
  private readonly IEnableBankingService _service;
  private readonly ILogger<EnableBankingController> _logger;

  public EnableBankingController(IEnableBankingService service, ILogger<EnableBankingController> logger) {
    _service = service;
    _logger = logger;
  }

  [HttpPost("start-auth")]
  public async Task<ActionResult<cApiResponse<AuthResponse_DTO>>> StartAuthorization([FromBody] AuthRequest_DTO request) {
    try {
      if (string.IsNullOrEmpty(request.AspspName) || string.IsNullOrEmpty(request.Country)) {
        return BadRequest(cApiResponse<AuthResponse_DTO>.Error("AspspName and Country are required"));
      }

      // Automatyczne budowanie adresu callback na podstawie bieżącego żądania
      var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/enablebanking/callback";

      var authResponse = await _service.StartAuthorizationAsync(request.AspspName, request.Country, callbackUrl);
      return Ok(cApiResponse<AuthResponse_DTO>.SuccessResult(authResponse));
    } catch (Exception ex) {
      _logger.LogError(ex, "Error starting authorization");
      return StatusCode(500, cApiResponse<AuthResponse_DTO>.Error($"Failed to start authorization: {ex.Message}"));
    }
  }

  [HttpGet("callback")]
  public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state) {
    try {
      if (string.IsNullOrEmpty(code)) return BadRequest("Missing code parameter");

      // Wymiana kodu na sesję w serwisie
      var session = await _service.CreateSessionAsync(code, state);

      // PRZEKIEROWANIE NA FRONTEND (React/Angular)
      // Ważne: Podaj pełny adres frontendu!
      var frontendUrl = "https://localhost:4200";
      return Redirect($"{frontendUrl}/bank-import?sessionId={session.SessionId}&success=true");
    } catch (Exception ex) {
      _logger.LogError(ex, "Error in callback");
      return Redirect($"https://localhost:4200/bank-import?error={Uri.EscapeDataString(ex.Message)}");
    }
  }

  [HttpGet("aspsps/{country}")]

  public async Task<ActionResult<cApiResponse<List<Aspsp_DTO>>>> GetAspsps(string country) {

    try {

      var aspsps = await _service.GetAspspsAsync(country);

      return Ok(cApiResponse<List<Aspsp_DTO>>.SuccessResult(aspsps));

    } catch (Exception ex) {

      _logger.LogError(ex, "Error getting ASPSPs for country {Country}", country);

      return StatusCode(500, cApiResponse<List<Aspsp_DTO>>.Error($"Failed to retrieve banks: {ex.Message}"));

    }

  }


  // ... reszta metod (GetAspsps, GetAccounts, GetTransactions) pozostaje bez zmian
}
