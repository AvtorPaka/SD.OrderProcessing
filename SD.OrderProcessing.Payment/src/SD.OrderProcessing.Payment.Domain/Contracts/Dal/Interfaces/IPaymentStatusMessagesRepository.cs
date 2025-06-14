using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;

namespace SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;

public interface IPaymentStatusMessagesRepository: IDbRepository
{
    public Task Create(PaymentStatusMessageEntity[] entities, CancellationToken cancellationToken);
    public Task<IReadOnlyList<PaymentStatusMessageEntity>> GetPriorPendingMessagesToUpdateAndPublish(int limit,
        CancellationToken cancellationToken);

    public Task MarkMessagesDone(long[] messagesIds, CancellationToken cancellationToken);
}