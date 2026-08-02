using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/doctors")]
[Authorize(Policy = Policies.AdminPolicy)]
public class DoctorController : AppController
{
    private readonly IDoctorService _service;

    public DoctorController(IDoctorService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string? name = null)
    {
        var result = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(result);
    }

    [HttpGet("{id:guid}/availabilities")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DoctorModel.AvailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailabilities([FromRoute] Guid id)
    {
        var result = await _service.GetAvailabilities(id);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DoctorModel.Response), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] DoctorModel.Request request)
    {
        var result = await _service.Create(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DoctorModel.Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] DoctorModel.Request request)
    {
        var result = await _service.Update(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _service.Delete(id);
        return Ok("ok");
    }

}
