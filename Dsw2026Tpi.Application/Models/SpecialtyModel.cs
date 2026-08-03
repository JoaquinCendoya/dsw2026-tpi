namespace Dsw2026Tpi.Application.Models;

    public record SpecialtyModel
    {
        public record Request(string Name, string Description);

        public record Response(Guid Id, string Name, string Description);
    }

