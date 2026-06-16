namespace Wardkitten.Domain.Watches;

/// <summary>
/// Las dos dimensiones de tolerancia de un watch antes de alertar:
/// <list type="bullet">
/// <item><b>Gracia</b>: margen de retraso permitido tras el deadline.</item>
/// <item><b>Skip</b>: número de incumplimientos consecutivos permitidos (p.ej. regar las plantas un
/// día tarde no es drama).</item>
/// </list>
/// </summary>
public sealed class Tolerance
{
    /// <summary>Segundos de gracia tras el vencimiento antes de considerar el plazo incumplido.</summary>
    public int GraceSeconds { get; set; }

    /// <summary>Incumplimientos consecutivos permitidos antes de abrir incidente y alertar.</summary>
    public int SkipTolerance { get; set; }

    public TimeSpan Grace => TimeSpan.FromSeconds(Math.Max(0, GraceSeconds));

    /// <summary>
    /// ¿Se ha agotado la tolerancia a fallos? Con <see cref="SkipTolerance"/> = 0 se alerta al primer
    /// incumplimiento; con N, se permiten N saltos y se alerta en el (N+1)-ésimo.
    /// </summary>
    public bool IsBreached(int consecutiveMisses) => consecutiveMisses > SkipTolerance;

    public static Tolerance None => new() { GraceSeconds = 0, SkipTolerance = 0 };
}
