namespace SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;

public class BalanceAccountEntity
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public decimal Balance { get; init; }
    public long Version { get; init; }
}