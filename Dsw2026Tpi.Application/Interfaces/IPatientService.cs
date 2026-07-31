using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IPatientService
    {
        Task<Patient> GetByUserIdAsync(Guid userId);
    }
}
