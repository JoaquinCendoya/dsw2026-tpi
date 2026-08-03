using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Mappings;

public static class PatientMappingExtensions
{
    public static PatientModel.Response ToResponse(this Patient entity)
    {
        return new PatientModel.Response(entity.Id, entity.Dni, entity.FullName, entity.Phone);
    }
}
