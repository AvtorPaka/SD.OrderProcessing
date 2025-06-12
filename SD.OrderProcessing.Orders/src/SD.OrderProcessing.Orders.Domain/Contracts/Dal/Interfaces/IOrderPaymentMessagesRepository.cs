using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;

namespace SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;

public interface IOrderPaymentMessagesRepository: IDbRepository
{
    public Task Create(OrderPaymentMessageEntity[] entities, CancellationToken cancellationToken);
    public Task<IReadOnlyList<OrderPaymentMessageEntity>> GetPriorPendingMessagesToPublishAndUpdate(int limit, CancellationToken cancellationToken);
    public Task MarkMessagesAsDone(long[] messagesIds, CancellationToken cancellationToken);
}