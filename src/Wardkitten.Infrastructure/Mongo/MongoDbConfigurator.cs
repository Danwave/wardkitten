using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Wardkitten.Infrastructure.Mongo;

/// <summary>
/// Registra las convenciones BSON globales de Wardkitten. <b>Debe llamarse una sola vez y ANTES</b> de
/// construir cualquier <c>IMongoClient</c>/contexto o de serializar entidades (ver AGENTS.md).
/// </summary>
public static class MongoDbConfigurator
{
    private static bool _configured;
    private static readonly object Gate = new();

    public static void Configure()
    {
        if (_configured) return;
        lock (Gate)
        {
            if (_configured) return;

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),     // PascalCase C# -> camelCase en Mongo
                new IgnoreExtraElementsConvention(true),   // tolera campos extra al deserializar
                new IgnoreIfNullConvention(true),          // no persiste miembros null (sparse-friendly)
                new EnumRepresentationConvention(BsonType.String), // enums legibles como string
            };
            ConventionRegistry.Register("wardkitten", pack, _ => true);

            // Decimales como Decimal128 (numérico real) para permitir $inc atómico en la wallet.
            BsonSerializer.TryRegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.TryRegisterSerializer(
                typeof(decimal?),
                new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

            _configured = true;
        }
    }
}
