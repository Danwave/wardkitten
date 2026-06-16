namespace Wardkitten.Domain.Common;

/// <summary>
/// Raíz común de las entidades persistidas. El <see cref="Id"/> es un ObjectId de Mongo
/// representado como string; se genera en el insert (ver MongoDbConfigurator) cuando está vacío.
/// Las marcas de tiempo se almacenan en UTC.
/// </summary>
public abstract class Entity
{
    public string Id { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
