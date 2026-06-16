using Microsoft.AspNetCore.SignalR;
using Wardkitten.Api.Mapping;
using Wardkitten.Api.RealTime;
using Wardkitten.Application.Abstractions.Persistence;
using Wardkitten.Shared.Contracts;

namespace Wardkitten.Api.Endpoints;

/// <summary>
/// Endpoint interno (worker -> API) que reemite por SignalR los eventos generados por el worker.
/// Protegido por token compartido (X-Internal-Token); no es público. Feature: F08.02.
/// </summary>
public static class InternalEndpoints
{
    public static void MapInternalEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/events", async (
            InternalEventRequest evt, HttpRequest request, IHubContext<WatchHub> hub,
            IWatchRepository watches, IIncidentRepository incidents, IConfiguration config, CancellationToken ct) =>
        {
            var expected = config["INTERNAL_TOKEN"];
            if (string.IsNullOrEmpty(expected) || request.Headers["X-Internal-Token"] != expected)
                return Results.Unauthorized();

            switch (evt.Type)
            {
                case "watchUpdated" when evt.WatchId is not null:
                    var watch = await watches.GetByIdAsync(evt.WatchId, ct);
                    if (watch is not null)
                        await hub.Clients.Group(evt.UserId).SendAsync("watchUpdated", watch.ToDto(), ct);
                    break;

                case "incidentOpened" when evt.IncidentId is not null:
                    var opened = await incidents.GetByIdAsync(evt.IncidentId, ct);
                    if (opened is not null)
                        await hub.Clients.Group(evt.UserId).SendAsync("incidentOpened", opened.ToDto(), ct);
                    break;

                case "incidentResolved" when evt.IncidentId is not null:
                    var resolved = await incidents.GetByIdAsync(evt.IncidentId, ct);
                    if (resolved is not null)
                        await hub.Clients.Group(evt.UserId).SendAsync("incidentResolved", resolved.ToDto(), ct);
                    break;
            }
            return Results.Ok();
        }).WithTags("Internal").ExcludeFromDescription();
    }
}
