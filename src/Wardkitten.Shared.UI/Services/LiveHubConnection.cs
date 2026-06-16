using Microsoft.AspNetCore.SignalR.Client;
using Wardkitten.Shared.UI.Auth;

namespace Wardkitten.Shared.UI.Services;

/// <summary>
/// Conexión SignalR al hub de Wardkitten para recibir cambios en vivo (watchUpdated, incidentOpened,
/// incidentResolved). Degrada con elegancia: si no conecta, la UI sigue funcionando por polling. Feature: F08.02.
/// </summary>
public sealed class LiveHubConnection : IAsyncDisposable
{
    private readonly WardkittenApiClient _api;
    private readonly ITokenStore _store;
    private HubConnection? _connection;

    /// <summary>Se dispara ante cualquier cambio relevante para refrescar la vista.</summary>
    public event Action? OnChanged;

    public LiveHubConnection(WardkittenApiClient api, ITokenStore store)
    {
        _api = api;
        _store = store;
    }

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task StartAsync()
    {
        if (_connection is not null) return;
        var baseUrl = _api.BaseAddress?.ToString().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) return;

        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/watch", options =>
                options.AccessTokenProvider = async () => await _store.GetAccessTokenAsync())
            .WithAutomaticReconnect()
            .Build();

        _connection.On<object>("watchUpdated", _ => OnChanged?.Invoke());
        _connection.On<object>("incidentOpened", _ => OnChanged?.Invoke());
        _connection.On<object>("incidentResolved", _ => OnChanged?.Invoke());

        try { await _connection.StartAsync(); }
        catch { /* la UI sigue por polling */ }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
