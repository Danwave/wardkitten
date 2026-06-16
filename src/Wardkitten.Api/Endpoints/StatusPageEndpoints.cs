using System.Security.Claims;
using Wardkitten.Api.Security;
using Wardkitten.Application.Services;
using Wardkitten.Shared.Contracts;

namespace Wardkitten.Api.Endpoints;

public static class StatusPageEndpoints
{
    public static void MapStatusPageEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/status-pages").WithTags("StatusPages").RequireAuthorization();

        g.MapGet("/", async (ClaimsPrincipal p, StatusPageService svc, CancellationToken ct) =>
        {
            var pages = await svc.ListByUserAsync(p.UserId()!, ct);
            return Results.Ok(pages.Select(s => new StatusPageDto(s.Id, s.Title, s.Slug, s.IsPublic, s.WatchIds)));
        });

        g.MapPost("/", async (StatusPageRequest req, ClaimsPrincipal p, StatusPageService svc, CancellationToken ct) =>
        {
            var r = await svc.CreateAsync(p.UserId()!, req.Title, req.IsPublic, req.WatchIds, ct);
            return r.Success
                ? Results.Ok(new StatusPageDto(r.Value!.Id, r.Value.Title, r.Value.Slug, r.Value.IsPublic, r.Value.WatchIds))
                : Results.BadRequest(new { error = r.Error });
        });

        g.MapDelete("/{id}", async (string id, ClaimsPrincipal p, StatusPageService svc, CancellationToken ct) =>
        {
            var r = await svc.DeleteAsync(id, p.UserId()!, ct);
            return r.Success ? Results.NoContent() : Results.NotFound();
        });

        // Vista pública (sin sesión).
        app.MapGet("/s/{slug}", async (string slug, StatusPageService svc, CancellationToken ct) =>
        {
            var view = await svc.GetPublicViewAsync(slug, ct);
            if (view is null) return Results.NotFound();
            var items = view.Watches
                .Select(w => new StatusItemDto(w.Name, w.Status.ToString(), w.NextDueAtUtc, w.LastCheckInAtUtc))
                .ToList();
            return Results.Ok(new PublicStatusPageDto(view.Title, items));
        }).WithTags("StatusPages").RequireRateLimiting("ping");
    }
}
