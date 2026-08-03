using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/specialties")]
[Authorize(Policy = Policies.AdminPolicy)]
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
    /// <param name="pageSize">Cantidad de registros por página.</param>
    /// <param name="pageIndex">Número de página a consultar.</param>
    /// <param name="name">Filtro opcional por nombre de la especialidad.</param>
    /// <response code="200">Listado paginado de especialidades.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string name)
    {
        var specialties = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(specialties);
    }

    /// <summary>
    /// Registra una nueva especialidad médica.
    /// </summary>
    /// <param name="request">Nombre y descripción de la especialidad.</param>
    /// <response code="201">Especialidad creada exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialtyModel.Request request)
    {
        await _service.Update(id, request);
        return Ok();
    }

    /// <summary>
    /// Elimina lógicamente una especialidad del sistema.
    /// </summary>
    /// <param name="id">Id de la especialidad a eliminar.</param>
    /// <response code="200">Especialidad eliminada exitosamente.</response>
    /// <response code="404">No existe una especialidad con el Id indicado.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok();
    }
}