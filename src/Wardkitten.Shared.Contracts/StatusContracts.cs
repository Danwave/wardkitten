namespace Wardkitten.Shared.Contracts;

public sealed record StatusPageDto(string Id, string Title, string Slug, bool IsPublic, List<string> WatchIds);

public sealed record StatusPageRequest(string Title, bool IsPublic, List<string> WatchIds);

public sealed record StatusItemDto(string Name, string Status, DateTime? NextDueAtUtc, DateTime? LastCheckInAtUtc);

public sealed record PublicStatusPageDto(string Title, List<StatusItemDto> Items);
