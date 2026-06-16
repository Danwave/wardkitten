namespace Wardkitten.Infrastructure.Mongo;

/// <summary>
/// Configuración de conexión a MongoDB. Se inyecta por variables de entorno (convención de la casa):
/// <c>MONGOSETTINGS_CONNECTION</c> y <c>MONGOSETTINGS_DATABASENAME</c>. Nunca en el repo (ver SECURITY.md).
/// </summary>
public sealed class MongoSettings
{
    // Standalone por defecto (dev). En producción se inyecta la URI real (puede ser replica set).
    public string Connection { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "Wardkitten";
}
