using FinancesTracker.Shared.DTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FinancesTracker.Client.Services;

public class cApiService {
  private readonly HttpClient _httpClient;
  private readonly JsonSerializerOptions _jsonOptions;

  public cApiService(HttpClient httpClient) {
    _httpClient = httpClient;
    _jsonOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
  }

  public async Task<cApiResponse<T>> GetAsync<T>(string endpoint) {
    try {
      var response = await _httpClient.GetFromJsonAsync<cApiResponse<T>>(endpoint, _jsonOptions);
      return response ?? cApiResponse<T>.Error("Pusta odpowiedź z serwera");
    } catch (HttpRequestException ex) {
      return cApiResponse<T>.Error($"Błąd sieci: {ex.Message}");
    } catch (Exception ex) {
      return cApiResponse<T>.Error($"Nieoczekiwany błąd: {ex.Message}");
    }
  }

  public async Task<cApiResponse<T>> PostAsync<T>(string endpoint, object data) {
    try {
      var json = JsonSerializer.Serialize(data, _jsonOptions);
      var content = new StringContent(json, Encoding.UTF8, "application/json");
      
      var response = await _httpClient.PostAsync(endpoint, content);
      var responseContent = await response.Content.ReadAsStringAsync();
      
      if (response.IsSuccessStatusCode) {
        var result = JsonSerializer.Deserialize<cApiResponse<T>>(responseContent, _jsonOptions);
        return result ?? cApiResponse<T>.Error("Błąd deserializacji odpowiedzi");
      } else {
        var Error = JsonSerializer.Deserialize<cApiResponse<T>>(responseContent, _jsonOptions);
        return Error ?? cApiResponse<T>.Error($"Błąd HTTP {response.StatusCode}");
      }
    } catch (Exception ex) {
      return cApiResponse<T>.Error($"Błąd podczas wysyłania żądania: {ex.Message}");
    }
  }

  public async Task<cApiResponse<T>> PutAsync<T>(string endpoint, object data) {
    try {
      var json = JsonSerializer.Serialize(data, _jsonOptions);
      var content = new StringContent(json, Encoding.UTF8, "application/json");
      
      var response = await _httpClient.PutAsync(endpoint, content);
      var responseContent = await response.Content.ReadAsStringAsync();
      
      if (response.IsSuccessStatusCode) {
        var result = JsonSerializer.Deserialize<cApiResponse<T>>(responseContent, _jsonOptions);
        return result ?? cApiResponse<T>.Error("Błąd deserializacji odpowiedzi");
      } else {
        var error = JsonSerializer.Deserialize<cApiResponse<T>>(responseContent, _jsonOptions);
        return error ?? cApiResponse<T>.Error($"Błąd HTTP {response.StatusCode}");
      }
    } catch (Exception ex) {
      return cApiResponse<T>.Error($"Błąd podczas aktualizacji: {ex.Message}");
    }
  }

  public async Task<cApiResponse<T>> PatchAsync<T>(string endpoint, object? data) {
    try {
      var json = JsonSerializer.Serialize(data ?? new { }, _jsonOptions);
      var content = new StringContent(json, Encoding.UTF8, "application/json");
      
      var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) {
        Content = content
      };
      
      var response = await _httpClient.SendAsync(request);
      var responseContent = await response.Content.ReadAsStringAsync();
      
      if (response.IsSuccessStatusCode) {
        var result = JsonSerializer.Deserialize<cApiResponse<T>>(responseContent, _jsonOptions);
        return result ?? cApiResponse<T>.Error("Błąd deserializacji odpowiedzi");
      } else {
        var error = JsonSerializer.Deserialize<cApiResponse<T>>(responseContent, _jsonOptions);
        return error ?? cApiResponse<T>.Error($"Błąd HTTP {response.StatusCode}");
      }
    } catch (Exception ex) {
      return cApiResponse<T>.Error($"Błąd podczas częściowej aktualizacji: {ex.Message}");
    }
  }

  public async Task<cApiResponse> DeleteAsync(string endpoint) {
    try {
      var response = await _httpClient.DeleteAsync(endpoint);
      var responseContent = await response.Content.ReadAsStringAsync();
      
      if (response.IsSuccessStatusCode) {
        var result = JsonSerializer.Deserialize<cApiResponse>(responseContent, _jsonOptions);
        return result ?? cApiResponse.SuccessResult("Operacja zakończona pomyślnie");
      } else {
        var Error = JsonSerializer.Deserialize<cApiResponse>(responseContent, _jsonOptions);
        return Error ?? cApiResponse.Error($"Błąd HTTP {response.StatusCode}");
      }
    } catch (Exception ex) {
      return cApiResponse.Error($"Błąd podczas usuwania: {ex.Message}");
    }
  }
}

