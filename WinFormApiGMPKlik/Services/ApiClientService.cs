using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WinFormApiGMPKlik.Models;

namespace WinFormApiGMPKlik.Services
{
    /// <summary>
    /// Service untuk komunikasi dengan API Backend
    /// </summary>
    public class ApiClientService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private AuthSettings? _authSettings;

        public ApiClientService(ApiSettings settings)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.BaseUrl),
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            };
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
        }

        public void SetAuthSettings(AuthSettings auth)
        {
            _authSettings = auth;
            SetAuthToken(auth.Token);
        }

        public bool IsAuthenticated => _authSettings != null && !string.IsNullOrEmpty(_authSettings.Token);

        #region HTTP Methods

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint, ct);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Error("Gagal mengambil data", ex.Message);
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, ct);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Error("Gagal membuat data", ex.Message);
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content, ct);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Error("Gagal mengupdate data", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint, ct);
                return await HandleResponse<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error("Gagal menghapus data", ex.Message);
            }
        }

        public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint) { Content = content };
                var response = await _httpClient.SendAsync(request, ct);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Error("Gagal mengupdate data", ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                    return result ?? ApiResponse<T>.Error("Invalid response format", "Deserialization failed");
                }
                catch (JsonException)
                {
                    // Jika tidak bisa deserialize sebagai ApiResponse, coba deserialize langsung ke T
                    try
                    {
                        var directData = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                        return ApiResponse<T>.Success(directData!);
                    }
                    catch
                    {
                        return ApiResponse<T>.Error("Invalid response format", "Deserialization failed");
                    }
                }
            }

            // Handle error responses
            var errorMessage = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Sesi telah berakhir. Silakan login kembali.",
                System.Net.HttpStatusCode.Forbidden => "Anda tidak memiliki akses ke fitur ini.",
                System.Net.HttpStatusCode.NotFound => "Data tidak ditemukan.",
                System.Net.HttpStatusCode.BadRequest => "Permintaan tidak valid.",
                System.Net.HttpStatusCode.Conflict => "Data sudah ada atau terjadi konflik.",
                _ => $"Terjadi kesalahan: {response.StatusCode}"
            };

            return ApiResponse<T>.Error(errorMessage, $"HTTP {(int)response.StatusCode}", (int)response.StatusCode);
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Generic API Response wrapper untuk WinForm
    /// </summary>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Success(T data, string message = "Success")
        {
            return new ApiResponse<T> { IsSuccess = true, Data = data, Message = message, StatusCode = 200 };
        }

        public static ApiResponse<T> Error(string message, string error, int statusCode = 400)
        {
            return new ApiResponse<T> { IsSuccess = false, Message = message, Errors = new List<string> { error }, StatusCode = statusCode };
        }
    }
}
