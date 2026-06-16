namespace Wardkitten.Infrastructure.Mongo;

/// <summary>
/// Configuración de conexión a MongoDB. Se inyecta por variables de entorno (convención de la casa):
/// <c>MONGOSETTINGS_CONNECTION</c> y <c>MONGOSETTINGS_DATABASENAME</c>. Nunca en el repo (ver SECURITY.md).
/// </summary>
public sealed class MongoSettings
{
    public string Connection { get; set; } = "mongodb://localhost:27017/?replicaSet=rs0";
    public string DatabaseName { get; set; } = "Wardkitten";
}
