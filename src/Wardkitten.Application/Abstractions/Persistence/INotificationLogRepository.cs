using Wardkitten.Domain.Notifications;

namespace Wardkitten.Application.Abstractions.Persistence;

public interface INotificationLogRepository
{
    Task InsertAsync(NotificationLog log, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationLog>> GetByUserAsync(string userId, int skip, int take, CancellationToken ct = default);
}
