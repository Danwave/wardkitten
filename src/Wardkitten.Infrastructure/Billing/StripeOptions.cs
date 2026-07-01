namespace Wardkitten.Infrastructure.Billing;

/// <summary>Configuración de Stripe (inyectada por secret; ver SECURITY.md). Feature: F07.</summary>
public sealed class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>Price IDs de las suscripciones mensuales.</summary>
    public string PriceProMonthly { get; set; } = string.Empty;
    public string PriceTeamMonthly { get; set; } = string.Empty;

    /// <summary>
    /// Price ID del producto de créditos (per_unit; la cantidad = nº de créditos del lote).
    /// Si está vacío, se cae a un line item dinámico con <see cref="CreditUnitAmountCents"/>.
    /// </summary>
    public string PriceCredit { get; set; } = string.Empty;

    /// <summary>Precio (céntimos) de 1 crédito para el line item dinámico (fallback si no hay <see cref="PriceCredit"/>).</summary>
    public long CreditUnitAmountCents { get; set; } = 100;
    public string CreditCurrency { get; set; } = "eur";

    /// <summary>Comportamiento fiscal del line item dinámico de créditos: "inclusive" = el precio ya lleva IVA.</summary>
    public string CreditTaxBehavior { get; set; } = "inclusive";

    /// <summary>Activa el cálculo automático de impuestos (Stripe Tax) en los checkouts. Requiere Stripe Tax configurado.</summary>
    public bool AutomaticTaxEnabled { get; set; }
}
