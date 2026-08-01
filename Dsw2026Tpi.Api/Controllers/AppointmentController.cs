using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = Policies.PatientPolicy)]
    [EnableRateLimiting("AppointmentPolicy")]
    public async Task<IActionResult> BookAsync([FromBody] AppointmentModel.Request request)
    {
        var response = await _service.BookAsync(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("patient")]
    [Authorize(Policy = Policies.PatientPolicy)]
    public async Task<IActionResult> GetByPatientAsync([FromQuery] long dni)
    {
        var response = await _service.GetByPatientDniAsync(new AppointmentModel.PatientDto(dni));
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.PatientPolicy)]
    public async Task<IActionResult> CancelAsync([FromRoute] Guid id)
    {
        await _service.CancelAsync(id);
        return Ok("ok");
    }

    [HttpGet]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> GetByDateAsync([FromQuery] DateOnly date, [FromQuery] int pageSize, [FromQuery] int pageIndex)
    {
        var response = await _service.GetByDateAsync(date, pageSize, pageIndex);
        return Ok(response);
    }

    [HttpGet("search")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<IActionResult> SearchAsync([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] Guid? specialtyId, [FromQuery] Guid? doctorId, [FromQuery] long? dni, [FromQuery] DateOnly? date)
    {
        AppointmentModel.PatientDto? patientDto = dni.HasValue ? new AppointmentModel.PatientDto(dni.Value) : null;
        var response = await _service.SearchAsync(pageSize, pageIndex, specialtyId, doctorId, patientDto, date);
        return Ok(response);
    }
}