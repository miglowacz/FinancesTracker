using FinancesTracker.Services;
using FinancesTracker.Shared.DTOs;
using FinancesTracker.Shared.Models;

using Microsoft.AspNetCore.Mvc;

namespace FinancesTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase {
  private readonly ITransactionService _service;

  public TransactionsController(ITransactionService service) {
    _service = service;
  }

  [HttpGet]
  public async Task<ActionResult<cApiResponse<cPagedResult<cTransaction_DTO>>>> GetTransactions([FromQuery] cTransactionFilter_DTO xFilter) {
    var result = await _service.GetTransactionsAsync(xFilter);
    return Ok(cApiResponse<cPagedResult<cTransaction_DTO>>.SuccessResult(result));
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> GetTransaction(int xId) {
    var result = await _service.GetTransactionByIdAsync(xId);
    return result == null
        ? NotFound(cApiResponse<cTransaction_DTO>.Error("Nie znaleziono transakcji"))
        : Ok(cApiResponse<cTransaction_DTO>.SuccessResult(result));
  }

  [HttpPost]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> CreateTransaction(cTransaction_DTO xDto) {
    if (!ModelState.IsValid) return BadRequest(cApiResponse<cTransaction_DTO>.Error("Błędne dane"));

    try {
      cTransaction_DTO result;
      if (xDto.IsTransfer && xDto.TargetAccountId.HasValue) {
        result = await _service.CreateTransferAsync(xDto);
      } else {
        result = await _service.CreateTransactionAsync(xDto);
      }
      return CreatedAtAction(nameof(GetTransaction), new { xId = result.Id }, cApiResponse<cTransaction_DTO>.SuccessResult(result));
    } catch (ArgumentException ex) {
      return BadRequest(cApiResponse<cTransaction_DTO>.Error(ex.Message));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse<cTransaction_DTO>.Error($"Błąd: {ex.Message}"));
    }
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<cApiResponse<cTransaction_DTO>>> UpdateTransaction(int id, cTransaction_DTO xDto) {
    var result = await _service.UpdateTransactionAsync(id, xDto);
    return result == null
        ? NotFound(cApiResponse<cTransaction_DTO>.Error("Nie znaleziono"))
        : Ok(cApiResponse<cTransaction_DTO>.SuccessResult(result));
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult<cApiResponse>> DeleteTransaction(int id) {
    var success = await _service.DeleteTransactionAsync(id);
    return success ? Ok(cApiResponse.SuccessResult("Usunięto")) : NotFound(cApiResponse.Error("Nie znaleziono"));
  }

  [HttpGet("summary")]
  public async Task<ActionResult<cApiResponse<cSummary_DTO>>> GetSummary(
      [FromQuery(Name = "year")] int xYear, 
      [FromQuery(Name = "month")] int? xMonth = null, 
      [FromQuery(Name = "includeInsignificant")] bool xIncludeInsignificant = false) {
    var result = await _service.GetSummaryAsync(xYear, xMonth, xIncludeInsignificant);
    return Ok(cApiResponse<cSummary_DTO>.SuccessResult(result));
  }

  [HttpPost("import")]
  public async Task<ActionResult<cApiResponse>> ImportTransactions([FromBody] List<cTransaction_DTO> transactions) {
    try {
      var (imported, insignif, errors) = await _service.ImportTransactionsAsync(transactions);
      string msg = $"Zaimportowano {imported} (w tym {insignif} nieistotnych).";
      return errors.Any() ? Ok(cApiResponse.Error($"{msg} Część wpisów wymagała uwagi.", errors)) : Ok(cApiResponse.SuccessResult(msg));
    } catch (Exception ex) {
      return StatusCode(500, cApiResponse.Error($"Błąd importu: {ex.Message}"));
    }
  }
}
