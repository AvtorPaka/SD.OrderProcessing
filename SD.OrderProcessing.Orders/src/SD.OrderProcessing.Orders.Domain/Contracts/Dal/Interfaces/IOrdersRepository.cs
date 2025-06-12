using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Orders.Domain.Models.Enums;

namespace SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;

public interface IOrdersRepository: IDbRepository
{
    public Task<OrderEntity> GetById(long orderId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<OrderEntity>> GetAllForUser(long userId, CancellationToken cancellationToken);
    public Task<long[]> Create(OrderEntity[] entities, CancellationToken cancellationToken);
    public Task UpdateStatus(long orderId, OrderStatus newStatus, CancellationToken cancellationToken);
}