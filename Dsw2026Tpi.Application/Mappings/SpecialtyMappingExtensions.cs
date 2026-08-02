using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Mappings;

public static class SpecialtyMappingExtensions
{
    public static SpecialtyModel.Response ToResponse(this Specialty entity)
    {
        return new SpecialtyModel.Response(entity.Id, entity.Name, entity.Description);
    }

    public static Specialty ToEntity(this SpecialtyModel.Request dto)
    {
        return new Specialty(dto.Name, dto.Description);
    }
}