using Wardkitten.Domain.Billing;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Application.Abstractions.Persistence;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken ct = default);
}

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<Wallet> GetOrCreateForUserAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Débito atómico y condicional: descuenta <paramref name="amount"/> solo si hay saldo suficiente.
    /// Devuelve el saldo resultante, o null si no había saldo (no se permite negativo). Feature: F06.01.
    /// </summary>
    Task<decimal?> TryDebitAsync(string userId, decimal amount, CancellationToken ct = default);

    /// <summary>Acredita saldo de forma atómica y devuelve el nuevo balance.</summary>
    Task<decimal> CreditAsync(string userId, decimal amount, CancellationToken ct = default);
}

public interface ICreditTransactionRepository : IRepository<CreditTransaction>
{
    Task<IReadOnlyList<CreditTransaction>> GetByUserAsync(string userId, int skip, int take, CancellationToken ct = default);
    Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
}

public interface IChannelRateRepository : IRepository<ChannelRate>
{
    /// <summary>Coste en créditos de un mensaje del canal hacia el destino (match por prefijo E.164).</summary>
    Task<decimal> GetCreditsPerMessageAsync(ChannelType channel, string? destination, CancellationToken ct = default);
}
