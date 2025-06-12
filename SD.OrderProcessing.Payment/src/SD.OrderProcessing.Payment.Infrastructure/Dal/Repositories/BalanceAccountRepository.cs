using Dapper;
using Npgsql;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Repositories;

public class BalanceAccountRepository : BaseRepository, IBalanceAccountRepository
{
    public BalanceAccountRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }

    public async Task<long[]> CreateNew(BalanceAccountEntity[] entities, CancellationToken cancellation)
    {
        const string sqlQuery = @"
INSERT INTO balance_accounts (user_id, balance, version)
    SELECT user_id, balance, version 
    FROM UNNEST(@Entities::balance_account_type[])
    RETURNING id;
";
        var sqlParameters = new
        {
            Entities = entities
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellation);

        IEnumerable<long> createdIds;

        try
        {
            createdIds = await connection.QueryAsync<long>(
                new CommandDefinition(
                    commandText: sqlQuery,
                    parameters: sqlParameters,
                    cancellationToken: cancellation
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

        return createdIds.ToArray();
    }

    public async Task<BalanceAccountEntity> GetByUserId(long userId, bool isForUpdate, CancellationToken cancellationToken)
    {
        string sqlQuery = !isForUpdate ?  @"
SELECT * FROM balance_accounts
    WHERE user_id = @UserId;
" : @"
SELECT * FROM balance_accounts
    WHERE user_id = @UserId
    FOR UPDATE;
";

        var sqlParameters = new
        {
            UserId = userId
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        var entity = await connection.QueryFirstOrDefaultAsync<BalanceAccountEntity?>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        if (entity == null)
        {
            throw new EntityNotFoundException("Entity couldn't be found.");
        }

        return entity;
    }

    public async Task<long> CasUpdateBalance(long userId, long curVersion, decimal amount,
        CancellationToken cancellationToken)
    {
        const string sqlCommand = @"
UPDATE balance_accounts
    SET 
        balance = balance + @Amount,
        version = version + 1
    WHERE 
        user_id = @UserId AND
        balance + @Amount >= 0 AND
        version = @CurVersion;
";

        var sqlParameters = new
        {
            Amount = amount,
            UserId = userId,
            CurVersion = curVersion
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                commandText: sqlCommand,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        return affectedRows;
    }

    public async Task UpdateBalance(long userId, decimal amount, CancellationToken cancellation)
    {
        const string sqlCommand = @"
UPDATE balance_accounts
    SET 
        balance = balance + @Amount,
        version = version + 1
    WHERE 
        user_id = @UserId;
";

        var sqlParameters = new
        {
            Amount = amount,
            UserId = userId
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellation);

        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                commandText: sqlCommand,
                parameters: sqlParameters,
                cancellationToken: cancellation
            )
        );

        if (affectedRows == 0)
        {
            throw new EntityNotFoundException("Entity couldn't be found.");
        }
    }
}