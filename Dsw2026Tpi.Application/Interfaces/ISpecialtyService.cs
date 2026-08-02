using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ISpecialtyService
    {
        Task<Pagination<SpecialtyModel.Response>> GetAll(int pageSize, int pageIndex, string name);
        Task<SpecialtyModel.Response> Create(SpecialtyModel.Request request);
        Task<SpecialtyModel.Response> Update(Guid id, SpecialtyModel.Request request);
        Task Delete(Guid id);
    }
}
