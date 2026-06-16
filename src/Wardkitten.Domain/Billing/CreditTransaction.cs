using Wardkitten.Domain.Common;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Domain.Billing;

public enum CreditTransactionType
{
    TopUp = 0,        // recarga vía Stripe
    Consumption = 1,  // envío de SMS/WhatsApp
    Adjustment = 2,   // ajuste manual / soporte
    Refund = 3,
    AutoTopUp = 4,
}

/// <summary>Movimiento de la wallet. Asiento contable inmutable con el saldo resultante. Feature: F06.02.</summary>
public sealed class CreditTransaction : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string WalletId { get; set; } = string.Empty;

    public CreditTransactionType Type { get; set; }

    /// <summary>Importe con signo: positivo en recargas, negativo en consumos.</summary>
    public decimal AmountCredits { get; set; }

    /// <summary>Saldo tras aplicar el movimiento.</summary>
    public decimal BalanceAfter { get; set; }

    public string? Reason { get; set; }
    public ChannelType? Channel { get; set; }

    /// <summary>Referencia externa: PaymentIntent de Stripe o SID de Twilio.</summary>
    public string? ProviderReference { get; set; }

    /// <summary>Clave de idempotencia para no duplicar consumos/recargas ante reintentos.</summary>
    public string? IdempotencyKey { get; set; }
}
