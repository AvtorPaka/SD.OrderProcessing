using SD.OrderProcessing.Orders.Domain.Models;

namespace SD.OrderProcessing.Orders.Domain.Services.Interfaces;

public interface IOrdersService
{
    public Task<OrderModel> GetOrder(long orderId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<OrderModel>> GetUserOrders(long userId, CancellationToken cancellationToken);

    public Task<OrderModel> CreateOrder(long userId, decimal amount, string description,
        CancellationToken cancellationToken);
}