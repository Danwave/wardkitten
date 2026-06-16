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
