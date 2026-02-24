using ApiGMPKlik.Application.Interfaces;
using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize]
    public class ReferralTreeController : ControllerBase
    {
        private readonly IReferralTreeService _referralTreeService;
        private readonly ILogger<ReferralTreeController> _logger;
        private readonly ICurrentUserService _currentUserService;
        public ReferralTreeController(
        IReferralTreeService referralTreeService,
        ICurrentUserService currentUserService, 
        ILogger<ReferralTreeController> logger)
        {
            _referralTreeService = referralTreeService;
            _currentUserService = currentUserService; 
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<ReferralTreeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? rootUserId = null,
            [FromQuery] string? referredUserId = null,
            [FromQuery] int? level = null,
            [FromQuery] int? maxLevel = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "Level",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var filter = new ReferralTreeFilterDto
            {
                RootUserId = rootUserId,
                ReferredUserId = referredUserId,
                Level = level,
                MaxLevel = maxLevel,
                IsActive = isActive,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _referralTreeService.GetPaginatedAsync(filter, page, pageSize, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ReferralTreeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.GetByIdAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}/tree")]
        [ProducesResponseType(typeof(ApiResponse<List<ReferralTreeNodeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTreeByUserId(
            string userId,
            [FromQuery] int? maxLevel = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.GetTreeByRootUserIdAsync(userId, maxLevel, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}/tree-structure")]
        [ProducesResponseType(typeof(ApiResponse<TreeNodeDto<ReferralTreeNodeData>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTreeStructure(
            string userId,
            [FromQuery] int? maxLevel = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.GetTreeStructureAsync(userId, maxLevel, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}/referrals")]
        [ProducesResponseType(typeof(ApiResponse<List<ReferralTreeNodeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReferralsByUserId(
            string userId,
            [FromQuery] int? level = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.GetReferralsByUserIdAsync(userId, level, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}/ancestors")]
        [ProducesResponseType(typeof(ApiResponse<List<ReferralTreeNodeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAncestorsByUserId(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.GetAncestorsByUserIdAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}/statistics")]
        [ProducesResponseType(typeof(ApiResponse<ReferralStatisticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics(string userId, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.GetStatisticsAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReferralTreeDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateReferralTreeDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.CreateAsync(dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ReferralTreeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReferralTreeDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.UpdateAsync(id, dto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.DeleteAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:int}/activate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.ActivateAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id:int}/deactivate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.DeactivateAsync(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check/can-add")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CanAddReferral(
            [FromQuery] string rootUserId,
            [FromQuery] string referredUserId,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.CanAddReferralAsync(rootUserId, referredUserId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("exists/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Exists(int id, CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.ExistsAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("exists-referral")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReferralExists(
            [FromQuery] string rootUserId,
            [FromQuery] string referredUserId,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.ReferralExistsAsync(rootUserId, referredUserId, cancellationToken);
            return Ok(result);
        }
        #region Advanced Operations (Move & Assign)

        [HttpPost("move")]
        [Authorize(Roles = "Admin,SuperAdmin,Management")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MoveDownline(
            [FromBody] MoveDownlineRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.UserIdToMove) || string.IsNullOrWhiteSpace(dto.NewParentId))
                return BadRequest(ApiResponse.Error("Parameter tidak lengkap", "UserId dan NewParentId wajib diisi"));

            // Validasi permission (hanya bisa memindahkan jika punya akses ke kedua user)
            var canAccessSource = _currentUserService.CanAccessOwnData(dto.UserIdToMove) ||
                                  _currentUserService.IsAdmin() ||
                                  _currentUserService.IsSuperAdmin();

            var canAccessTarget = _currentUserService.CanAccessOwnData(dto.NewParentId) ||
                                  _currentUserService.IsAdmin() ||
                                  _currentUserService.IsSuperAdmin();

            if (!canAccessSource || !canAccessTarget)
                return StatusCode(403, ApiResponse<object>.Forbidden("Anda tidak memiliki akses untuk memindahkan user ini"));

            var result = await _referralTreeService.MoveDownlineAsync(dto.UserIdToMove, dto.NewParentId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("assign-orphan")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignOrphanUser(
            [FromBody] AssignOrphanRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _referralTreeService.AssignOrphanUserAsync(dto.OrphanUserId, dto.TargetParentId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId}/direct-downlines")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<ReferralTreeNodeDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDirectDownlines(string userId, CancellationToken cancellationToken = default)
        {
            // Validasi akses
            if (!_currentUserService.CanAccessOwnData(userId) &&
                !_currentUserService.IsAdmin() &&
                !_currentUserService.IsSuperAdmin())
                return StatusCode(403, ApiResponse<object>.Forbidden("Tidak memiliki akses"));

            var result = await _referralTreeService.GetDirectDownlinesAsync(userId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("check/descendant")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckIsDescendant(
            [FromQuery] string ancestorId,
            [FromQuery] string descendantId,
            CancellationToken cancellationToken = default)
        {
            var isDescendant = await _referralTreeService.IsDescendantAsync(ancestorId, descendantId, cancellationToken);
            return Ok(ApiResponse<bool>.Success(isDescendant, isDescendant ? "Adalah descendant" : "Bukan descendant"));
        }

        #endregion
    }
}