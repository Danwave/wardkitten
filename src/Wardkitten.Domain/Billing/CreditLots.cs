// Feature: F06.03 / F07 (recargas de wallet por lotes)
namespace Wardkitten.Domain.Billing;

/// <summary>
/// Lotes de créditos que se pueden recargar. El importe cobrado es <c>lote × precio unidad</c>
/// (la cantidad se envía como quantity del line item de Stripe). Ver SECURITY.md §4.
/// </summary>
public static class CreditLots
{
    /// <summary>Tamaños de lote ofrecidos al usuario.</summary>
    public static readonly int[] Sizes = { 10, 50, 100, 500, 1000 };

    /// <summary>True si <paramref name="credits"/> es exactamente uno de los lotes permitidos.</summary>
    public static bool IsValid(decimal credits)
    {
        if (credits <= 0 || credits % 1 != 0) return false;
        foreach (var size in Sizes)
            if (size == credits) return true;
        return false;
    }
}
