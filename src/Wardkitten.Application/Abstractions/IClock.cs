namespace Wardkitten.Application.Abstractions;

/// <summary>Reloj inyectable: permite controlar el tiempo en tests (scheduling, tolerancias, leases).</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
