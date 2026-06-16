using System.Net.Http.Json;
using Wardkitten.Application.RealTime;
using Wardkitten.Domain.Incidents;
using Wardkitten.Domain.Watches;
using Wardkitten.Shared.Contracts;

namespace Wardkitten.Worker;

/// <summary>
/// Publica los eventos del worker llamando al endpoint interno de la API, que los reemite por SignalR a la
/// web (sin necesidad de backplane Redis). Es best-effort: si falla, no afecta a la evaluación. Feature: F08.02.
/// </summary>
public sealed class HttpWatchEventPublisher : IWatchEventPublisher
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string? _baseUrl;
    private readonly string? _token;
    private readonly ILogger<HttpWatchEventPublisher> _logger;

    public HttpWatchEventPublisher(IHttpClientFactory httpFactory, IConfiguration config, ILogger<HttpWatchEventPublisher> logger)
    {
        _httpFactory = httpFactory;
        _baseUrl = config["API_INTERNAL_URL"];
        _token = config["INTERNAL_TOKEN"];
        _logger = logger;
    }

    public Task WatchUpdatedAsync(Watch watch, CancellationToken ct = default)
        => PostAsync(new InternalEventRequest("watchUpdated", watch.UserId, watch.Id, null), ct);

    public Task IncidentOpenedAsync(Incident incident, CancellationToken ct = default)
        => PostAsync(new InternalEventRequest("incidentOpened", incident.UserId, incident.WatchId, incident.Id), ct);

    public Task IncidentResolvedAsync(Incident incident, CancellationToken ct = default)
        => PostAsync(new InternalEventRequest("incidentResolved", incident.UserId, incident.WatchId, incident.Id), ct);

    private async Task PostAsync(InternalEventRequest evt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl) || string.IsNullOrWhiteSpace(_token))
            return; // tiempo real desactivado (no configurado)

        try
        {
            var client = _httpFactory.CreateClient("internal-events");
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl.TrimEnd('/')}/internal/events")
            {
                Content = JsonContent.Create(evt),
            };
            request.Headers.Add("X-Internal-Token", _token);
            using var _ = await client.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo publicar el evento en tiempo real (best-effort)");
        }
    }
}
