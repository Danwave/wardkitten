namespace Wardkitten.Application.Abstractions.Persistence;

/// <summary>
/// Lock distribuido para leader election del motor de evaluación. Implementado sobre Mongo con TTL.
/// </summary>
public interface ILeaseStore
{
    /// <summary>Intenta adquirir o renovar el lease del recurso. Devuelve true si este <paramref name="holder"/> lo posee.</summary>
    Task<bool> TryAcquireAsync(string resource, string holder, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>Renueva el lease si lo posee este holder.</summary>
    Task<bool> RenewAsync(string resource, string holder, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>Libera el lease si lo posee este holder.</summary>
    Task ReleaseAsync(string resource, string holder, CancellationToken ct = default);
}
