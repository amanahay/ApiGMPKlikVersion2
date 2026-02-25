using ApiGMPKlik.Application.Interfaces;
using ApiGMPKlik.DTOs.DataPrice;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Interfaces.DataPrice;
using ApiGMPKlik.Interfaces.Repositories;
using ApiGMPKlik.Models.DataPrice;
using ApiGMPKlik.Shared;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace ApiGMPKlik.Services.DataPrice;

public class DataPriceRangeService : IDataPriceRangeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<DataPriceRange, int> _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public DataPriceRangeService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.Repository<DataPriceRange, int>();
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DataPriceRangeResponseDto>> CreateAsync(CreateDataPriceRangeDto dto, CancellationToken cancellationToken = default)
    {
        // Business validation
        if (dto.MinPrice < 0)
            return ApiResponse<DataPriceRangeResponseDto>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Field = "MinPrice", Message = "MinPrice harus >= 0" }
            });

        if (dto.MaxPrice <= dto.MinPrice)
            return ApiResponse<DataPriceRangeResponseDto>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Field = "MaxPrice", Message = "MaxPrice harus lebih besar dari MinPrice" }
            });

        // Check duplicate name (TenantId dihapus)
        var exists = await _repository.AnyAsync(x =>
            x.Name.ToLower() == dto.Name.ToLower() &&
            !x.IsDeleted, cancellationToken);

        if (exists)
            return ApiResponse<DataPriceRangeResponseDto>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Field = "Name", Message = "Nama sudah digunakan" }
            });

        var entity = _mapper.Map<DataPriceRange>(dto);
        entity.MarkAsCreated(_currentUserService.UserId ?? "System");

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<DataPriceRangeResponseDto>(entity);
        return ApiResponse<DataPriceRangeResponseDto>.Created(result, "Data berhasil dibuat");
    }

    public async Task<ApiResponse<DataPriceRangeResponseDto>> UpdateAsync(int id, UpdateDataPriceRangeDto dto, CancellationToken cancellationToken = default)
    {
        if (id != dto.Id)
            return ApiResponse<DataPriceRangeResponseDto>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Message = "ID tidak cocok" }
            });

        // Business validation
        if (dto.MinPrice < 0)
            return ApiResponse<DataPriceRangeResponseDto>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Field = "MinPrice", Message = "MinPrice harus >= 0" }
            });

        if (dto.MaxPrice <= dto.MinPrice)
            return ApiResponse<DataPriceRangeResponseDto>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Field = "MaxPrice", Message = "MaxPrice harus lebih besar dari MinPrice" }
            });

        var entity = await _repository.QueryIgnoreFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null || entity.IsDeleted)
            return ApiResponse<DataPriceRangeResponseDto>.NotFound("DataPriceRange");

        // Concurrency check
        if (dto.RowVersion != null && !entity.RowVersion!.SequenceEqual(dto.RowVersion))
        {
            return ApiResponse<DataPriceRangeResponseDto>.Error(
                "Data sudah diubah oleh user lain",
                "Concurrency conflict",
                409);
        }

        // Check duplicate name (exclude self) - Tanpa TenantId
        var exists = await _repository.AnyAsync(x =>
            x.Id != id &&
            x.Name.ToLower() == dto.Name.ToLower() &&
            !x.IsDeleted, cancellationToken);

        if (exists)
            return ApiResponse<DataPriceRangeResponseDto>.ValidationError(new List<ErrorDetail>
            {
                new ErrorDetail { Field = "Name", Message = "Nama sudah digunakan" }
            });

        _mapper.Map(dto, entity);
        entity.MarkAsUpdated(_currentUserService.UserId ?? "System");

        _repository.Update(entity);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResponse<DataPriceRangeResponseDto>.Error(
                "Data sudah diubah oleh user lain",
                "Concurrency conflict",
                409);
        }

        var result = _mapper.Map<DataPriceRangeResponseDto>(entity);
        return ApiResponse<DataPriceRangeResponseDto>.Success(result, "Data berhasil diupdate");
    }

    public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return ApiResponse<object>.NotFound("DataPriceRange");

        _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Success(null!, "Data berhasil dihapus");
    }

    public async Task<ApiResponse<DataPriceRangeResponseDto>> GetByIdAsync(
     int id,
     CancellationToken cancellationToken = default)
    {
        var entity = await _repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null)
            return ApiResponse<DataPriceRangeResponseDto>.NotFound("DataPriceRange");

        // Pastikan mapping ini terdefinisi di Profile
        var result = _mapper.Map<DataPriceRangeResponseDto>(entity);
        return ApiResponse<DataPriceRangeResponseDto>.Success(result);
    }
    public async Task<ApiResponse<List<DataPriceRangeDropdownDto>>> GetDropdownAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.Query()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Select(x => new DataPriceRangeDropdownDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                MinPrice = x.MinPrice,
                MaxPrice = x.MaxPrice,
                Currency = x.Currency
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DataPriceRangeDropdownDto>>.Success(items);
    }

    public async Task<ApiResponse<List<DataPriceRangeResponseDto>>> GetPagedAsync(
     DataPriceRangePagedRequest request,
     CancellationToken cancellationToken = default)
    {
        var query = _repository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                (x.Code != null && x.Code.ToLower().Contains(keyword)) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)));
        }

        // Sorting
        query = (request.SortBy?.ToLower(), request.SortOrder?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(x => x.Name),
            ("name", _) => query.OrderBy(x => x.Name),
            ("minprice", "desc") => query.OrderByDescending(x => x.MinPrice),
            ("minprice", _) => query.OrderBy(x => x.MinPrice),
            ("maxprice", "desc") => query.OrderByDescending(x => x.MaxPrice),
            ("maxprice", _) => query.OrderBy(x => x.MaxPrice),
            ("sortorder", "desc") => query.OrderByDescending(x => x.SortOrder),
            _ => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // Gunakan ProjectTo untuk mapping di level database (lebih efisien)
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<DataPriceRangeResponseDto>(_mapper.ConfigurationProvider)  // <-- Ganti Select dengan ini
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DataPriceRangeResponseDto>>.Paginated(
            items, request.Page, request.PageSize, totalCount, "Data berhasil diambil");
    }
}