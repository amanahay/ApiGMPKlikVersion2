using ApiGMPKlik.DTOs.DataPrice;
using ApiGMPKlik.Shared;

namespace ApiGMPKlik.Interfaces.DataPrice;

public interface IDataPriceRangeService
{
    Task<ApiResponse<DataPriceRangeResponseDto>> CreateAsync(CreateDataPriceRangeDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<DataPriceRangeResponseDto>> UpdateAsync(int id, UpdateDataPriceRangeDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<DataPriceRangeResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<DataPriceRangeDropdownDto>>> GetDropdownAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<DataPriceRangeResponseDto>>> GetPagedAsync(DataPriceRangePagedRequest request, CancellationToken cancellationToken = default);
}