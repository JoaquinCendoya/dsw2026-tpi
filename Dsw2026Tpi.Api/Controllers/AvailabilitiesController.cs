using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminPolicy)]
[EnableRateLimiting("DefaultPolicy")]
public class AvailabilitiesController : ControllerBase
{
    private readonly IAvailabilityService _service;

    public AvailabilitiesController(IAvailabilityService service)
    {
        _service = service;
    }

    /// <summary>
    /// Genera la disponibilidad horaria mensual de un médico según los días y rangos indicados.
    /// </summary>
    /// <param name="request">Médico y configuración de días/horarios de atención.</param>
    /// <response code="200">Disponibilidad generada exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos (rango de horarios inválido o solapado dentro del request).</response>
    /// <response code="404">El médico indicado no existe.</response>
    /// <response code="409">El horario solicitado se solapa con disponibilidad ya configurada para el médico.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAvailability([FromBody] AvailabilityModel.Request request)
    {
        var result = await _service.GenerateMonthlyAvailabilityAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Sobrescribe la disponibilidad horaria mensual de un médico. Los turnos ya reservados no se modifican.
    /// </summary>
    /// <param name="request">Médico y nueva configuración de días/horarios de atención.</param>
    /// <response code="200">Disponibilidad actualizada exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos (rango de horarios inválido o solapado dentro del request).</response>
    /// <response code="404">El médico indicado no existe.</response>
    /// <response code="409">El horario solicitado se solapa con turnos ya reservados para el médico.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAvailability([FromBody] AvailabilityModel.Request request)
    {
        var result = await _service.GenerateMonthlyAvailabilityAsync(request, overwrite: true);
        return Ok(result);
    }
}