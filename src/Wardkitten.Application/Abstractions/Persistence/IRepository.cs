using Wardkitten.Domain.Common;

namespace Wardkitten.Application.Abstractions.Persistence;

/// <summary>CRUD genérico para entidades con Id. Las implementaciones asignan Id y marcas de tiempo.</summary>
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task InsertAsync(T entity, CancellationToken ct = default);
    Task ReplaceAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
