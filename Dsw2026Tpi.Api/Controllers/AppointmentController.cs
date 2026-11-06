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
    [HttpPost]
    [Authorize(Policy = Policies.PatientPolicy)]
    [EnableRateLimiting("AppointmentPolicy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookAsync([FromBody] AppointmentModel.Request request)
    {
        if (!TryGetAuthenticatedPatientDni(out var tokenDni))
        {
            return ForbiddenPatientIdentity("No se pudo resolver la identidad del paciente a través del token provisto.");
        }

        if (request.Patient?.Dni != tokenDni)
        {
            return ForbiddenPatientIdentity("El DNI enviado no coincide con el paciente autenticado.");
        }

        var response = await _service.BookAsync(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Obtiene los turnos activos del paciente autenticado de manera segura usando su Token JWT con soporte de paginación.
    /// </summary>
    /// <response code="200">Listado paginado de turnos activos del paciente.</response>
    /// <response code="403">No se pudo validar la identidad del paciente desde el token.</response>
    [HttpGet("patient")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByPatientAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1)
    {
        // VALIDACIÓN DE PACIENTE: Extraer el DNI desde el Claim del token JWT para evitar suplantación de identidad (IDOR)
        if (!TryGetAuthenticatedPatientDni(out var tokenDni))
        {
            return ForbiddenPatientIdentity("No se pudo resolver la identidad del paciente a través del token provisto.");
        }

        // Se envía a la capa de aplicación el DNI extraído del token de forma segura al igual que los datos de paginado
        var response = await _service.GetByPatientDniAsync(new AppointmentModel.PatientDto(tokenDni), pageSize, pageIndex);
        return Ok(response);
    }

    /// <summary>
    /// Cancela un turno reservado validando la pertenencia al paciente autenticado.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelAsync([FromRoute] Guid id)
    {
        // VALIDACIÓN DE PACIENTE: Extraer el DNI desde el Claim del token JWT
        if (!TryGetAuthenticatedPatientDni(out var tokenDni))
        {
            return ForbiddenPatientIdentity("No se pudo resolver la identidad del paciente a través del token provisto.");
        }

        // Se delega al servicio el ID del turno y el DNI seguro para constatar que le pertenece previo a la cancelación
        await _service.CancelAsync(id, tokenDni);
        return Ok("ok");
    }

    /// <summary>
    /// Obtiene los turnos de una fecha específica (uso administrativo).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDateAsync([FromQuery] AppointmentModel.DailyRequest request)
    {
        var response = await _service.GetByDateAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Búsqueda avanzada de turnos combinando filtros ampliados con rango de fechas y estados (uso administrativo).
    /// </summary>
    /// <param name="request">Datos de la consulta enriquecidos (pageSize, pageIndex, specialtyId, doctorId, dni, startDate, endDate, status, orden).</param>
    /// <response code="200">Listado paginado de turnos según los filtros aplicados con datos temporales.</response>
    /// <response code="400">El rango de fechas provisto es inválido.</response>
    [HttpGet("search")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAsync([FromQuery] AppointmentModel.SearchRequest request)
    {
        // Validación preventiva de rango consistente utilizando DateOnly?
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate.Value > request.EndDate.Value)
        {
            return BadRequest(new ErrorResponse(
                "INVALID_DATE_RANGE",
                "La fecha de inicio (StartDate) no puede ser posterior a la fecha de fin (EndDate)."
            ));
        }

        var response = await _service.SearchAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Registra la asistencia o ausencia a un turno médico (uso administrativo).
    /// </summary>
    [HttpPut("{id:guid}/attendance")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAttendanceAsync([FromRoute] Guid id, [FromBody] AppointmentModel.AttendanceRequest request)
    {
        await _service.MarkAttendanceAsync(id, request.Attended);
        return Ok("ok");
    }

    private bool TryGetAuthenticatedPatientDni(out long patientDni)
    {
        var dniClaim = User.FindFirst(IdentityClaims.PatientDni)?.Value;
        return long.TryParse(dniClaim, out patientDni) && patientDni > 0;
    }

    private ObjectResult ForbiddenPatientIdentity(string message)
    {
        return StatusCode(
            StatusCodes.Status403Forbidden,
            new ErrorResponse("FORBIDDEN_IDENTITY", message));
    }
}
