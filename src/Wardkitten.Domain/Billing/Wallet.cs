using Wardkitten.Domain.Common;

namespace Wardkitten.Domain.Billing;

/// <summary>
/// Monedero de créditos prepago para canales <em>metered</em> (SMS/WhatsApp). Es <b>independiente del
/// plan</b> (incluido Free): sin saldo suficiente, esos canales se desactivan; los gratuitos siguen.
/// No se permite saldo negativo. Feature: F06.01.
/// </summary>
public sealed class Wallet : Entity
{
    public string UserId { get; set; } = string.Empty;

    /// <summary>Saldo en créditos. 1 crédito = unidad interna; el coste por mensaje vive en ChannelRate.</summary>
    public decimal BalanceCredits { get; set; }

    /// <summary>Umbral mínimo: por debajo se avisa al usuario y conviene recargar.</summary>
    public decimal MinThresholdCredits { get; set; } = 10m;

    public bool AutoTopUpEnabled { get; set; }
    public decimal AutoTopUpAmountCredits { get; set; }

    public string Currency { get; set; } = "EUR";

    public bool CanAfford(decimal cost) => cost > 0 && BalanceCredits >= cost;
    public bool IsBelowThreshold => BalanceCredits < MinThresholdCredits;
}
