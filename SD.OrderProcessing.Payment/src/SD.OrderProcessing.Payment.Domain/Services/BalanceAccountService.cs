using Microsoft.Extensions.Logging;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Payment.Domain.Exceptions.Domain.BalanceAccount;
using SD.OrderProcessing.Payment.Domain.Exceptions.Infrastructure.Dal;
using SD.OrderProcessing.Payment.Domain.Models;
using SD.OrderProcessing.Payment.Domain.Models.Enums;
using SD.OrderProcessing.Payment.Domain.Services.Interfaces;

namespace SD.OrderProcessing.Payment.Domain.Services;

public class BalanceAccountService : IBalanceAccountsService
{
    private readonly IBalanceAccountRepository _balanceAccountRepository;
    private readonly IBalanceWithdrawUpdatesRepository _balanceWithdrawUpdatesRepository;
    private readonly IPaymentStatusMessagesRepository _paymentStatusMessagesRepository;
    private readonly ILogger<BalanceAccountService> _logger;

    public BalanceAccountService(
        IBalanceAccountRepository balanceAccountRepository,
        IBalanceWithdrawUpdatesRepository balanceWithdrawUpdatesRepository,
        IPaymentStatusMessagesRepository paymentStatusMessagesRepository,
        ILogger<BalanceAccountService> logger
    )
    {
        _balanceAccountRepository = balanceAccountRepository;
        _balanceWithdrawUpdatesRepository = balanceWithdrawUpdatesRepository;
        _paymentStatusMessagesRepository = paymentStatusMessagesRepository;
        _logger = logger;
    }

    public async Task<BalanceAccountModel> CreateNew(long userId, CancellationToken cancellationToken)
    {
        try
        {
            return await CreateNewUnsafe(userId, cancellationToken);
        }
        catch (EntityAlreadyExistsException ex)
        {
            throw new BalanceAccountAlreadyExistsException(
                message: $"Balance account for user with id: {userId} already exists.",
                userId: userId,
                innerException: ex
            );
        }
    }

    private async Task<BalanceAccountModel> CreateNewUnsafe(long userId, CancellationToken cancellation)
    {
        using var transaction = _balanceAccountRepository.CreateTransactionScope();

        long[] createdIds = await _balanceAccountRepository.CreateNew(
            entities:
            [
                new BalanceAccountEntity
                {
                    Balance = 0.0m,
                    UserId = userId,
                    Version = 1
                }
            ],
            cancellation: cancellation
        );

        transaction.Complete();

        return new BalanceAccountModel(
            Id: createdIds[0],
            UserId: userId,
            Balance: 0,
            Version: 1
        );
    }

    public async Task DepositSum(long userId, decimal depositSum, CancellationToken cancellation)
    {
        try
        {
            await DepositSumUnsafe(userId, depositSum, cancellation);
        }
        catch (EntityNotFoundException ex)
        {
            throw new BalanceAccountNotFoundForUserException(
                message: $"Balance account for user with id: {userId} couldn't be found.",
                userId: userId,
                innerException: ex
            );
        }
    }

    private async Task DepositSumUnsafe(long userId, decimal depositSum, CancellationToken cancellation)
    {
        if (depositSum <= 0)
        {
            throw new BalanceAccountDepositBadRequestException(
                message: $"Invalid deposit sum: {depositSum} for account with user id: {userId}",
                userId: userId,
                invalidSum: depositSum
            );
        }

        using var transaction = _balanceAccountRepository.CreateTransactionScope();

        await UpdateBalance(userId, depositSum, cancellation);

        transaction.Complete();
    }

