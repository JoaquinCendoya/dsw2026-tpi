using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ISpecialtyService
    {
        Task<Pagination<SpecialtyModel.Response>> GetAll(SpecialtyModel.SearchRequest request);
        Task<SpecialtyModel.Response> Create(SpecialtyModel.Request request);
        Task<SpecialtyModel.Response> Update(Guid id, SpecialtyModel.Request request);
        Task Delete(Guid id);
    }
}
