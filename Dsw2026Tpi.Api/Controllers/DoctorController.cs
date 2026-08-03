using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/doctors")]
[Authorize(Policy = Policies.AdminPolicy)]
[EnableRateLimiting("DefaultPolicy")]
public class DoctorController : AppController
{
    private readonly IDoctorService _service;

    public DoctorController(IDoctorService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene un listado paginado de médicos activos.
    /// </summary>
    /// <param name="request">Parámetros de búsqueda, paginación y orden (nombre asc/desc).</param>
    /// <response code="200">Listado paginado de médicos.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] DoctorModel.SearchRequest request)
    {
        var result = await _service.GetAll(request);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene la disponibilidad horaria mensual de un médico.
    /// </summary>
    /// <param name="id">Id del médico.</param>
    /// <response code="200">Disponibilidad horaria por día de la semana.</response>
    [HttpGet("{id:guid}/availabilities")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(typeof(IEnumerable<DoctorModel.AvailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailabilities([FromRoute] Guid id)
    {
        var result = await _service.GetAvailabilities(id);
        return Ok(result);
    }

    /// <summary>
    /// Registra un nuevo médico en el sistema.
    /// </summary>
    /// <param name="request">Datos del médico a crear (nombre, matrícula y especialidad).</param>
    /// <returns>El médico creado, incluyendo su Id generado.</returns>
    /// <response code="201">Médico creado exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos (ver <c>details</c> en la respuesta).</response>
    [HttpPost]
    [ProducesResponseType(typeof(DoctorModel.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DoctorModel.Request request)
    {
        var result = await _service.Create(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Actualiza los datos de un médico existente.
    /// </summary>
    /// <param name="id">Id del médico a actualizar.</param>
    /// <param name="request">Nuevos datos del médico.</param>
    /// <response code="200">Médico actualizado exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No existe un médico con el Id indicado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DoctorModel.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] DoctorModel.Request request)
    {
        var result = await _service.Update(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Elimina lógicamente un médico del sistema.
    /// </summary>
    /// <param name="id">Id del médico a eliminar.</param>
    /// <response code="200">Médico eliminado exitosamente.</response>
    /// <response code="404">No existe un médico con el Id indicado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _service.Delete(id);
        return Ok("ok");
    }
}