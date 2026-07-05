using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<int> RevokeIfActiveAsync(string token, string? replacedByToken, CancellationToken cancellationToken = default);
    Task<int> RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}