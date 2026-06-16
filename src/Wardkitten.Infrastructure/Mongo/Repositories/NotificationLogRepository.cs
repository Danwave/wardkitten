using MongoDB.Driver;
using Wardkitten.Application.Abstractions;
using Wardkitten.Application.Abstractions.Persistence;
using Wardkitten.Domain.Notifications;

namespace Wardkitten.Infrastructure.Mongo.Repositories;

public sealed class NotificationLogRepository : INotificationLogRepository
{
    private readonly IMongoCollection<NotificationLog> _collection;
    private readonly IClock _clock;

    public NotificationLogRepository(MongoContext ctx, IClock clock)
    {
        _collection = ctx.NotificationLogs;
        _clock = clock;
    }

    public async Task InsertAsync(NotificationLog log, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(log.Id))
            log.Id = Guid.NewGuid().ToString("N");
        var now = _clock.UtcNow;
        if (log.SentAtUtc == default) log.SentAtUtc = now;
        log.CreatedAtUtc = now;
        log.UpdatedAtUtc = now;
        await _collection.InsertOneAsync(log, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<NotificationLog>> GetByUserAsync(string userId, int skip, int take, CancellationToken ct = default)
        => await _collection.Find(Builders<NotificationLog>.Filter.Eq(n => n.UserId, userId))
                            .SortByDescending(n => n.SentAtUtc)
                            .Skip(skip).Limit(take)
                            .ToListAsync(ct);
}
