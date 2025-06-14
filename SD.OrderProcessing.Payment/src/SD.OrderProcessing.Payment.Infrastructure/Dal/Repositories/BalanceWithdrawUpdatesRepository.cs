using Dapper;
using Npgsql;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Repositories;

public class BalanceWithdrawUpdatesRepository: BaseRepository, IBalanceWithdrawUpdatesRepository
{
    public BalanceWithdrawUpdatesRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }

    public async Task Create(BalanceWithdrawUpdateEntity[] entities, CancellationToken cancellationToken)
    {
        const string sqlComand = @"
INSERT INTO balance_withdraw_updates (order_id, user_id, amount, created_at)
    SELECT order_id, user_id, amount, created_at
    FROM UNNEST(@Entities::balance_withdraw_update_type[]);
";
        
        var sqlParameters = new
        {
            Entities = entities
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    commandText: sqlComand,
                    parameters: sqlParameters,
                    cancellationToken: cancellationToken
                )
            );
        }
        catch (NpgsqlException ex)
        {
            if (ex.SqlState == "23505")
            {
                throw new EntityAlreadyExistsException("Entity already exists.");
            }

            throw;
        }
    }

    public async Task<IReadOnlyList<BalanceWithdrawUpdateEntity>> GetPriorPendingOperationsToUpdate(int limit, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
SELECT * FROM balance_withdraw_updates
    WHERE state = 'pending'
    ORDER BY created_at
    FOR UPDATE SKIP LOCKED
    LIMIT @Limit;
";
        
        var sqlParameters = new
        {
            Limit = limit
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        var entities = await connection.QueryAsync<BalanceWithdrawUpdateEntity>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        return entities.ToArray();
    }

    public async Task MarkOperationsDone(long[] operationIds, CancellationToken cancellationToken)
    {
        const string sqlCommand = @"
UPDATE balance_withdraw_updates
    SET state = 'done'
    WHERE id = ANY(@Ids::bigint[]);
";
        
        var sqlParameter = new
        {
            Ids = operationIds
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            commandText: sqlCommand,
            parameters: sqlParameter,
            cancellationToken: cancellationToken
        ));
    }
}