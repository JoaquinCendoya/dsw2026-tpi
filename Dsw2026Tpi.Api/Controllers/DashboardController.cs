using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [EnableRateLimiting("DefaultPolicy")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public class DashboardController : ControllerBase
    {
        private static readonly TimeZoneInfo ProjectTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Tucuman");

        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateOnly? date = null,
            CancellationToken cancellationToken = default)
        {
            var summaryDate = date ?? GetCurrentDate();
            var result = await _dashboardService.GetSummaryAsync(summaryDate, cancellationToken);
            return Ok(result);
        }

        private static DateOnly GetCurrentDate()
        {
            var projectTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ProjectTimeZone);
            return DateOnly.FromDateTime(projectTime.DateTime);
        }
    }
}
