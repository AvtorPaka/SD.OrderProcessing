using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;

namespace SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;

public interface IBalanceWithdrawUpdatesRepository: IDbRepository
{
    public Task Create(BalanceWithdrawUpdateEntity[] entities, CancellationToken cancellationToken);
    public Task<IReadOnlyList<BalanceWithdrawUpdateEntity>> GetPriorPendingOperationsToUpdate(int limit, CancellationToken cancellationToken);
    public Task MarkOperationsDone(long[] operationIds, CancellationToken cancellationToken);
}