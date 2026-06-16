using Wardkitten.Domain.Common;

namespace Wardkitten.Domain.StatusPages;

/// <summary>
/// Página de estado (pública o privada) que muestra el estado de un conjunto de watches. Disponible en
/// planes de pago. Feature: F08.03.
/// </summary>
public sealed class StatusPage : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    /// <summary>Identificador público en la URL (único, inadivinable si es privada).</summary>
    public string Slug { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;

    /// <summary>Watches incluidos en la página.</summary>
    public List<string> WatchIds { get; set; } = new();
}
