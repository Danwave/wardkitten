using System.Text.RegularExpressions;
using Wardkitten.Application.Abstractions.Persistence;
using Wardkitten.Application.Common;
using Wardkitten.Application.Security;
using Wardkitten.Domain.Billing;
using Wardkitten.Domain.StatusPages;
using Wardkitten.Domain.Watches;

namespace Wardkitten.Application.Services;

public sealed record StatusPageView(string Title, IReadOnlyList<Watch> Watches);

/// <summary>Gestión de status pages (disponibles en planes de pago) y su vista pública. Feature: F08.03.</summary>
public sealed partial class StatusPageService
{
    private readonly IStatusPageRepository _pages;
    private readonly IWatchRepository _watches;
    private readonly IUserRepository _users;

    public StatusPageService(IStatusPageRepository pages, IWatchRepository watches, IUserRepository users)
    {
        _pages = pages;
        _watches = watches;
        _users = users;
    }

    public Task<IReadOnlyList<StatusPage>> ListByUserAsync(string userId, CancellationToken ct = default)
        => _pages.GetByUserAsync(userId, ct);

    public async Task<Result<StatusPage>> CreateAsync(string userId, string title, bool isPublic, List<string> watchIds, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null) return Result<StatusPage>.Fail("Usuario no encontrado.");
        if (!PlanCatalog.For(user.Plan).StatusPages)
            return Result<StatusPage>.Fail("Las status pages requieren un plan de pago.");
        if (string.IsNullOrWhiteSpace(title))
            return Result<StatusPage>.Fail("El título es obligatorio.");

        var page = new StatusPage
        {
            UserId = userId,
            Title = title.Trim(),
            IsPublic = isPublic,
            WatchIds = watchIds ?? new List<string>(),
            Slug = $"{Slugify(title)}-{SecureTokenGenerator.New(3)}",
        };
        await _pages.InsertAsync(page, ct);
        return Result<StatusPage>.Ok(page);
    }

    public async Task<Result> DeleteAsync(string id, string userId, CancellationToken ct = default)
    {
        var page = await _pages.GetByIdAsync(id, ct);
        if (page is null || page.UserId != userId) return Result.Fail("No encontrada.");
        await _pages.DeleteAsync(id, ct);
        return Result.Ok();
    }

    /// <summary>Vista pública por slug. Devuelve null si no existe o no es pública.</summary>
    public async Task<StatusPageView?> GetPublicViewAsync(string slug, CancellationToken ct = default)
    {
        var page = await _pages.GetBySlugAsync(slug, ct);
        if (page is null || !page.IsPublic) return null;

        var watches = new List<Watch>();
        foreach (var watchId in page.WatchIds)
        {
            var watch = await _watches.GetByIdAsync(watchId, ct);
            if (watch is not null && watch.UserId == page.UserId)
                watches.Add(watch);
        }
        return new StatusPageView(page.Title, watches);
    }

    private static string Slugify(string text)
    {
        var slug = NonAlphanumeric().Replace(text.Trim().ToLowerInvariant(), "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "status" : slug[..Math.Min(slug.Length, 40)];
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
