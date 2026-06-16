using Wardkitten.Domain.Common;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Domain.Incidents;

/// <summary>Un escalón de escalado: tras <see cref="DelaySeconds"/> sin ACK, notifica por estos canales.</summary>
public sealed class EscalationStep
{
    public int Order { get; set; }

    /// <summary>Retardo desde la apertura del incidente antes de activar este escalón.</summary>
    public int DelaySeconds { get; set; }

    public List<ChannelType> Channels { get; set; } = new();

    /// <summary>Escalar a otra persona (id de usuario) además del titular. Opcional.</summary>
    public string? NotifyUserId { get; set; }

    public TimeSpan Delay => TimeSpan.FromSeconds(Math.Max(0, DelaySeconds));
}

/// <summary>
/// Política de escalado reutilizable: si nadie reconoce el incidente, se sube de escalón. Disponible en
/// planes de pago. Feature: F04.02.
/// </summary>
public sealed class EscalationPolicy : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<EscalationStep> Steps { get; set; } = new();
}
