using Core.Domain.Entities;

namespace Core.Domain.Interfaces;

public interface IRepositoryManager : IDisposable
{
    IRepository<RefreshToken> RefreshToken { get; }

    Task SaveAsync(CancellationToken cancellationToken = default);
}