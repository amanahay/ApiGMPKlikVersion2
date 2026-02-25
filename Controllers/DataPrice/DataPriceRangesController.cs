using ApiGMPKlik.DTOs.DataPrice;
using ApiGMPKlik.Interfaces.DataPrice;
using ApiGMPKlik.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers.DataPrice;

[ApiController]
[Route("api/datapriceranges")]
public class DataPriceRangesController : ControllerBase
{
    private readonly IDataPriceRangeService _service;

    public DataPriceRangesController(IDataPriceRangeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DataPriceRangeResponseDto>>>> GetPaged(
        [FromQuery] DataPriceRangePagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DataPriceRangeResponseDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _service.GetByIdAsync(id, cancellationToken);
        if (!response.IsSuccess && response.Type == ResponseType.NotFound)
            return NotFound(response);
        return Ok(response);
    }

    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<List<DataPriceRangeDropdownDto>>>> GetDropdown(CancellationToken cancellationToken)
    {
        var response = await _service.GetDropdownAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DataPriceRangeResponseDto>>> Create(
        [FromBody] CreateDataPriceRangeDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<DataPriceRangeResponseDto>.ValidationError(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => new ErrorDetail
                {
                    Message = e.ErrorMessage
                }).ToList()));

        var response = await _service.CreateAsync(dto, cancellationToken);
        if (!response.IsSuccess && response.Type == ResponseType.ValidationError)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DataPriceRangeResponseDto>>> Update(
        int id,
        [FromBody] UpdateDataPriceRangeDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.Id)
            return BadRequest(ApiResponse<DataPriceRangeResponseDto>.ValidationError(
                new List<ErrorDetail> { new ErrorDetail { Message = "ID tidak cocok" } }));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<DataPriceRangeResponseDto>.ValidationError(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => new ErrorDetail
                {
                    Message = e.ErrorMessage
                }).ToList()));

        var response = await _service.UpdateAsync(id, dto, cancellationToken);

        if (!response.IsSuccess)
        {
            if (response.Type == ResponseType.NotFound) return NotFound(response);
            if (response.StatusCode == 409) return Conflict(response);
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _service.SoftDeleteAsync(id, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Type == ResponseType.NotFound) return NotFound(response);
            return BadRequest(response);
        }
        return Ok(response);
    }
}