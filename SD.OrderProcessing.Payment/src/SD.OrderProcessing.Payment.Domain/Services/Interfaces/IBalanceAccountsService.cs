using SD.OrderProcessing.Payment.Domain.Models;

namespace SD.OrderProcessing.Payment.Domain.Services.Interfaces;

public interface IBalanceAccountsService
{
    public Task<BalanceAccountModel> CreateNew(long userId, CancellationToken cancellationToken);
    public Task DepositSum(long userId, decimal depositSum, CancellationToken cancellation);
    public Task<BalanceAccountModel> GetAccount(long userId, CancellationToken cancellation);
}