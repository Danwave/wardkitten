using Wardkitten.Domain.Common;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Domain.Notifications;

/// <summary>
/// Registro de toda notificación saliente (alerta, ACK, aviso de saldo bajo, sistema). Sirve para
/// auditoría, depuración y conciliación de consumos metered. Feature: F05.04.
/// </summary>
public sealed class NotificationLog : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string? WatchId { get; set; }
    public string? IncidentId { get; set; }

    public ChannelType Channel { get; set; }
    public string Destination { get; set; } = string.Empty;

    /// <summary>Tipo lógico: "alert", "low-balance", "ack", "system"…</summary>
    public string Kind { get; set; } = "alert";

    public bool Success { get; set; }
    public decimal CreditsCharged { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? Error { get; set; }

    public DateTime SentAtUtc { get; set; }
}
