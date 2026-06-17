using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Wardkitten.Api.Security;
using Wardkitten.Application.Services;
using Wardkitten.Domain.CheckIns;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Api.Mcp;

/// <summary>
/// Herramientas MCP (Model Context Protocol) de Wardkitten. <b>MCP es un protocolo/interfaz, no IA</b>:
/// permite que un cliente externo (p.ej. un agente) opere Wardkitten en el futuro. Cada herramienta
/// resuelve el usuario autenticado del JWT (el endpoint /mcp exige Bearer). Feature: F14.01.
/// </summary>
[McpServerToolType]
public sealed class WardkittenMcpTools
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static string? UserId(IHttpContextAccessor http) => http.HttpContext?.User.UserId();
    private static string NotAuthed() => "{\"error\":\"no autenticado\"}";
    private static string Error(string? msg) => JsonSerializer.Serialize(new { error = msg ?? "error" }, Json);

    [McpServerTool(Name = "list_watches"), Description("Lista las tareas vigiladas (watches) del usuario, con estado, criticidad y racha.")]
    public static async Task<string> ListWatches(WatchService watches, IHttpContextAccessor http, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var list = await watches.ListByUserAsync(userId, ct);
        return JsonSerializer.Serialize(list.Select(w => new
        {
            w.Id, w.Name, status = w.Status.ToString(), severity = w.Severity.ToString(),
            w.NextDueAtUtc, w.ConsecutiveMisses, w.CurrentStreak,
        }), Json);
    }

    [McpServerTool(Name = "get_watch"), Description("Devuelve el detalle de una tarea vigilada por su id.")]
    public static async Task<string> GetWatch(WatchService watches, IHttpContextAccessor http,
        [Description("Id del watch")] string id, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var w = await watches.GetAsync(id, userId, ct);
        return w is null ? Error("no encontrado") : JsonSerializer.Serialize(w, Json);
    }

    [McpServerTool(Name = "create_watch"), Description("Crea una tarea vigilada. Usa intervalSeconds para 'cada N segundos' o cronExpression (5 campos) para horario fijo.")]
    public static async Task<string> CreateWatch(WatchService watches, IHttpContextAccessor http,
        [Description("Nombre de la tarea")] string name,
        [Description("'Ping' (proceso automático) o 'Manual'")] string type,
        [Description("Segundos entre check-ins; 0 si usas cron")] int intervalSeconds,
        [Description("Cron de 5 campos; vacío si usas intervalo")] string? cronExpression,
        [Description("Segundos de gracia tras el vencimiento")] int graceSeconds,
        [Description("Fallos consecutivos tolerados antes de alertar")] int skipTolerance,
        [Description("Criticidad: Low | Medium | High | Critical")] string severity,
        CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();

        var watchType = Enum.TryParse<WatchType>(type, true, out var wt) ? wt : WatchType.Manual;
        var sev = Enum.TryParse<Severity>(severity, true, out var s) ? s : Severity.Medium;
        var schedule = intervalSeconds > 0
            ? new Schedule { Kind = ScheduleKind.Interval, IntervalSeconds = intervalSeconds }
            : new Schedule { Kind = ScheduleKind.Cron, CronExpression = cronExpression };

        var input = new WatchInput(name, null, watchType, schedule,
            new Tolerance { GraceSeconds = graceSeconds, SkipTolerance = skipTolerance },
            new List<ChannelBinding> { new() { ChannelType = ChannelType.Email, Enabled = true } },
            sev, null, null);

        var r = await watches.CreateAsync(userId, input, ct);
        return r.Success
            ? JsonSerializer.Serialize(new { id = r.Value!.Id, pingToken = r.Value.PingToken }, Json)
            : Error(r.Error);
    }

    [McpServerTool(Name = "check_in_watch"), Description("Registra un check-in (marca como hecha) la tarea indicada.")]
    public static async Task<string> CheckInWatch(CheckInService checkIns, IHttpContextAccessor http,
        [Description("Id del watch")] string id, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var r = await checkIns.RecordManualAsync(id, userId, CheckInSource.System, ct);
        return r.Success ? "{\"ok\":true}" : Error(r.Error);
    }

    [McpServerTool(Name = "pause_watch"), Description("Pausa la vigilancia de una tarea.")]
    public static async Task<string> PauseWatch(WatchService watches, IHttpContextAccessor http,
        [Description("Id del watch")] string id, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var r = await watches.PauseAsync(id, userId, ct);
        return r.Success ? "{\"ok\":true}" : Error(r.Error);
    }

    [McpServerTool(Name = "resume_watch"), Description("Reanuda la vigilancia de una tarea pausada.")]
    public static async Task<string> ResumeWatch(WatchService watches, IHttpContextAccessor http,
        [Description("Id del watch")] string id, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var r = await watches.ResumeAsync(id, userId, ct);
        return r.Success ? "{\"ok\":true}" : Error(r.Error);
    }

    [McpServerTool(Name = "list_incidents"), Description("Lista los incidentes recientes del usuario.")]
    public static async Task<string> ListIncidents(IncidentService incidents, IHttpContextAccessor http, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var list = await incidents.GetByUserAsync(userId, 0, 50, ct);
        return JsonSerializer.Serialize(list.Select(i => new
        {
            i.Id, i.WatchName, severity = i.Severity.ToString(), state = i.State.ToString(), i.OpenedAtUtc,
        }), Json);
    }

    [McpServerTool(Name = "acknowledge_incident"), Description("Reconoce (ACK) un incidente abierto.")]
    public static async Task<string> AcknowledgeIncident(IncidentService incidents, IHttpContextAccessor http,
        [Description("Id del incidente")] string id, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var r = await incidents.AcknowledgeAsync(id, userId, ct);
        return r.Success ? "{\"ok\":true}" : Error(r.Error);
    }

    [McpServerTool(Name = "get_wallet_balance"), Description("Devuelve el saldo de créditos de la wallet del usuario.")]
    public static async Task<string> GetWalletBalance(WalletService wallet, IHttpContextAccessor http, CancellationToken ct)
    {
        var userId = UserId(http);
        if (userId is null) return NotAuthed();
        var w = await wallet.GetWalletAsync(userId, ct);
        return JsonSerializer.Serialize(new { balance = w.BalanceCredits, w.Currency, belowThreshold = w.IsBelowThreshold }, Json);
    }
}
