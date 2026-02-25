using ApiGMPKlik.DTOs.Address;
using ApiGMPKlik.Infrastructure.Address;
using ApiGMPKlik.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers.Address
{
    [ApiController]
    [Route("api/[controller]")]
    public class WilayahKecamatanController : ControllerBase
    {
        private readonly IWilayahKecamatanService _service;

        public WilayahKecamatanController(IWilayahKecamatanService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<KecamatanListDto>>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int? kotaKabId = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetListAsync(page, pageSize, keyword, sortBy, sortOrder, kotaKabId, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<KecamatanReadDto>>> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _service.GetByIdAsync(id, cancellationToken);
            if (!response.IsSuccess) return NotFound(response);
            return Ok(response);
        }

        [HttpGet("dropdown")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<KecamatanDropdownDto>>>> GetDropdown(
            [FromQuery] int? kotaKabId = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetDropdownAsync(kotaKabId, cancellationToken);
            return Ok(response);
        }

        [HttpGet("by-kota-kab/{kotaKabId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<KecamatanDropdownDto>>>> GetByKotaKab(int kotaKabId, CancellationToken cancellationToken)
        {
            var response = await _service.GetByKotaKabIdAsync(kotaKabId, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<KecamatanReadDto>>> Create([FromBody] CreateKecamatanDto dto, CancellationToken cancellationToken)
        {
            var response = await _service.CreateAsync(dto, cancellationToken);
            if (!response.IsSuccess) return BadRequest(response);
            return CreatedAtAction(nameof(GetById), new { id = (response.Data?.Id ?? 0) }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<KecamatanReadDto>>> Update(int id, [FromBody] UpdateKecamatanDto dto, CancellationToken cancellationToken)
        {
            var response = await _service.UpdateAsync(id, dto, cancellationToken);
            if (!response.IsSuccess)
            {
                if (response.Type == ResponseType.NotFound) return NotFound(response);
                return BadRequest(response);
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