var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Liveness/readiness probe usado por Kubernetes (ver K8S/**/wardkitten.yaml).
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "wardkitten-api" }));

app.MapGet("/", () => Results.Ok(new { name = "Wardkitten API", docs = "/openapi/v1.json" }));

app.Run();

public partial class Program;
