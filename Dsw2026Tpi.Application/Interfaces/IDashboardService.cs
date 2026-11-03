using Dsw2026Tpi.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryModel> GetSummaryAsync(DateOnly date, CancellationToken cancellationToken);
    }
}
