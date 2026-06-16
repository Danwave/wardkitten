using Wardkitten.Domain.StatusPages;

namespace Wardkitten.Application.Abstractions.Persistence;

public interface IStatusPageRepository : IRepository<StatusPage>
{
    Task<IReadOnlyList<StatusPage>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<StatusPage?> GetBySlugAsync(string slug, CancellationToken ct = default);
}
