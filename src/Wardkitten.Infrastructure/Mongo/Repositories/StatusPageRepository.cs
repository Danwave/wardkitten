using MongoDB.Driver;
using Wardkitten.Application.Abstractions;
using Wardkitten.Application.Abstractions.Persistence;
using Wardkitten.Domain.StatusPages;

namespace Wardkitten.Infrastructure.Mongo.Repositories;

public sealed class StatusPageRepository : MongoRepository<StatusPage>, IStatusPageRepository
{
    public StatusPageRepository(MongoContext ctx, IClock clock) : base(ctx.StatusPages, clock) { }

    public async Task<IReadOnlyList<StatusPage>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await Collection.Find(Builders<StatusPage>.Filter.Eq(s => s.UserId, userId))
                           .SortByDescending(s => s.CreatedAtUtc).ToListAsync(ct);

    public async Task<StatusPage?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await Collection.Find(Builders<StatusPage>.Filter.Eq(s => s.Slug, slug)).FirstOrDefaultAsync(ct);
}
