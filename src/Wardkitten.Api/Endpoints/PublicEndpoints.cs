using Wardkitten.Application.Notifications;
using Wardkitten.Application.Services;
using Wardkitten.Domain.CheckIns;
using Wardkitten.Infrastructure.Billing;

namespace Wardkitten.Api.Endpoints;

/// <summary>
/// Endpoints públicos (sin sesión) con defensa propia: ping (token inadivinable), magic links (firmados)
/// y webhook de Stripe (firma verificada). Ver SECURITY.md §3.
/// </summary>
public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        // ---- Ping (healthcheck inverso de procesos automáticos) ----
        var ping = app.MapGroup("/p").WithTags("Ping").RequireRateLimiting("ping");

        ping.MapMethods("/{token}", new[] { "GET", "POST" }, (string token, CheckInService svc, HttpContext http, CancellationToken ct)
            => HandlePingAsync(token, CheckInKind.Success, svc, http, ct));
        ping.MapMethods("/{token}/start", new[] { "GET", "POST" }, (string token, CheckInService svc, HttpContext http, CancellationToken ct)
            => HandlePingAsync(token, CheckInKind.Start, svc, http, ct));
        ping.MapMethods("/{token}/fail", new[] { "GET", "POST" }, (string token, CheckInService svc, HttpContext http, CancellationToken ct)
            => HandlePingAsync(token, CheckInKind.Fail, svc, http, ct));

        // ---- Magic link (ACK / Hecho / Snooze) ----
        app.MapGet("/a/{token}", async (string token, IMagicLinkValidator validator, CheckInService checkIns,
            IncidentService incidents, CancellationToken ct) =>
        {
            var data = validator.Validate(token);
            if (data is null) return Results.Content(Html("Enlace caducado o inválido."), "text/html");

            switch (data.Action)
            {
                case "done":
                    await checkIns.RecordSuccessByWatchIdAsync(data.WatchId, CheckInSource.App, ct);
                    return Results.Content(Html("✅ ¡Hecho! Tarea marcada como completada."), "text/html");
                case "snooze":
                    await incidents.SnoozeAsync(data.IncidentId, TimeSpan.FromHours(1), ct);
                    return Results.Content(Html("😴 Pospuesto una hora."), "text/html");
                case "ack":
                    await incidents.AcknowledgeAsync(data.IncidentId, "magic-link", ct);
                    return Results.Content(Html("👍 Incidente reconocido."), "text/html");
                default:
                    return Results.Content(Html("Acción no reconocida."), "text/html");
            }
        }).WithTags("Magic").RequireRateLimiting("ping");

        // ---- Webhook de Stripe ----
        app.MapPost("/webhooks/stripe", async (HttpRequest request, StripeWebhookProcessor processor, CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync(ct);
            var signature = request.Headers["Stripe-Signature"].ToString();
            var ok = await processor.ProcessAsync(json, signature, ct);
            return ok ? Results.Ok() : Results.BadRequest();
        }).WithTags("Webhooks");
    }

    private static async Task<IResult> HandlePingAsync(string token, CheckInKind kind, CheckInService svc, HttpContext http, CancellationToken ct)
    {
        var ip = http.Connection.RemoteIpAddress?.ToString();
        var r = await svc.RecordByPingTokenAsync(token, kind, payload: null, ip, ct);
        return r.Success ? Results.Ok(new { status = "ok" }) : Results.NotFound();
    }

    private static string Html(string message) => $"""
        <!doctype html><html lang="es"><head><meta charset="utf-8">
        <meta name="viewport" content="width=device-width,initial-scale=1">
        <title>Wardkitten</title></head>
        <body style="font-family:system-ui;display:grid;place-items:center;height:100vh;margin:0;background:#0f172a;color:#e2e8f0">
        <div style="text-align:center"><div style="font-size:48px">🐾</div><h1 style="font-weight:600">{message}</h1>
        <p>Wardkitten</p></div></body></html>
        """;
}
