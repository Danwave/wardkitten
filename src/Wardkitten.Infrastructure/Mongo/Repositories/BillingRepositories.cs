using MongoDB.Driver;
using Wardkitten.Application.Abstractions;
using Wardkitten.Application.Abstractions.Persistence;
using Wardkitten.Domain.Billing;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Infrastructure.Mongo.Repositories;

public sealed class SubscriptionRepository : MongoRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(MongoContext ctx, IClock clock) : base(ctx.Subscriptions, clock) { }

    public async Task<Subscription?> GetByUserAsync(string userId, CancellationToken ct = default)
        => await Collection.Find(Builders<Subscription>.Filter.Eq(s => s.UserId, userId)).FirstOrDefaultAsync(ct);

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken ct = default)
        => await Collection.Find(Builders<Subscription>.Filter.Eq(s => s.StripeSubscriptionId, stripeSubscriptionId)).FirstOrDefaultAsync(ct);
}

public sealed class WalletRepository : MongoRepository<Wallet>, IWalletRepository
{
    public WalletRepository(MongoContext ctx, IClock clock) : base(ctx.Wallets, clock) { }

    public async Task<Wallet?> GetByUserAsync(string userId, CancellationToken ct = default)
        => await Collection.Find(Builders<Wallet>.Filter.Eq(w => w.UserId, userId)).FirstOrDefaultAsync(ct);

    public async Task<Wallet> GetOrCreateForUserAsync(string userId, CancellationToken ct = default)
    {
        var existing = await GetByUserAsync(userId, ct);
        if (existing is not null) return existing;

        var wallet = new Wallet { UserId = userId, BalanceCredits = 0m };
        try
        {
            await InsertAsync(wallet, ct);
            return wallet;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Carrera: otra petición la creó primero. Releemos la existente.
            return (await GetByUserAsync(userId, ct))!;
        }
    }

    public async Task<decimal?> TryDebitAsync(string userId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) return await GetBalanceAsync(userId, ct);

        var filter = Builders<Wallet>.Filter.And(
            Builders<Wallet>.Filter.Eq(w => w.UserId, userId),
            Builders<Wallet>.Filter.Gte(w => w.BalanceCredits, amount));
        var update = Builders<Wallet>.Update
            .Inc(w => w.BalanceCredits, -amount)
            .Set(w => w.UpdatedAtUtc, Clock.UtcNow);

        var updated = await Collection.FindOneAndUpdateAsync(
            filter, update,
            new FindOneAndUpdateOptions<Wallet> { ReturnDocument = ReturnDocument.After }, ct);

        return updated?.BalanceCredits; // null => saldo insuficiente
    }

    public async Task<decimal> CreditAsync(string userId, decimal amount, CancellationToken ct = default)
    {
        await GetOrCreateForUserAsync(userId, ct);
        var update = Builders<Wallet>.Update
            .Inc(w => w.BalanceCredits, amount)
            .Set(w => w.UpdatedAtUtc, Clock.UtcNow);
        var updated = await Collection.FindOneAndUpdateAsync(
            Builders<Wallet>.Filter.Eq(w => w.UserId, userId), update,
            new FindOneAndUpdateOptions<Wallet> { ReturnDocument = ReturnDocument.After }, ct);
        return updated!.BalanceCredits;
    }

    private async Task<decimal?> GetBalanceAsync(string userId, CancellationToken ct)
        => (await GetByUserAsync(userId, ct))?.BalanceCredits;
}

public sealed class CreditTransactionRepository : MongoRepository<CreditTransaction>, ICreditTransactionRepository
{
    public CreditTransactionRepository(MongoContext ctx, IClock clock) : base(ctx.CreditTransactions, clock) { }

    public async Task<IReadOnlyList<CreditTransaction>> GetByUserAsync(string userId, int skip, int take, CancellationToken ct = default)
        => await Collection.Find(Builders<CreditTransaction>.Filter.Eq(t => t.UserId, userId))
                           .SortByDescending(t => t.CreatedAtUtc)
                           .Skip(skip).Limit(take)
                           .ToListAsync(ct);

    public async Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => await Collection.CountDocumentsAsync(
               Builders<CreditTransaction>.Filter.Eq(t => t.IdempotencyKey, idempotencyKey),
               new CountOptions { Limit = 1 }, ct) > 0;
}

public sealed class ChannelRateRepository : MongoRepository<ChannelRate>, IChannelRateRepository
{
    // Tarifas por defecto si la BBDD no tiene rate card configurada (créditos por mensaje).
    private static readonly Dictionary<ChannelType, decimal> Defaults = new()
    {
        [ChannelType.Sms] = 1m,
        [ChannelType.WhatsApp] = 1m,
    };

    public ChannelRateRepository(MongoContext ctx, IClock clock) : base(ctx.ChannelRates, clock) { }

    public async Task<decimal> GetCreditsPerMessageAsync(ChannelType channel, string? destination, CancellationToken ct = default)
    {
        var rates = await Collection.Find(Builders<ChannelRate>.Filter.Eq(r => r.Channel, channel)).ToListAsync(ct);

        // Match por prefijo E.164 más largo; si no, tarifa "*"; si no hay nada, default del código.
        var match = rates
            .Where(r => r.CountryPrefix == "*" || (destination is not null && destination.StartsWith(r.CountryPrefix, StringComparison.Ordinal)))
            .OrderByDescending(r => r.CountryPrefix == "*" ? 0 : r.CountryPrefix.Length)
            .FirstOrDefault();

        if (match is not null) return match.CreditsPerMessage;
        return Defaults.TryGetValue(channel, out var d) ? d : 0m;
    }
}
