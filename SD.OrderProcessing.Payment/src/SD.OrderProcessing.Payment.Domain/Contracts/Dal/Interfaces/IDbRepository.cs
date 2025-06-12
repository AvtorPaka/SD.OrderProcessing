using System.Transactions;

namespace SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;

public interface IDbRepository
{
    public TransactionScope CreateTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}