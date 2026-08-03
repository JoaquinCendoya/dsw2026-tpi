using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[EnableRateLimiting("DefaultPolicy")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Reserva un turno médico para un paciente.
    /// </summary>
    /// <param name="request">Datos de la reserva (médico, disponibilidad, paciente y motivo).</param>
    /// <response code="201">Turno reservado exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">El médico o la disponibilidad indicados no existen.</response>
    /// <response code="409">El turno ya fue reservado por otro paciente.</response>
    [HttpPost]
    [Authorize(Policy = Policies.PatientPolicy)]
    [EnableRateLimiting("AppointmentPolicy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookAsync([FromBody] AppointmentModel.Request request)
    {
        var response = await _service.BookAsync(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Obtiene los turnos activos de un paciente según su DNI.
    /// </summary>
    /// <param name="dni">DNI del paciente.</param>
    /// <response code="200">Listado de turnos activos del paciente.</response>
    [HttpGet("patient")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatientAsync([FromQuery] long dni)
    {
        var response = await _service.GetByPatientDniAsync(new AppointmentModel.PatientDto(dni));
        return Ok(response);
    }

    /// <summary>
    /// Cancela un turno reservado.
    /// </summary>
    /// <param name="id">Id del turno a cancelar.</param>
    /// <response code="200">Turno cancelado exitosamente.</response>
    /// <response code="404">No existe un turno con el Id indicado.</response>
    /// <response code="409">El turno no puede cancelarse en su estado actual.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelAsync([FromRoute] Guid id)
    {
        await _service.CancelAsync(id);
        return Ok("ok");
    }

    /// <summary>
    /// Obtiene los turnos de una fecha específica (uso administrativo).
    /// </summary>
    /// <param name="request">Datos de la consulta (fecha, tamaño de página, número de página y orden).</param>
    /// <response code="200">Listado paginado de turnos de la fecha indicada.</response>
    [HttpGet]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDateAsync([FromQuery] AppointmentModel.DailyRequest request)
    {
        var response = await _service.GetByDateAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Búsqueda avanzada de turnos combinando filtros (uso administrativo).
    /// </summary>
    /// <param name="request">Datos de la consulta (tamaño de página, número de página, filtros y orden).</param>
    /// <response code="200">Listado paginado de turnos según los filtros aplicados.</response>
    [HttpGet("search")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAsync([FromQuery] AppointmentModel.SearchRequest request)
    {
        var response = await _service.SearchAsync(request);
        return Ok(response);
    }
}