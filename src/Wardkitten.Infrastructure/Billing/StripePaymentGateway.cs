using Microsoft.Extensions.Options;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using Wardkitten.Application.Billing;
using Wardkitten.Domain.Billing;
using Wardkitten.Domain.Identity;
using SessionService = Stripe.Checkout.SessionService;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using Plan = Wardkitten.Domain.Billing.Plan;

namespace Wardkitten.Infrastructure.Billing;

/// <summary>Pasarela Stripe: sesiones de checkout para suscripciones y recargas, y portal de cliente.</summary>
public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeOptions _options;

    public StripePaymentGateway(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        if (!string.IsNullOrWhiteSpace(_options.SecretKey))
            StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public async Task<string> CreateSubscriptionCheckoutAsync(User user, Plan plan, string successUrl, string cancelUrl, CancellationToken ct = default)
    {
        var priceId = plan switch
        {
            Plan.Pro => _options.PriceProMonthly,
            Plan.Team => _options.PriceTeamMonthly,
            _ => throw new InvalidOperationException("Plan sin precio configurado."),
        };

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            ClientReferenceId = user.Id,
            Customer = user.StripeCustomerId,
            CustomerEmail = user.StripeCustomerId is null ? user.Email : null,
            LineItems = new List<SessionLineItemOptions> { new() { Price = priceId, Quantity = 1 } },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string> { ["userId"] = user.Id, ["plan"] = plan.ToString(), ["kind"] = "subscription" },
        };
        ApplyAutomaticTax(options, user);

        var session = await new SessionService().CreateAsync(options, cancellationToken: ct);
        return session.Url;
    }

    public async Task<string> CreateCreditTopUpCheckoutAsync(User user, decimal credits, string successUrl, string cancelUrl, CancellationToken ct = default)
    {
        var quantity = (long)Math.Ceiling(credits);
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = user.Id,
            Customer = user.StripeCustomerId,
            CustomerEmail = user.StripeCustomerId is null ? user.Email : null,
            LineItems = new List<SessionLineItemOptions> { BuildCreditLineItem(quantity) },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                ["userId"] = user.Id,
                ["credits"] = quantity.ToString(),
                ["kind"] = "credit-topup",
            },
        };
        ApplyAutomaticTax(options, user);

        var session = await new SessionService().CreateAsync(options, cancellationToken: ct);
        return session.Url;
    }

    /// <summary>
    /// Line item de créditos: usa el precio real de Stripe (<c>PriceCredit</c>) si está configurado —para que
    /// aplique su tax code e IVA incluido—; si no, cae a un precio dinámico. La cantidad = créditos del lote.
    /// </summary>
    private SessionLineItemOptions BuildCreditLineItem(long quantity)
        => string.IsNullOrWhiteSpace(_options.PriceCredit)
            ? new SessionLineItemOptions
            {
                Quantity = quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = _options.CreditCurrency,
                    UnitAmount = _options.CreditUnitAmountCents,
                    TaxBehavior = _options.CreditTaxBehavior,
                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = "Créditos Wardkitten" },
                },
            }
            : new SessionLineItemOptions { Price = _options.PriceCredit, Quantity = quantity };

    /// <summary>
    /// Activa Stripe Tax en el checkout si está habilitado. Con un customer existente, exige
    /// <c>customer_update.address=auto</c> para poder recalcular impuestos con la dirección recogida.
    /// </summary>
    private void ApplyAutomaticTax(SessionCreateOptions options, User user)
    {
        if (!_options.AutomaticTaxEnabled) return;
        options.AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true };
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
            options.CustomerUpdate = new SessionCustomerUpdateOptions { Address = "auto" };
    }

    public async Task<string> CreateBillingPortalAsync(User user, string returnUrl, CancellationToken ct = default)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = user.StripeCustomerId,
            ReturnUrl = returnUrl,
        };
        var session = await new Stripe.BillingPortal.SessionService().CreateAsync(options, cancellationToken: ct);
        return session.Url;
    }
}
