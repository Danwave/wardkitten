namespace Wardkitten.Domain.Watches;

/// <summary>
/// Asociación de un canal a un watch. Los canales son <b>apilables y personalizables por tarea</b>:
/// un watch puede tener varios bindings activos a la vez y cada uno con su propia configuración
/// (destino, orden de escalado, horas de silencio). Feature: F02.03.
/// </summary>
public sealed class ChannelBinding
{
    public ChannelType ChannelType { get; set; }

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Destino específico para esta tarea (email, teléfono E.164 o chatId de Telegram). Si es null se
    /// usa el destino por defecto del usuario para ese canal.
    /// </summary>
    public string? DestinationOverride { get; set; }

    /// <summary>Orden de escalado: los bindings con menor <see cref="Order"/> se notifican antes.</summary>
    public int Order { get; set; }

    /// <summary>
    /// Retardo (segundos) desde la apertura del incidente antes de disparar este binding. 0 = inmediato.
    /// Permite escalar (p.ej. Email ya, SMS a los 15 min si nadie hace ACK).
    /// </summary>
    public int EscalationDelaySeconds { get; set; }

    /// <summary>Franja horaria local en la que NO se notifica por este canal (opcional).</summary>
    public QuietHours? QuietHours { get; set; }

    public TimeSpan EscalationDelay => TimeSpan.FromSeconds(Math.Max(0, EscalationDelaySeconds));
}

/// <summary>Franja de silencio diaria, en minutos desde la medianoche local. Soporta cruzar medianoche.</summary>
public sealed class QuietHours
{
    public int StartMinute { get; set; }
    public int EndMinute { get; set; }

    /// <summary>¿La hora local cae dentro de la franja de silencio?</summary>
    public bool IsQuiet(DateTime localTime)
    {
        var minute = localTime.Hour * 60 + localTime.Minute;
        if (StartMinute == EndMinute)
            return false;
        return StartMinute < EndMinute
            ? minute >= StartMinute && minute < EndMinute
            : minute >= StartMinute || minute < EndMinute; // cruza medianoche
    }
}
