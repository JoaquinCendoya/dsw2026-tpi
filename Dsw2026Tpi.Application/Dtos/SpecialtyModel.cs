using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public static class SpecialtyModel
    {
        public record Request(string Name, string Description);

        public record Response(Guid Id, string Name, string Description);
    }
}
