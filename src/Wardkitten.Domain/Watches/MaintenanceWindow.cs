namespace Wardkitten.Domain.Watches;

/// <summary>
/// Ventana de mantenimiento: intervalo (UTC) durante el cual el watch no se evalúa ni alerta
/// (vacaciones, parada planificada…). No desactiva la vigilancia, solo la silencia temporalmente.
/// </summary>
public sealed class MaintenanceWindow
{
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? Reason { get; set; }

    public bool Contains(DateTime utc) => utc >= StartUtc && utc <= EndUtc;
}
