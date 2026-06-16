using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Wardkitten.Api.Security;

namespace Wardkitten.Api.RealTime;

/// <summary>
/// Hub SignalR para el dashboard en vivo. Cada conexión se une a un grupo con su userId; el servidor
/// emite "watchUpdated", "incidentOpened", "incidentResolved" a ese grupo. Feature: F08.02.
/// </summary>
[Authorize]
public sealed class WatchHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.UserId();
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }
}
