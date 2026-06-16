using NCrontab;

namespace Wardkitten.Domain.Watches;

/// <summary>
/// Periodicidad de un watch. El cálculo del próximo vencimiento es <b>timezone-aware</b> y correcto
/// con cambios de hora (DST): los cron/calendar se interpretan en <see cref="TimeZoneId"/> (IANA) y se
/// convierten a UTC para almacenarse. Es lógica de dominio pura (testeable sin infraestructura).
/// </summary>
public sealed class Schedule
{
    public ScheduleKind Kind { get; set; } = ScheduleKind.Interval;

    /// <summary>Para <see cref="ScheduleKind.Interval"/>: segundos entre check-ins esperados.</summary>
    public int? IntervalSeconds { get; set; }

    /// <summary>Para <see cref="ScheduleKind.Cron"/>: expresión cron de 5 campos (min hora día mes dow).</summary>
    public string? CronExpression { get; set; }

    /// <summary>Para <see cref="ScheduleKind.Calendar"/>: fechas/horas locales esperadas.</summary>
    public List<DateTime> CalendarDatesLocal { get; set; } = new();

    /// <summary>Zona horaria IANA (p.ej. "Europe/Madrid"). En .NET 10 funciona en Windows y Linux.</summary>
    public string TimeZoneId { get; set; } = "UTC";

    public TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    /// <summary>
    /// Próximo vencimiento (UTC) estrictamente posterior a <paramref name="afterUtc"/>, o null si la
    /// configuración no produce más ocurrencias (p.ej. calendar agotado o cron inválido).
    /// </summary>
    public DateTime? ComputeNextDueUtc(DateTime afterUtc)
    {
        var tz = ResolveTimeZone();
        switch (Kind)
        {
            case ScheduleKind.Interval:
                if (IntervalSeconds is null or <= 0)
                    return null;
                return afterUtc.AddSeconds(IntervalSeconds.Value);

            case ScheduleKind.Cron:
                if (string.IsNullOrWhiteSpace(CronExpression))
                    return null;
                var cron = CrontabSchedule.TryParse(CronExpression);
                if (cron is null)
                    return null;
                var localAfter = TimeZoneInfo.ConvertTimeFromUtc(afterUtc, tz);
                var nextLocal = cron.GetNextOccurrence(localAfter);
                return LocalToUtc(nextLocal, tz);

            case ScheduleKind.Calendar:
                var localAfterCal = TimeZoneInfo.ConvertTimeFromUtc(afterUtc, tz);
                var next = CalendarDatesLocal
                    .Where(d => d > localAfterCal)
                    .OrderBy(d => d)
                    .Cast<DateTime?>()
                    .FirstOrDefault();
                return next is null ? null : LocalToUtc(next.Value, tz);

            default:
                return null;
        }
    }

    /// <summary>Convierte hora local a UTC tolerando los huecos de DST (spring-forward).</summary>
    private static DateTime LocalToUtc(DateTime local, TimeZoneInfo tz)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (tz.IsInvalidTime(unspecified))
            unspecified = unspecified.AddHours(1); // salta el hueco de cambio de hora
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
    }

    public bool IsValid(out string? error)
    {
        switch (Kind)
        {
            case ScheduleKind.Interval when IntervalSeconds is null or <= 0:
                error = "IntervalSeconds debe ser mayor que 0.";
                return false;
            case ScheduleKind.Cron when string.IsNullOrWhiteSpace(CronExpression) || CrontabSchedule.TryParse(CronExpression) is null:
                error = "CronExpression no es una expresión cron válida de 5 campos.";
                return false;
            case ScheduleKind.Calendar when CalendarDatesLocal.Count == 0:
                error = "Calendar requiere al menos una fecha.";
                return false;
            default:
                error = null;
                return true;
        }
    }
}
