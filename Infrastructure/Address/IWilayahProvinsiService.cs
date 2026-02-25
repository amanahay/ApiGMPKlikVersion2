using ApiGMPKlik.DTOs.Address;
using ApiGMPKlik.Shared;

namespace ApiGMPKlik.Infrastructure.Address
{
    public interface IWilayahProvinsiService
    {
        Task<ApiResponse<ProvinsiReadDto>> CreateAsync(CreateProvinsiDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<ProvinsiReadDto>> UpdateAsync(int id, UpdateProvinsiDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<ProvinsiReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ProvinsiDropdownDto>>> GetDropdownAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<List<ProvinsiListDto>>> GetListAsync(int page, int pageSize, string? keyword, string? sortBy, string? sortOrder, CancellationToken cancellationToken = default);
    }

    public interface IWilayahKotaKabService
    {
        Task<ApiResponse<KotaKabReadDto>> CreateAsync(CreateKotaKabDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<KotaKabReadDto>> UpdateAsync(int id, UpdateKotaKabDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<KotaKabReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KotaKabDropdownDto>>> GetDropdownAsync(int? provinsiId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KotaKabListDto>>> GetListAsync(int page, int pageSize, string? keyword, string? sortBy, string? sortOrder, int? provinsiId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KotaKabDropdownDto>>> GetByProvinsiIdAsync(int provinsiId, CancellationToken cancellationToken = default);
    }
    public interface IWilayahKecamatanService
    {
        Task<ApiResponse<KecamatanReadDto>> CreateAsync(CreateKecamatanDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<KecamatanReadDto>> UpdateAsync(int id, UpdateKecamatanDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<KecamatanReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KecamatanDropdownDto>>> GetDropdownAsync(int? kotaKabId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KecamatanListDto>>> GetListAsync(int page, int pageSize, string? keyword, string? sortBy, string? sortOrder, int? kotaKabId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KecamatanDropdownDto>>> GetByKotaKabIdAsync(int kotaKabId, CancellationToken cancellationToken = default);
    }

    public interface IWilayahKelurahanDesaService
    {
        Task<ApiResponse<KelurahanDesaReadDto>> CreateAsync(CreateKelurahanDesaDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<KelurahanDesaReadDto>> UpdateAsync(int id, UpdateKelurahanDesaDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<KelurahanDesaReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KelurahanDesaDropdownDto>>> GetDropdownAsync(int? kecamatanId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KelurahanDesaListDto>>> GetListAsync(int page, int pageSize, string? keyword, string? sortBy, string? sortOrder, int? kecamatanId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<KelurahanDesaDropdownDto>>> GetByKecamatanIdAsync(int kecamatanId, CancellationToken cancellationToken = default);
    }



    public interface IWilayahDusunService
    {
        Task<ApiResponse<DusunReadDto>> CreateAsync(CreateDusunDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<DusunReadDto>> UpdateAsync(int id, UpdateDusunDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<DusunReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<DusunDropdownDto>>> GetDropdownAsync(int? kelurahanDesaId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<DusunListDto>>> GetListAsync(int page, int pageSize, string? keyword, string? sortBy, string? sortOrder, int? kelurahanDesaId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<DusunDropdownDto>>> GetByKelurahanDesaIdAsync(int kelurahanDesaId, CancellationToken cancellationToken = default);
    }

    public interface IWilayahRwService
    {
        Task<ApiResponse<RwReadDto>> CreateAsync(CreateRwDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<RwReadDto>> UpdateAsync(int id, UpdateRwDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<RwReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RwDropdownDto>>> GetDropdownAsync(int? dusunId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RwListDto>>> GetListAsync(int page, int pageSize, string? keyword, string? sortBy, string? sortOrder, int? dusunId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RwDropdownDto>>> GetByDusunIdAsync(int dusunId, CancellationToken cancellationToken = default);
    }

    public interface IWilayahRtService
    {
        Task<ApiResponse<RtReadDto>> CreateAsync(CreateRtDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<RtReadDto>> UpdateAsync(int id, UpdateRtDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<RtReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RtDropdownDto>>> GetDropdownAsync(int? rwId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RtListDto>>> GetListAsync(int page, int pageSize, string? keyword, string? sortBy, string? sortOrder, int? rwId = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RtDropdownDto>>> GetByRwIdAsync(int rwId, CancellationToken cancellationToken = default);
    }

    public interface IAddressSearchService
    {
        Task<ApiResponse<List<AddressSearchResultDto>>> SearchAsync(string keyword, int limit = 7, CancellationToken cancellationToken = default);
    }
}
