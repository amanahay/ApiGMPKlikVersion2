using ApiGMPKlik.DTOs.Address;
using WinFormApiGMPKlik.Models;

namespace WinFormApiGMPKlik.Services
{
    #region == PROVINSI SERVICE ==

    public class WilayahProvinsiService
    {
        private readonly ApiClientService _apiClient;

        public WilayahProvinsiService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<ProvinsiListDto>>> GetListAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = null, string? sortOrder = "asc",
            CancellationToken ct = default)
        {
            var endpoint = $"api/wilayah/provinsi?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(keyword)) endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
            if (!string.IsNullOrEmpty(sortBy)) endpoint += $"&sortBy={Uri.EscapeDataString(sortBy)}";
            if (!string.IsNullOrEmpty(sortOrder)) endpoint += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
            
            return await _apiClient.GetAsync<List<ProvinsiListDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<ProvinsiDropdownDto>>> GetDropdownAsync(CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<List<ProvinsiDropdownDto>>("api/wilayah/provinsi/dropdown", ct);
        }

        public async Task<ApiResponse<ProvinsiReadDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<ProvinsiReadDto>($"api/wilayah/provinsi/{id}", ct);
        }

        public async Task<ApiResponse<ProvinsiReadDto>> CreateAsync(CreateProvinsiDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<ProvinsiReadDto>("api/wilayah/provinsi", dto, ct);
        }

