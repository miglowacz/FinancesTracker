using FinancesTracker.Shared.DTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FinancesTracker.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<T>>(endpoint, _jsonOptions);
            return response ?? ApiResponse<T>.ErrorResult("Pusta odpowiedŸ z serwera");
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.ErrorResult($"B³¹d sieci: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResult($"Nieoczekiwany b³¹d: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return result ?? ApiResponse<T>.ErrorResult("B³¹d deserializacji odpowiedzi");
            }
            else
            {
                var errorResult = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return errorResult ?? ApiResponse<T>.ErrorResult($"B³¹d HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResult($"B³¹d podczas wysy³ania ¿¹dania: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return result ?? ApiResponse<T>.ErrorResult("B³¹d deserializacji odpowiedzi");
            }
            else
            {
                var errorResult = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return errorResult ?? ApiResponse<T>.ErrorResult($"B³¹d HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResult($"B³¹d podczas aktualizacji: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions);
                return result ?? ApiResponse.Success("Operacja zakoñczona pomyœlnie");
            }
            else
            {
                var errorResult = JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions);
                return errorResult ?? ApiResponse.Error($"B³¹d HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return ApiResponse.Error($"B³¹d podczas usuwania: {ex.Message}");
        }
    }
}