using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record PaginationModel
    {
        public record Response<T>(int PageSize, int PageIndex, int Total, IEnumerable<T> Data);
    }
}
