namespace Wardkitten.Shared.Contracts;

public sealed record WalletDto(decimal BalanceCredits, decimal MinThresholdCredits, string Currency, bool IsBelowThreshold);

public sealed record CreditTransactionDto(
    string Id, string Type, decimal AmountCredits, decimal BalanceAfter,
    string? Reason, string? Channel, DateTime CreatedAtUtc);

public sealed record TopUpRequest(decimal Credits);

public sealed record SubscribeRequest(string Plan);

public sealed record CheckoutResponse(string Url);

public sealed record IncidentDto(
    string Id, string WatchId, string WatchName, string Severity, string State,
    DateTime OpenedAtUtc, DateTime? AcknowledgedAtUtc, DateTime? ResolvedAtUtc, int CurrentEscalationStep);

/// <summary>Evento interno worker -> API para reemitir por SignalR a la web (ver F08.02).</summary>
public sealed record InternalEventRequest(string Type, string UserId, string? WatchId, string? IncidentId);