        public async Task<ApiResponse<ProvinsiReadDto>> UpdateAsync(int id, UpdateProvinsiDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<ProvinsiReadDto>($"api/wilayah/provinsi/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/wilayah/provinsi/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/wilayah/provinsi/{id}/restore", new { }, ct);
        }
    }

    #endregion

    #region == KOTA/KABUPATEN SERVICE ==

    public class WilayahKotaKabService
    {
        private readonly ApiClientService _apiClient;

        public WilayahKotaKabService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<KotaKabListDto>>> GetListAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = null, string? sortOrder = "asc",
            int? provinsiId = null, CancellationToken ct = default)
        {
            var endpoint = $"api/wilayah/kotakab?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(keyword)) endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
            if (!string.IsNullOrEmpty(sortBy)) endpoint += $"&sortBy={Uri.EscapeDataString(sortBy)}";
            if (!string.IsNullOrEmpty(sortOrder)) endpoint += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
            if (provinsiId.HasValue) endpoint += $"&provinsiId={provinsiId.Value}";
            
            return await _apiClient.GetAsync<List<KotaKabListDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<KotaKabDropdownDto>>> GetDropdownAsync(int? provinsiId = null, CancellationToken ct = default)
        {
            var endpoint = "api/wilayah/kotakab/dropdown";
            if (provinsiId.HasValue) endpoint += $"?provinsiId={provinsiId.Value}";
            return await _apiClient.GetAsync<List<KotaKabDropdownDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<KotaKabReadDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<KotaKabReadDto>($"api/wilayah/kotakab/{id}", ct);
        }

        public async Task<ApiResponse<KotaKabReadDto>> CreateAsync(CreateKotaKabDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<KotaKabReadDto>("api/wilayah/kotakab", dto, ct);
        }

        public async Task<ApiResponse<KotaKabReadDto>> UpdateAsync(int id, UpdateKotaKabDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<KotaKabReadDto>($"api/wilayah/kotakab/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/wilayah/kotakab/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/wilayah/kotakab/{id}/restore", new { }, ct);
        }
    }

    #endregion

    #region == KECAMATAN SERVICE ==

    public class WilayahKecamatanService
    {
        private readonly ApiClientService _apiClient;

        public WilayahKecamatanService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<KecamatanListDto>>> GetListAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = null, string? sortOrder = "asc",
            int? kotaKabId = null, CancellationToken ct = default)
        {
            var endpoint = $"api/wilayah/kecamatan?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(keyword)) endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
            if (!string.IsNullOrEmpty(sortBy)) endpoint += $"&sortBy={Uri.EscapeDataString(sortBy)}";
            if (!string.IsNullOrEmpty(sortOrder)) endpoint += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
            if (kotaKabId.HasValue) endpoint += $"&kotaKabId={kotaKabId.Value}";
            
            return await _apiClient.GetAsync<List<KecamatanListDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<KecamatanDropdownDto>>> GetDropdownAsync(int? kotaKabId = null, CancellationToken ct = default)
        {
            var endpoint = "api/wilayah/kecamatan/dropdown";
            if (kotaKabId.HasValue) endpoint += $"?kotaKabId={kotaKabId.Value}";
            return await _apiClient.GetAsync<List<KecamatanDropdownDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<KecamatanReadDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<KecamatanReadDto>($"api/wilayah/kecamatan/{id}", ct);
        }

        public async Task<ApiResponse<KecamatanReadDto>> CreateAsync(CreateKecamatanDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<KecamatanReadDto>("api/wilayah/kecamatan", dto, ct);
        }

        public async Task<ApiResponse<KecamatanReadDto>> UpdateAsync(int id, UpdateKecamatanDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<KecamatanReadDto>($"api/wilayah/kecamatan/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/wilayah/kecamatan/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/wilayah/kecamatan/{id}/restore", new { }, ct);
        }
    }

    #endregion

    #region == KELURAHAN/DESA SERVICE ==

    public class WilayahKelurahanDesaService
    {
        private readonly ApiClientService _apiClient;

        public WilayahKelurahanDesaService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<KelurahanDesaListDto>>> GetListAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = null, string? sortOrder = "asc",
            int? kecamatanId = null, CancellationToken ct = default)
        {
            var endpoint = $"api/wilayah/kelurahan?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(keyword)) endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
            if (!string.IsNullOrEmpty(sortBy)) endpoint += $"&sortBy={Uri.EscapeDataString(sortBy)}";
            if (!string.IsNullOrEmpty(sortOrder)) endpoint += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
            if (kecamatanId.HasValue) endpoint += $"&kecamatanId={kecamatanId.Value}";
            
            return await _apiClient.GetAsync<List<KelurahanDesaListDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<KelurahanDesaDropdownDto>>> GetDropdownAsync(int? kecamatanId = null, CancellationToken ct = default)
        {
            var endpoint = "api/wilayah/kelurahan/dropdown";
            if (kecamatanId.HasValue) endpoint += $"?kecamatanId={kecamatanId.Value}";
            return await _apiClient.GetAsync<List<KelurahanDesaDropdownDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<KelurahanDesaReadDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<KelurahanDesaReadDto>($"api/wilayah/kelurahan/{id}", ct);
        }

        public async Task<ApiResponse<KelurahanDesaReadDto>> CreateAsync(CreateKelurahanDesaDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<KelurahanDesaReadDto>("api/wilayah/kelurahan", dto, ct);
        }

        public async Task<ApiResponse<KelurahanDesaReadDto>> UpdateAsync(int id, UpdateKelurahanDesaDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<KelurahanDesaReadDto>($"api/wilayah/kelurahan/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/wilayah/kelurahan/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/wilayah/kelurahan/{id}/restore", new { }, ct);
        }
    }

    #endregion

    #region == DUSUN SERVICE ==

    public class WilayahDusunService
    {
        private readonly ApiClientService _apiClient;

        public WilayahDusunService(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<DusunListDto>>> GetListAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = null, string? sortOrder = "asc",
            int? kelurahanId = null, CancellationToken ct = default)
        {
            var endpoint = $"api/wilayah/dusun?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(keyword)) endpoint += $"&keyword={Uri.EscapeDataString(keyword)}";
            if (!string.IsNullOrEmpty(sortBy)) endpoint += $"&sortBy={Uri.EscapeDataString(sortBy)}";
            if (!string.IsNullOrEmpty(sortOrder)) endpoint += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
            if (kelurahanId.HasValue) endpoint += $"&kelurahanId={kelurahanId.Value}";
            
            return await _apiClient.GetAsync<List<DusunListDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<List<DusunDropdownDto>>> GetDropdownAsync(int? kelurahanId = null, CancellationToken ct = default)
        {
            var endpoint = "api/wilayah/dusun/dropdown";
            if (kelurahanId.HasValue) endpoint += $"?kelurahanId={kelurahanId.Value}";
            return await _apiClient.GetAsync<List<DusunDropdownDto>>(endpoint, ct);
        }

        public async Task<ApiResponse<DusunReadDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.GetAsync<DusunReadDto>($"api/wilayah/dusun/{id}", ct);
        }

        public async Task<ApiResponse<DusunReadDto>> CreateAsync(CreateDusunDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PostAsync<DusunReadDto>("api/wilayah/dusun", dto, ct);
        }

        public async Task<ApiResponse<DusunReadDto>> UpdateAsync(int id, UpdateDusunDto dto, CancellationToken ct = default)
        {
            return await _apiClient.PutAsync<DusunReadDto>($"api/wilayah/dusun/{id}", dto, ct);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.DeleteAsync($"api/wilayah/dusun/{id}", ct);
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken ct = default)
        {
            return await _apiClient.PatchAsync<bool>($"api/wilayah/dusun/{id}/restore", new { }, ct);
        }
    }

    #endregion
}
