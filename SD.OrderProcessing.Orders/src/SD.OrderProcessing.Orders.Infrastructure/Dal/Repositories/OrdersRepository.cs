using Dapper;
using Npgsql;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Orders.Domain.Exceptions.Infrastructure.Dal;
using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Infrastructure.Dal.Repositories;

public class OrdersRepository : BaseRepository, IOrdersRepository
{
    public OrdersRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }

    public async Task<long[]> Create(OrderEntity[] entities, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
INSERT INTO user_orders (user_id, amount, description)
    SELECT user_id, amount, description
    FROM UNNEST(@Entities::user_order_type[])
    RETURNING id;
";

        var sqlParameters = new
        {
            Entities = entities
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        var createdIds = await connection.QueryAsync<long>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        return createdIds.ToArray();
    }

    public async Task<OrderEntity> GetById(long orderId, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
SELECT * FROM user_orders
    WHERE id = @OrderId;
";

        var sqlParameters = new
        {
            OrderId = orderId
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        var entity = await connection.QueryFirstOrDefaultAsync<OrderEntity?>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        if (entity == null)
        {
            throw new EntityNotFoundException("Entity couldn't be found");
        }

        return entity;
    }

    public async Task<IReadOnlyList<OrderEntity>> GetAllForUser(long userId, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
SELECT * FROM user_orders
    WHERE user_id = @UserId;
";

        var sqlParameters = new
        {
            UserId = userId
        };

        await using NpgsqlConnection connection = await GetAndOpenConnectionAsync(cancellationToken);

        var entities = await connection.QueryAsync<OrderEntity>(
            new CommandDefinition(
                commandText: sqlQuery,
                parameters: sqlParameters,
                cancellationToken: cancellationToken
            )
        );

        return entities.ToList();
    }

    public async Task UpdateStatus(long orderId, OrderStatus newStatus, CancellationToken cancellationToken)
    {
        const string sqlCommand = @"
UPDATE user_orders
    SET status = @NewStatus::order_status_enum
    WHERE id = @OrderId;
";

        var sqlParameters = new
        {
            NewStatus = newStatus.ToString().ToLower(),
            OrderId = orderId
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
}