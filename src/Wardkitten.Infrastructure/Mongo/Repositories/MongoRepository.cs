using MongoDB.Driver;
using Wardkitten.Application.Abstractions;
using Wardkitten.Application.Abstractions.Persistence;
using Wardkitten.Domain.Common;

namespace Wardkitten.Infrastructure.Mongo.Repositories;

/// <summary>Base genérica de repositorios: asigna Id (GUID) y marcas de tiempo, y expone CRUD básico.</summary>
public class MongoRepository<T> : IRepository<T> where T : Entity
{
    protected readonly IMongoCollection<T> Collection;
    protected readonly IClock Clock;

    public MongoRepository(IMongoCollection<T> collection, IClock clock)
    {
        Collection = collection;
        Clock = clock;
    }

    protected static FilterDefinition<T> ById(string id) => Builders<T>.Filter.Eq(e => e.Id, id);

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
        => await Collection.Find(ById(id)).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
        => await Collection.Find(FilterDefinition<T>.Empty).ToListAsync(ct);

    public async Task InsertAsync(T entity, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = Guid.NewGuid().ToString("N");

        var now = Clock.UtcNow;
        if (entity.CreatedAtUtc == default)
            entity.CreatedAtUtc = now;
        entity.UpdatedAtUtc = now;

        await Collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task ReplaceAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAtUtc = Clock.UtcNow;
        await Collection.ReplaceOneAsync(ById(entity.Id), entity, cancellationToken: ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
        => await Collection.DeleteOneAsync(ById(id), ct);
}
