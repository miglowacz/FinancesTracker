using System.Collections.Generic;

namespace FinancesTracker.Shared.DTOs;

public class cApiResponse<T> {
  public bool Success { get; set; }
  public T? Data { get; set; }
  public string? Message { get; set; }
  public List<string> Errors { get; set; } = new();

  public static cApiResponse<T> SuccessResult(T data, string? message = null) {
    return new cApiResponse<T> {
      Success = true,
      Data = data,
      Message = message
    };
  }

  public static cApiResponse<T> Error(string message, List<string>? errors = null) {
    return new cApiResponse<T> {
      Success = false,
      Message = message,
      Errors = errors ?? new List<string>()
    };
  }
}

public class cApiResponse : cApiResponse<object> {
  public new static cApiResponse SuccessResult(string? message = null) {
    return new cApiResponse {
      Success = true,
      Message = message
    };
  }

  public new static cApiResponse Error(string message, List<string>? errors = null) {
    return new cApiResponse {
      Success = false,
      Message = message,
      Errors = errors ?? new List<string>()
    };
  }
}
