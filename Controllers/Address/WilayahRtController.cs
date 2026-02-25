using ApiGMPKlik.DTOs.Address;
using ApiGMPKlik.Infrastructure.Address;
using ApiGMPKlik.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers.Address
{
    [ApiController]
    [Route("api/[controller]")]
    public class WilayahRtController : ControllerBase
    {
        private readonly IWilayahRtService _service;

        public WilayahRtController(IWilayahRtService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<RtListDto>>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int? rwId = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetListAsync(page, pageSize, keyword, sortBy, sortOrder, rwId, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RtReadDto>>> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _service.GetByIdAsync(id, cancellationToken);
            if (!response.IsSuccess) return NotFound(response);
            return Ok(response);
        }

        [HttpGet("dropdown")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<RtDropdownDto>>>> GetDropdown(
            [FromQuery] int? rwId = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetDropdownAsync(rwId, cancellationToken);
            return Ok(response);
        }

        [HttpGet("by-rw/{rwId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<RtDropdownDto>>>> GetByRw(int rwId, CancellationToken cancellationToken)
        {
            var response = await _service.GetByRwIdAsync(rwId, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<RtReadDto>>> Create([FromBody] CreateRtDto dto, CancellationToken cancellationToken)
        {
            var response = await _service.CreateAsync(dto, cancellationToken);
            if (!response.IsSuccess) return BadRequest(response);
            return CreatedAtAction(nameof(GetById), new { id = (response.Data?.Id ?? 0) }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<RtReadDto>>> Update(int id, [FromBody] UpdateRtDto dto, CancellationToken cancellationToken)
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