    public async Task ProcessPaymentOperations(int limit, CancellationToken cancellationToken)
    {
        if (limit <= 0)
        {
            throw new ArgumentException($"Invalid limit parameter: {limit} for withdraw operations processing");
        }

        IReadOnlyList<BalanceWithdrawUpdateEntity> withdrawOperations =
            await _balanceWithdrawUpdatesRepository.GetPriorPendingOperationsToUpdate(
                limit: limit,
                cancellationToken: cancellationToken
            );

        if (withdrawOperations.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "[{CurTime}] Withdraw payment operations processor start processing: {numberOfPayments} payments.",
            DateTime.UtcNow, withdrawOperations.Count);

        using var transaction = _balanceWithdrawUpdatesRepository.CreateTransactionScope();

        List<PaymentStatusMessageEntity> paymentStatusMessages = [];

        foreach (var withdraw in withdrawOperations)
        {
            OrderStatus paymentStatus = await ProcessWithdraw(
                userId: withdraw.UserId,
                sum: withdraw.Amount,
                cancellationToken: cancellationToken
            );

            paymentStatusMessages.Add(new PaymentStatusMessageEntity
            {
                OrderId = withdraw.OrderId,
                OrderStatus = paymentStatus,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }


        await _paymentStatusMessagesRepository.Create(
            entities: paymentStatusMessages.ToArray(),
            cancellationToken: cancellationToken
        );

        await _balanceWithdrawUpdatesRepository.MarkOperationsDone(
            operationIds: withdrawOperations.Select(e => e.Id).ToArray(),
            cancellationToken: cancellationToken
        );

        transaction.Complete();
        
        _logger.LogInformation(
            "[{CurTime}] Withdraw payment operations processor ended processing: {numberOfPayments} payments.",
            DateTime.UtcNow, withdrawOperations.Count);
    }

    private async Task<OrderStatus> ProcessWithdraw(long userId, decimal sum, CancellationToken cancellationToken)
    {
        try
        {
            if (sum < 0)
            {
                return OrderStatus.Insufficient_Funds;
            }

            await UpdateBalance(
                userId: userId,
                sum: -sum,
                cancellation: cancellationToken
            );
            return OrderStatus.Finished;
        }
        catch (BalanceInsufficientFundsException)
        {
            return OrderStatus.Insufficient_Funds;
        }
        catch (EntityNotFoundException)
        {
            return OrderStatus.Balance_Not_Exists;
        }
        catch (Exception)
        {
            return OrderStatus.Failed;
        }
    }

    private async Task UpdateBalance(long userId, decimal sum, CancellationToken cancellation)
    {
        var casEntity = await _balanceAccountRepository.GetByUserId(
            userId: userId,
            isForUpdate: false,
            cancellationToken: cancellation
        );

        var affectedRows = await _balanceAccountRepository.CasUpdateBalance(
            userId: userId,
            curVersion: casEntity.Version,
            amount: sum,
            cancellationToken: cancellation
        );

        if (affectedRows == 0)
        {
            var lockedEntity = await _balanceAccountRepository.GetByUserId(
                userId: userId,
                isForUpdate: true,
                cancellationToken: cancellation
            );

            if (lockedEntity.Balance + sum < 0)
            {
                throw new BalanceInsufficientFundsException(
                    message: "Insufficient funds for account balance"
                );
            }

            await _balanceAccountRepository.UpdateBalance(
                userId: userId,
                amount: sum,
                cancellation: cancellation
            );
        }
    }


    public async Task<BalanceAccountModel> GetAccount(long userId, CancellationToken cancellation)
    {
        try
        {
            return await GetAccountUnsafe(userId, cancellation);
        }
        catch (EntityNotFoundException ex)
        {
            throw new BalanceAccountNotFoundForUserException(
                message: $"Balance account for user with id: {userId} couldn't be found.",
                userId: userId,
                innerException: ex
            );
        }
    }

    private async Task<BalanceAccountModel> GetAccountUnsafe(long userId, CancellationToken cancellation)
    {
        using var transaction = _balanceAccountRepository.CreateTransactionScope();

        var entity = await _balanceAccountRepository.GetByUserId(
            userId: userId,
            isForUpdate: false,
            cancellationToken: cancellation
        );

        transaction.Complete();

        return new BalanceAccountModel(
            Id: entity.Id,
            UserId: entity.UserId,
            Balance: entity.Balance,
            Version: entity.Version
        );
    }
}