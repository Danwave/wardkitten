using MongoDB.Driver;
using Wardkitten.Application.Abstractions;
using Wardkitten.Application.Abstractions.Persistence;
using Wardkitten.Domain.Leasing;

namespace Wardkitten.Infrastructure.Mongo;

/// <summary>
/// Lock distribuido sobre Mongo. Patrón de upsert condicional: se adquiere si el lease está libre,
/// expirado o ya es nuestro; si lo tiene otro y sigue vigente, el upsert provoca duplicate-key y se
/// interpreta como "no adquirido". Evita alertas duplicadas con varias réplicas del worker.
/// </summary>
public sealed class MongoLeaseStore : ILeaseStore
{
    private readonly IMongoCollection<Lease> _collection;
    private readonly IClock _clock;

    public MongoLeaseStore(MongoContext ctx, IClock clock)
    {
        _collection = ctx.Leases;
        _clock = clock;
    }

    public async Task<bool> TryAcquireAsync(string resource, string holder, TimeSpan ttl, CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var expires = now.Add(ttl);

        var filter = Builders<Lease>.Filter.And(
            Builders<Lease>.Filter.Eq(l => l.Id, resource),
            Builders<Lease>.Filter.Or(
                Builders<Lease>.Filter.Lte(l => l.ExpiresAtUtc, now),
                Builders<Lease>.Filter.Eq(l => l.Holder, holder)));

        var update = Builders<Lease>.Update
            .Set(l => l.Holder, holder)
            .Set(l => l.AcquiredAtUtc, now)
            .Set(l => l.ExpiresAtUtc, expires);

        try
        {
            var result = await _collection.FindOneAndUpdateAsync(filter, update,
                new FindOneAndUpdateOptions<Lease> { IsUpsert = true, ReturnDocument = ReturnDocument.After }, ct);
            return result is not null && result.Holder == holder;
        }
        catch (MongoCommandException ex) when (ex.Code == 11000)
        {
            return false;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task<bool> RenewAsync(string resource, string holder, TimeSpan ttl, CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var filter = Builders<Lease>.Filter.And(
            Builders<Lease>.Filter.Eq(l => l.Id, resource),
            Builders<Lease>.Filter.Eq(l => l.Holder, holder));
        var update = Builders<Lease>.Update.Set(l => l.ExpiresAtUtc, now.Add(ttl));
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task ReleaseAsync(string resource, string holder, CancellationToken ct = default)
    {
        var filter = Builders<Lease>.Filter.And(
            Builders<Lease>.Filter.Eq(l => l.Id, resource),
            Builders<Lease>.Filter.Eq(l => l.Holder, holder));
        await _collection.DeleteOneAsync(filter, ct);
    }
}
