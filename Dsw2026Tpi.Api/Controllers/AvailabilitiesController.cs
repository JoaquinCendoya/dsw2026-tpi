using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminPolicy)]
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
    /// <response code="400">Los datos enviados no son válidos (rango de horarios inválido o solapado).</response>
    /// <response code="404">El médico indicado no existe.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAvailability([FromBody] AvailabilityModel.Request request)
    {
        var result = await _service.GenerateMonthlyAvailabilityAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Actualiza (sobrescribe) la disponibilidad horaria mensual de un médico.
    /// </summary>
    /// <param name="request">Médico y nueva configuración de días/horarios de atención.</param>
    /// <response code="200">Disponibilidad actualizada exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos (rango de horarios inválido o solapado).</response>
    /// <response code="404">El médico indicado no existe.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailability([FromBody] AvailabilityModel.Request request)
    {
        var result = await _service.GenerateMonthlyAvailabilityAsync(request);
        return Ok(result);
    }
}