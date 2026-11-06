using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = Policies.AdminPolicy)]
[EnableRateLimiting("DefaultPolicy")]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public record SummaryResponse(int ActiveDoctors, int ActiveSpecialities, int ConfirmedAppointments);

    /// <summary>
    /// Devuelve el resumen para el panel de administración: cantidad de médicos activos,
    /// especialidades activas y turnos confirmados para la fecha indicada.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var doctors = await _unitOfWork.Repository<Doctor>().FindAsync(d => !d.Deleted);
        var specialties = await _unitOfWork.Repository<Specialty>().FindAsync(s => !s.Deleted);
        var appointments = await _unitOfWork.Repository<Appointment>().FindAsync(
            a => a.AvailabilitySlot.SlotDate == targetDate && a.Status == AppointmentStatus.Booked
        );

        return Ok(new SummaryResponse(
            ActiveDoctors: doctors.Count(),
            ActiveSpecialities: specialties.Count(),
            ConfirmedAppointments: appointments.Count()
        ));
    }
}
