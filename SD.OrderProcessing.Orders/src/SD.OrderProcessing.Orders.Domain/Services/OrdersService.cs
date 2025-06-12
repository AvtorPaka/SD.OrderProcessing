using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Orders.Domain.Exceptions.Domain.OrderPaymentMessages;
using SD.OrderProcessing.Orders.Domain.Exceptions.Domain.Orders;
using SD.OrderProcessing.Orders.Domain.Exceptions.Infrastructure.Dal;
using SD.OrderProcessing.Orders.Domain.Models;
using SD.OrderProcessing.Orders.Domain.Models.Enums;
using SD.OrderProcessing.Orders.Domain.Services.Interfaces;

namespace SD.OrderProcessing.Orders.Domain.Services;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IOrderPaymentMessagesRepository _paymentMessagesRepository;

    public OrdersService(IOrdersRepository ordersRepository, IOrderPaymentMessagesRepository paymentMessagesRepository)
    {
        _ordersRepository = ordersRepository;
        _paymentMessagesRepository = paymentMessagesRepository;
    }

    public async Task<OrderModel> GetOrder(long orderId, CancellationToken cancellationToken)
    {
        try
        {
            return await GetOrderUnsafe(orderId, cancellationToken);
        }
        catch (EntityNotFoundException ex)
        {
            throw new OrderNotFoundException(
                message: $"Order with id: {orderId} couldn't be found.",
                orderId: orderId,
                innerException: ex
            );
        }
    }

    private async Task<OrderModel> GetOrderUnsafe(long orderId, CancellationToken cancellationToken)
    {
        using var transaction = _ordersRepository.CreateTransactionScope();

        var orderEntity = await _ordersRepository.GetById(
            orderId: orderId,
            cancellationToken: cancellationToken
        );

        transaction.Complete();

        return new OrderModel(
            Id: orderEntity.Id,
            UserId: orderEntity.UserId,
            Amount: orderEntity.Amount,
            Description: orderEntity.Description,
            Status: orderEntity.Status
        );
    }

    public async Task<IReadOnlyList<OrderModel>> GetUserOrders(long userId, CancellationToken cancellationToken)
    {
        using var transaction = _ordersRepository.CreateTransactionScope();

        var orderEntities = await _ordersRepository.GetAllForUser(
            userId: userId,
            cancellationToken: cancellationToken
        );

        transaction.Complete();

        return orderEntities.Select(e => new OrderModel(
            Id: e.Id,
            UserId: e.UserId,
            Amount: e.Amount,
            Description: e.Description,
            Status: e.Status
        )).ToArray();
    }


    public async Task<OrderModel> CreateOrder(long userId, decimal amount, string description,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            throw new OrderInvalidAmountException(
                message: $"Invalid order amount: {amount} for user with id: {userId}",
                invalidAmount: amount,
                userId: userId
            );
        }

        using var transaction = _ordersRepository.CreateTransactionScope();

        var createdOrdersIds = await _ordersRepository.Create(
            entities:
            [
                new OrderEntity
                {
                    UserId = userId,
                    Amount = amount,
                    Description = description
                }
            ],
            cancellationToken: cancellationToken
        );

        long createdOrderId = createdOrdersIds.Length == 0 ? -1 : createdOrdersIds[0];

        try
        {
            await _paymentMessagesRepository.Create(
                entities:
                [
                    new OrderPaymentMessageEntity
                    {
                        OrderId = createdOrderId,
                        UserId = userId,
                        Amount = amount,
                        CreatedAt = DateTimeOffset.UtcNow,
                    }
                ],
                cancellationToken: cancellationToken
            );
        }
        catch (EntityNotFoundException ex)
        {
            throw new PaymentMessageOrderIdNotFound(
                message: $"Invalid order id: {createdOrderId} passed as foreign key",
                orderId: createdOrderId,
                userId: userId,
                innerException: ex
            );
        }

        transaction.Complete();

        return new OrderModel(
            Id: createdOrderId,
            UserId: userId,
            Amount: amount,
            Description: description,
            Status: OrderStatus.Pending
        );
    }
}