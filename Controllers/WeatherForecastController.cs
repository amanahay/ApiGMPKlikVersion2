using ApiGMPKlik.Infrastructure;
using ApiGMPKlik.Services;
using ApiGMPKlik.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGMPKlik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        // Endpoint dengan JWT + Permission
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Policy = "WEATHER_READ")]
        public IActionResult Get()
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            return Ok(ApiResponse<IEnumerable<WeatherForecast>>.Success(forecasts));
        }

        // Endpoint dengan API Key Only
        [HttpGet("public")]
        [Authorize(AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme)]
        public IActionResult GetPublic()
        {
            // API Key users bisa akses ini, tidak perlu JWT
            var forecast = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 25,
                Summary = "Mild"
            };

            return Ok(ApiResponse<WeatherForecast>.Success(forecast, "Data dari API Key Auth"));
        }

        // Endpoint dengan JWT + Permission (Create)
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Policy = "WEATHER_CREATE")]
        public IActionResult Create([FromBody] WeatherForecast forecast)
        {
            _logger.LogInformation("Weather forecast created by {User}", User.Identity?.Name);
            return Ok(ApiResponse<WeatherForecast>.Created(forecast, "Weather forecast berhasil dibuat"));
        }

        // Endpoint dengan JWT + Permission (Delete)
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Policy = "WEATHER_DELETE")]
        public IActionResult Delete(int id)
        {
            return Ok(ApiResponse<object>.Success(null!, $"Weather forecast {id} berhasil dihapus"));
        }

        // Endpoint yang support Both (JWT atau API Key)
        [HttpGet("universal")]
        [Authorize(AuthenticationSchemes = "Bearer,ApiKey")]
        public IActionResult GetUniversal()
        {
            var authType = User.FindFirst("AuthType")?.Value ?? "Unknown";
            var userName = User.Identity?.Name ?? "Anonymous";

            return Ok(ApiResponse<object>.Success(new
            {
                Message = "Akses berhasil",
                AuthenticationType = authType,
                User = userName,
                Timestamp = DateTime.UtcNow
            }));
        }
    }
}