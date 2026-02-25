using ApiGMPKlik.DTOs.DataPrice;
using WinFormApiGMPKlik.Models;

namespace WinFormApiGMPKlik.Services
{
    /// <summary>
    /// Service untuk mengelola DataPriceRange di WinForm
    /// </summary>
    public class DataPriceService
    {
        private readonly ApiClientService _apiClient;

        public DataPriceService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<DataPriceRangeResponseDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<DataPriceRangeResponseDto>($"api/data-price/{id}", ct);
        }

        public async Task<ApiResponse<List<DataPriceRangeResponseDto>>> GetPagedAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = null, string? sortOrder = "asc", 
            CancellationToken ct = default)
        {
            var endpoint = $"api/data-price?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(keyword)) endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
            if (!string.IsNullOrEmpty(sortBy)) endpoint += $"&sortBy={Uri.EscapeDataString(sortBy)}";
            if (!string.IsNullOrEmpty(sortOrder)) endpoint += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
            
            return await _apiClient.GetAsync<List<DataPriceRangeResponseDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<DataPriceRangeDropdownDto>>> GetDropdownAsync(CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<DataPriceRangeDropdownDto>>("api/data-price/dropdown", ct);
        }

        public async Task<ApiResponse<DataPriceRangeResponseDto>> CreateAsync(CreateDataPriceRangeDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<DataPriceRangeResponseDto>("api/data-price", dto, ct);
        }

        public async Task<ApiResponse<DataPriceRangeResponseDto>> UpdateAsync(int id, UpdateDataPriceRangeDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<DataPriceRangeResponseDto>($"api/data-price/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/data-price/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/data-price/{id}/restore", new { }, ct);
        }

        public async Task<ApiResponse<bool>> ActivateAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/data-price/{id}/activate", new { }, ct);
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/data-price/{id}/deactivate", new { }, ct);
        }
    }
}
