using Wardkitten.Application.DependencyInjection;
using Wardkitten.Infrastructure.DependencyInjection;
using Wardkitten.Worker;

var builder = Host.CreateApplicationBuilder(args);
var config = builder.Configuration;

builder.Services.AddWardkittenApplication(config["PUBLIC_BASE_URL"]);
builder.Services.AddWardkittenInfrastructure(config);
builder.Services.AddWardkittenIntegrations(config);
builder.Services.AddHostedService<EvaluationWorker>();

var host = builder.Build();

// Crea índices y la colección time-series (idempotente). El worker es el segundo punto de creación.
try
{
    await host.Services.InitializeWardkittenInfrastructureAsync();
}
catch (Exception ex)
{
    host.Services.GetRequiredService<ILogger<Program>>()
        .LogWarning(ex, "No se pudo inicializar MongoDB en el arranque del worker; se reintentará.");
}

host.Run();

public partial class Program;
