using Microsoft.AspNetCore.SignalR;
using Wardkitten.Api.Mapping;
using Wardkitten.Application.RealTime;
using Wardkitten.Domain.Incidents;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Api.RealTime;

/// <summary>Publica los cambios por SignalR al grupo del usuario. Sustituye al publicador no-op en la API.</summary>
public sealed class SignalRWatchEventPublisher : IWatchEventPublisher
{
    private readonly IHubContext<WatchHub> _hub;

    public SignalRWatchEventPublisher(IHubContext<WatchHub> hub) => _hub = hub;

    public Task WatchUpdatedAsync(Watch watch, CancellationToken ct = default)
        => _hub.Clients.Group(watch.UserId).SendAsync("watchUpdated", watch.ToDto(), ct);

    public Task IncidentOpenedAsync(Incident incident, CancellationToken ct = default)
        => _hub.Clients.Group(incident.UserId).SendAsync("incidentOpened", incident.ToDto(), ct);

    public Task IncidentResolvedAsync(Incident incident, CancellationToken ct = default)
        => _hub.Clients.Group(incident.UserId).SendAsync("incidentResolved", incident.ToDto(), ct);
}
