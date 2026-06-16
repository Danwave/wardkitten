namespace Wardkitten.Domain.Leasing;

/// <summary>
/// Lock distribuido para <b>leader election</b>. Garantiza que solo una réplica del worker ejecuta el
/// motor de evaluación, evitando alertas duplicadas al escalar en Kubernetes. El <see cref="Id"/> es el
/// nombre del recurso (no un GUID), p.ej. "evaluation-engine". Feature: F04.03.
/// </summary>
public sealed class Lease
{
    public string Id { get; set; } = string.Empty;
    public string Holder { get; set; } = string.Empty;
    public DateTime AcquiredAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;
}
