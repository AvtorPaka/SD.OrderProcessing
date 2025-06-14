using SD.OrderProcessing.Payment.Api.Extensions;
using SD.OrderProcessing.Payment.Domain.Services.Interfaces;

namespace SD.OrderProcessing.Payment.Api.BackgroundServices;

public class AccountWithdrawOperationsProcessor : BackgroundService
{
    private const int OperationsPerProcess = 50;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AccountWithdrawOperationsProcessor> _logger;

    public AccountWithdrawOperationsProcessor(IServiceProvider serviceProvider,
        ILogger<AccountWithdrawOperationsProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogWithdrawOperationsProcessorStart(
            curTime: DateTime.UtcNow
        );

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await ProcessPaymentOperations(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWithdrawOperationsProcessorStop(
                curTime: DateTime.UtcNow
            );
        }
    }

    private async Task ProcessPaymentOperations(CancellationToken cancellationToken)
    {
        _logger.LogWithdrawOperationsProcessorStartProcessing(
            curTime: DateTime.UtcNow
        );

        await using var scope = _serviceProvider.CreateAsyncScope();

        IBalanceAccountsService balanceAccountsService =
            scope.ServiceProvider.GetRequiredService<IBalanceAccountsService>();

        try
        {
            await balanceAccountsService.ProcessPaymentOperations(
                limit: OperationsPerProcess,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogWithdrawOperationsProcessorUnexpectedError(
                curTime: DateTime.UtcNow,
                exception: ex
            );
        }

        _logger.LogWithdrawOperationsProcessorStopProcessing(
            curTime: DateTime.UtcNow
        );
    }
}