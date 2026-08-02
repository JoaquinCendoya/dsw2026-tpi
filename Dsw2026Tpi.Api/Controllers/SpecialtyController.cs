using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string name)
    {
        var specialties = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(specialties);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SpecialtyModel.Request request)
    {
        var specialty = await _service.Create(request);
        return CreatedAtAction(nameof(GetAll), new { id = specialty.Id }, specialty);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialtyModel.Request request)
    {
        await _service.Update(id, request);
        return Ok();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok();
    }
}