using Wardkitten.Domain.Identity;

namespace Wardkitten.Application.Abstractions.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken ct = default);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
}
