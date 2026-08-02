using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Mappings;

public static class DoctorMappingExtensions
{
    public static DoctorModel.Response ToResponse(this Doctor entity)
    {
        return new DoctorModel.Response(
            entity.Id,
            entity.Name,
            entity.LicenseNumber,
            new DoctorModel.SpecialityDto(entity.Speciality?.Id, entity.Speciality?.Name));
    }

    public static Doctor ToEntity(this DoctorModel.Request dto, Speciality speciality)
    {
        return new Doctor(dto.Name, dto.LicenseNumber, speciality);
    }
}