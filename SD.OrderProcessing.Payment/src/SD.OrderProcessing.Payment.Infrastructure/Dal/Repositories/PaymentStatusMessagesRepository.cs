using Dapper;
using Npgsql;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;

namespace SD.OrderProcessing.Payment.Infrastructure.Dal.Repositories;

public class PaymentStatusMessagesRepository : BaseRepository, IPaymentStatusMessagesRepository
{
    public PaymentStatusMessagesRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }

    public async Task Create(PaymentStatusMessageEntity[] entities, CancellationToken cancellationToken)
    {
        const string sqlCommand = @"
INSERT INTO payment_status_messages (order_id, created_at, order_status)
    SELECT order_id, created_at, order_status
    FROM UNNEST(@Entities::payment_status_message_type[]);
";

        var sqlParameters = new
        {
            Entities = entities
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                commandText: sqlCommand,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );
    }

    public async Task<IReadOnlyList<PaymentStatusMessageEntity>> GetPriorPendingMessagesToUpdateAndPublish(int limit,
        CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
SELECT * FROM payment_status_messages
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

        var entities = await connection.QueryAsync<PaymentStatusMessageEntity>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        return entities.ToArray();
    }

    public async Task MarkMessagesDone(long[] messagesIds, CancellationToken cancellationToken)
    {
        const string sqlCommand = @"
UPDATE payment_status_messages
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