using Dsw2026Tpi.Api.Controllers;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dsw2026Tpi.Tests;

public class DashboardControllerTests
{
    private readonly IDashboardService _service = Substitute.For<IDashboardService>();

    [Fact]
    public async Task GetSummary_UsaLaFechaActualDeArgentina_CuandoNoSeIndicaFecha()
    {
        var expectedDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(
                DateTimeOffset.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Tucuman")
            ).DateTime
        );
        _service.GetSummaryAsync(expectedDate, Arg.Any<CancellationToken>())
            .Returns(new DashboardSummaryModel(12, 4, 3));
        var controller = new DashboardController(_service);

        var result = await controller.GetSummary();

        result.Should().BeOfType<OkObjectResult>();
        await _service.Received(1).GetSummaryAsync(expectedDate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSummary_ConservaLaFechaIndicada()
    {
        var requestedDate = new DateOnly(2026, 11, 17);
        _service.GetSummaryAsync(requestedDate, Arg.Any<CancellationToken>())
            .Returns(new DashboardSummaryModel(12, 4, 3));
        var controller = new DashboardController(_service);

        var result = await controller.GetSummary(requestedDate);

        result.Should().BeOfType<OkObjectResult>();
        await _service.Received(1).GetSummaryAsync(requestedDate, Arg.Any<CancellationToken>());
    }
}
