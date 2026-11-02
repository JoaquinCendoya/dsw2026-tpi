using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Models
{
    public record DashboardSummaryModel(
        int ActiveDoctors,
        int ActiveSpecialities,
        int ConfirmedAppointments);
}
