using System.Transactions;
using Npgsql;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Repositories;

public abstract class BaseRepository: IDbRepository
{
    private readonly NpgsqlDataSource _dataSource;

    protected BaseRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }
    
    protected async Task<NpgsqlConnection> GetAndOpenConnectionAsync(CancellationToken cancellationToken)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }
    
    public TransactionScope CreateTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return new TransactionScope(
            scopeOption: TransactionScopeOption.Required,
            transactionOptions: new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = TimeSpan.FromSeconds(2)
            },
            asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled
        );
    }
}