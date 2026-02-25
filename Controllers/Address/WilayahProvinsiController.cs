using ApiGMPKlik.DTOs.Address;
using ApiGMPKlik.Infrastructure.Address;
using ApiGMPKlik.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers.Address
{
    [ApiController]
    [Route("api/[controller]")]
    public class WilayahProvinsiController : ControllerBase
    {
        private readonly IWilayahProvinsiService _service;

        public WilayahProvinsiController(IWilayahProvinsiService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<ProvinsiListDto>>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetListAsync(page, pageSize, keyword, sortBy, sortOrder, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ProvinsiReadDto>>> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _service.GetByIdAsync(id, cancellationToken);
            if (!response.IsSuccess && response.Type == ResponseType.NotFound)
                return NotFound(response);
            return Ok(response);
        }

        [HttpGet("dropdown")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<ProvinsiDropdownDto>>>> GetDropdown(CancellationToken cancellationToken)
        {
            var response = await _service.GetDropdownAsync(cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<ProvinsiReadDto>>> Create([FromBody] CreateProvinsiDto dto, CancellationToken cancellationToken)
        {
            var response = await _service.CreateAsync(dto, cancellationToken);
            if (!response.IsSuccess && response.Type == ResponseType.ValidationError)
                return BadRequest(response);
            return CreatedAtAction(nameof(GetById), new { id = (response.Data?.Id ?? 0) }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<ProvinsiReadDto>>> Update(int id, [FromBody] UpdateProvinsiDto dto, CancellationToken cancellationToken)
        {
            var response = await _service.UpdateAsync(id, dto, cancellationToken);
            if (!response.IsSuccess)
            {
                if (response.Type == ResponseType.NotFound) return NotFound(response);
                if (response.Type == ResponseType.ValidationError) return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
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
}