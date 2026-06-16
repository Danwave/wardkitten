namespace Wardkitten.Domain.Billing;

/// <summary>Plan de suscripción. Los canales de pago (SMS/WhatsApp) NO dependen del plan: se pagan con
/// la wallet de créditos en cualquier plan, incluido Free. El plan limita cupos y funciones.</summary>
public enum Plan
{
    Free = 0,
    Pro = 1,
    Team = 2,
}

/// <summary>Límites y capacidades por plan (se comprueban siempre en servidor).</summary>
public sealed class PlanLimits
{
    public Plan Plan { get; init; }
    public int MaxWatches { get; init; }
    public int MinIntervalSeconds { get; init; }
    public int HistoryRetentionDays { get; init; }
    public bool EscalationPolicies { get; init; }
    public bool StatusPages { get; init; }
    public bool TeamFeatures { get; init; }
}

public static class PlanCatalog
{
    private static readonly Dictionary<Plan, PlanLimits> Limits = new()
    {
        [Plan.Free] = new PlanLimits
        {
            Plan = Plan.Free,
            MaxWatches = 5,
            MinIntervalSeconds = 3600,      // mínimo 1 hora entre check-ins
            HistoryRetentionDays = 7,
            EscalationPolicies = false,
            StatusPages = false,
            TeamFeatures = false,
        },
        [Plan.Pro] = new PlanLimits
        {
            Plan = Plan.Pro,
            MaxWatches = 100,
            MinIntervalSeconds = 60,
            HistoryRetentionDays = 365,
            EscalationPolicies = true,
            StatusPages = true,
            TeamFeatures = false,
        },
        [Plan.Team] = new PlanLimits
        {
            Plan = Plan.Team,
            MaxWatches = 1000,
            MinIntervalSeconds = 30,
            HistoryRetentionDays = 730,
            EscalationPolicies = true,
            StatusPages = true,
            TeamFeatures = true,
        },
    };

    public static PlanLimits For(Plan plan) => Limits[plan];
}
