using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Mappings;

public static class SpecialityMappingExtensions
{
    public static SpecialityModel.Response ToResponse(this Speciality entity)
    {
        return new SpecialityModel.Response(entity.Id, entity.Name, entity.Description);
    }

    public static Speciality ToEntity(this SpecialityModel.Request dto)
    {
        return new Speciality(dto.Name, dto.Description);
    }
}