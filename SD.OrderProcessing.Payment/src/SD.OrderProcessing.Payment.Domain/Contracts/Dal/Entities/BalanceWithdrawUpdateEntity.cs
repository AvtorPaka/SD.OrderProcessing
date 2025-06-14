using SD.OrderProcessing.Payment.Domain.Models.Enums;

namespace SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;

public class BalanceWithdrawUpdateEntity
{
    public long Id { get; init; }
    public long OrderId { get; init; }
    public long UserId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public MessageState State { get; init; }
}