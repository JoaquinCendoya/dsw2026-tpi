using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/specialties")]
[EnableRateLimiting("DefaultPolicy")]
public class SpecialtyController : ControllerBase
{
    private readonly ISpecialtyService _service;

    public SpecialtyController(ISpecialtyService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene un listado paginado de especialidades activas.
    /// </summary>
    /// <param name="request">Parámetros de búsqueda, paginación y orden (nombre asc/desc).</param>
    /// <response code="200">Listado paginado de especialidades.</response>
    /// <response code="401">No se proporcionó un token válido.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] SpecialtyModel.SearchRequest request)
    {
        var specialties = await _service.GetAll(request);
        return Ok(specialties);
    }

    /// <summary>
    /// Registra una nueva especialidad médica.
    /// </summary>
    /// <param name="request">Nombre y descripción de la especialidad.</param>
    /// <response code="201">Especialidad creada exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="401">No se proporcionó un token válido.</response>
    /// <response code="403">El usuario no tiene rol administrador.</response>
    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] SpecialtyModel.Request request)
    {
        var specialty = await _service.Create(request);
        return CreatedAtAction(nameof(GetAll), new { id = specialty.Id }, specialty);
    }

    /// <summary>
    /// Actualiza los datos de una especialidad existente.
    /// </summary>
    /// <param name="id">Id de la especialidad a actualizar.</param>
    /// <param name="request">Nuevos datos de la especialidad.</param>
    /// <response code="200">Especialidad actualizada exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No existe una especialidad con el Id indicado.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialtyModel.Request request)
    {
        var result = await _service.Update(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Elimina lógicamente una especialidad del sistema.
    /// </summary>
    /// <param name="id">Id de la especialidad a eliminar.</param>
    /// <response code="200">Especialidad eliminada exitosamente.</response>
    /// <response code="404">No existe una especialidad con el Id indicado.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok("ok");
    }
}
