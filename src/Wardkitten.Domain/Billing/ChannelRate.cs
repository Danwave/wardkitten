using Wardkitten.Domain.Common;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Domain.Billing;

/// <summary>
/// Tarifa en créditos de un canal metered, opcionalmente por prefijo de país E.164 (el SMS varía mucho
/// por destino). El prefijo "*" es la tarifa por defecto. Feature: F06.03.
/// </summary>
public sealed class ChannelRate : Entity
{
    public ChannelType Channel { get; set; }

    /// <summary>Prefijo E.164 (p.ej. "+34") o "*" para la tarifa por defecto.</summary>
    public string CountryPrefix { get; set; } = "*";

    public decimal CreditsPerMessage { get; set; }
}
