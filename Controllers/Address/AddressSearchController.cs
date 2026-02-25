using ApiGMPKlik.DTOs.Address;
using ApiGMPKlik.Infrastructure.Address;
using ApiGMPKlik.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers.Address
{
    [ApiController]
    [Route("api/address")]
    public class AddressSearchController : ControllerBase
    {
        private readonly IAddressSearchService _searchService;

        public AddressSearchController(IAddressSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<AddressSearchResultDto>>>> Search(
            [FromQuery] string keyword,
            [FromQuery] int limit = 7,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(ApiResponse<List<AddressSearchResultDto>>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "keyword", Message = "Keyword harus diisi" }
                }));

            var response = await _searchService.SearchAsync(keyword, limit, cancellationToken);
            return Ok(response);
        }
    }
}