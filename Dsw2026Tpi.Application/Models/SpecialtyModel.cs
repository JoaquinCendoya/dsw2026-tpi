namespace Dsw2026Tpi.Application.Models;

    public record SpecialtyModel
    {
        public record Request(string Name, string Description);

        public record Response(Guid Id, string Name, string Description);
        
        public record SearchRequest(int PageSize, int PageIndex, string? Name = null, bool Descending = false);
        public record MetricsResponse(
            int TotalSpecialties,
            int QuarterlyGrowthCount,
            string? MostDemandedSpecialtyName,
            int MostDemandedAppointmentsCount,
            double MostDemandedPercentage,
            int NewThisMonthCount,
            IEnumerable<string> NewThisMonthNames);
}

