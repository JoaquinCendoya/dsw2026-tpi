namespace Dsw2026Tpi.Application.Common;

public static class SpanishDayOfWeek
{
    private static readonly Dictionary<string, DayOfWeek> Days = new()
    {
        ["LUNES"] = DayOfWeek.Monday,
        ["MARTES"] = DayOfWeek.Tuesday,
        ["MIERCOLES"] = DayOfWeek.Wednesday,
        ["JUEVES"] = DayOfWeek.Thursday,
        ["VIERNES"] = DayOfWeek.Friday,
        ["SABADO"] = DayOfWeek.Saturday,
        ["DOMINGO"] = DayOfWeek.Sunday,
    };

    public static bool TryParse(string value, out DayOfWeek dayOfWeek) =>
        Days.TryGetValue(value.ToUpperInvariant(), out dayOfWeek);

    public static bool IsValid(string value) => Days.ContainsKey(value.ToUpperInvariant());

    public static string ToString(DayOfWeek day)
    {
        var match = Days.FirstOrDefault(x => x.Value == day);
        return match.Key ?? day.ToString();
    }
}
