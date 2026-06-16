using Wardkitten.Domain.Common;

namespace Wardkitten.Domain.Billing;

public enum SubscriptionStatus
{
    Active = 0,
    Trialing = 1,
    PastDue = 2,
    Canceled = 3,
    Incomplete = 4,
}

/// <summary>Suscripción Stripe del usuario (espejo del estado en Stripe). Feature: F07.01.</summary>
public sealed class Subscription : Entity
{
    public string UserId { get; set; } = string.Empty;
    public Plan Plan { get; set; } = Plan.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripePriceId { get; set; }

    public DateTime? CurrentPeriodEndUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }

    public bool GrantsPaidFeatures => Plan != Plan.Free
        && Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing;
}
