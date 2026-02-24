using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;
        private readonly ILogger<BranchController> _logger;

        public BranchController(IBranchService branchService, ILogger<BranchController> logger)
        {
            _branchService = branchService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Manager,BranchAdmin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<BranchDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? city = null,
            [FromQuery] string? province = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isMainBranch = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var filter = new BranchFilterDto
            {
                Search = search,
                City = city,
                Province = province,
                IsActive = isActive,
                IsMainBranch = isMainBranch,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _branchService.GetPaginatedAsync(filter, page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin,Manager,BranchAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.GetByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("code/{code}")]
        [Authorize(Roles = "Admin,SuperAdmin,Manager,BranchAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.GetByCodeAsync(code, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateBranchDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new branch: {BranchName}", dto.Name);
            var result = await _branchService.CreateAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BranchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.UpdateAsync(id, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.SoftDeleteAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}/hard")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HardDelete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.HardDeleteAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:int}/restore")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.RestoreAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:int}/activate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.ActivateAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:int}/deactivate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.DeactivateAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("exists/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Exists(int id, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.ExistsAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("code-exists/{code}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CodeExists(string code, CancellationToken cancellationToken = default)
        {
            var result = await _branchService.CodeExistsAsync(code, cancellationToken);
            return Ok(result);
        }
    }
}