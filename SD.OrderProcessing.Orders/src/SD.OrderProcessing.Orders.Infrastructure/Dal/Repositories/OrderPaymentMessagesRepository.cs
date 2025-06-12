using Dapper;
using Npgsql;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Orders.Domain.Exceptions.Infrastructure.Dal;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Repositories;

public class OrderPaymentMessagesRepository : BaseRepository, IOrderPaymentMessagesRepository
{
    public OrderPaymentMessagesRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }

    public async Task Create(OrderPaymentMessageEntity[] entities, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
INSERT INTO order_payment_messages (order_id, user_id, created_at, amount)
    SELECT order_id, user_id, created_at, amount
    FROM UNNEST(@Entities::order_payment_message_type[]);
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
                    commandText: sqlQuery,
                    parameters: sqlParameters,
                    cancellationToken: cancellationToken
                )
            );
        }
        catch (NpgsqlException ex)
        {
            if (ex.SqlState == "23503")
            {
                throw new EntityNotFoundException("Order id foreign key not found.");
            }

            throw;
        }
    }

    public async Task<IReadOnlyList<OrderPaymentMessageEntity>> GetPriorPendingMessagesToPublishAndUpdate(int limit,
        CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
SELECT * FROM order_payment_messages
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

        var entities = await connection.QueryAsync<OrderPaymentMessageEntity>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        return entities.ToArray();
    }

    public async Task MarkMessagesAsDone(long[] messagesIds, CancellationToken cancellationToken)
    {
        const string sqlCommand = @"
UPDATE order_payment_messages
    SET state = 'done'
    WHERE id = ANY(@Ids::bigint[]);
";

        var sqlParameter = new
        {
            Ids = messagesIds
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            commandText: sqlCommand,
            parameters: sqlParameter,
            cancellationToken: cancellationToken
        ));
    }
}