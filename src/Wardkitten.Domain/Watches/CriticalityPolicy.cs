namespace Wardkitten.Domain.Watches;

/// <summary>
/// Política asociada a un grado de criticidad (<see cref="Severity"/>): además de etiqueta y color para
/// la UI, define comportamiento real. Por ejemplo, los grados altos <b>se saltan las horas de silencio</b>
/// para que una urgencia siempre avise. Feature: F13.01.
/// </summary>
public sealed record CriticalityPolicy(Severity Severity, string Label, string Color, bool BypassQuietHours);

public static class CriticalityCatalog
{
    private static readonly Dictionary<Severity, CriticalityPolicy> Policies = new()
    {
        [Severity.Low] = new(Severity.Low, "Baja", "#64748b", BypassQuietHours: false),
        [Severity.Medium] = new(Severity.Medium, "Media", "#0ea5e9", BypassQuietHours: false),
        [Severity.High] = new(Severity.High, "Alta", "#f59e0b", BypassQuietHours: true),
        [Severity.Critical] = new(Severity.Critical, "Crítica", "#dc2626", BypassQuietHours: true),
    };

    public static CriticalityPolicy For(Severity severity) => Policies[severity];
}
