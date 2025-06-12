using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;

namespace SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;

public interface IBalanceAccountRepository: IDbRepository
{
    public Task<long[]> CreateNew(BalanceAccountEntity[] entities, CancellationToken cancellation);
    public Task<BalanceAccountEntity> GetByUserId(long userId, bool isForUpdate, CancellationToken cancellationToken);
    public Task<long> CasUpdateBalance(long userId, long curVersion, decimal amount, CancellationToken cancellationToken);
    public Task UpdateBalance(long userId, decimal amount, CancellationToken cancellation);
}