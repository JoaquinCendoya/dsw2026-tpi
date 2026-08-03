using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Registra un nuevo usuario administrador (endpoint temporal para pruebas).
    /// </summary>
    /// <param name="request">Email y contraseña del nuevo administrador.</param>
    /// <response code="200">Usuario registrado exitosamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    [HttpPost("admin/register")]
    [AllowAnonymous]
    [EnableRateLimiting("DefaultPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterModel.Request request)
    {
        var result = await _authenticationService.Register(request);
        return Ok(result.Email);
    }

    /// <summary>
    /// Autentica a un administrador y devuelve un token JWT.
    /// </summary>
    /// <param name="request">Email y contraseña del administrador.</param>
    /// <response code="200">Autenticación exitosa, devuelve el token y el rol.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    [HttpPost("admin/login")]
    [AllowAnonymous]
    [EnableRateLimiting("AdminLoginPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginAdminModel.Request request)
    {
        var result = await _authenticationService.LoginAdmin(request);
        return Ok(result);
    }

    /// <summary>
    /// Autentica a un paciente (registrándolo automáticamente si es su primer acceso) y devuelve un token JWT.
    /// </summary>
    /// <param name="request">Email y DNI del paciente.</param>
    /// <response code="200">Autenticación exitosa, devuelve el token y el rol.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    [HttpPost("patient/login")]
    [AllowAnonymous]
    [EnableRateLimiting("PatientLoginPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginPatient([FromBody] LoginPatientModel.Request request)
    {
        var result = await _authenticationService.LoginPatient(request);
        return Ok(result);
    }
}