namespace Dsw2026Tpi.Application.Models;

public record RegisterModel
{
    public record Request(string Email, string Password);
    public record Response(string Email);
}
