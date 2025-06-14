using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SD.OrderProcessing.Payment.Api.Extensions;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Payment.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Payment.Domain.Contracts.ISC.MessageQ.Messages;
using SD.OrderProcessing.Payment.Infrastructure.Configuration.Options;

namespace SD.OrderProcessing.Payment.Api.BackgroundServices;

public class PaymentStatusMessagesProducer : BackgroundService
{
    private const string PaymentStatusQueueName = "pay_status_mq";
    private const int MessagesPerProcessing = 50;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqConnectionOptions _mqConnectionOptions;
    private readonly ILogger<PaymentStatusMessagesProducer> _logger;
    private IConnection? _rmqConnection;

    public PaymentStatusMessagesProducer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqConnectionOptions> connectionOptions,
        ILogger<PaymentStatusMessagesProducer> logger
    )
    {
        _serviceProvider = serviceProvider;
        _mqConnectionOptions = connectionOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await InitializeRmqConnection(cancellationToken);

        _logger.LogPaymentStatusMessagesProducerStart(
            curTime: DateTime.UtcNow
        );

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    await ProcessPaymentStatusMessages(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogPaymentStatusMessagesProducesUnexpectedException(
                        curTime: DateTime.UtcNow,
                        exception: ex
                    );
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogPaymentStatusMessagesProducerEnd(
                curTime: DateTime.UtcNow
            );
        }
        finally
        {
            await CleanupResourcesAsync(cancellationToken);
        }
    }
    
    private async Task ProcessPaymentStatusMessages(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        IPaymentStatusMessagesRepository paymentStatusMessagesRepository =
            scope.ServiceProvider.GetRequiredService<IPaymentStatusMessagesRepository>();

        using var transaction = paymentStatusMessagesRepository.CreateTransactionScope();

        IReadOnlyList<PaymentStatusMessageEntity> paymentMessageEntities =
            await paymentStatusMessagesRepository.GetPriorPendingMessagesToUpdateAndPublish(
                limit: MessagesPerProcessing,
                cancellationToken: cancellationToken);


        if (paymentMessageEntities.Count != 0)
        {
            _logger.LogPaymentStatusMessagesProducerStartProcessing(
                curTime: DateTime.UtcNow,
                messagesAmount: paymentMessageEntities.Count
            );

            long[] successFullSentOrderIds = await PublishMessages(paymentMessageEntities, cancellationToken);

            await paymentStatusMessagesRepository.MarkMessagesDone(
                messagesIds: successFullSentOrderIds,
                cancellationToken: cancellationToken
            );

            _logger.LogPaymentStatusMessagesProducerEndProcessing(
                curTime: DateTime.UtcNow,
                messagesAmount: paymentMessageEntities.Count
            );
        }

        transaction.Complete();
    }

    private async Task<long[]> PublishMessages(IReadOnlyList<PaymentStatusMessageEntity> paymentStatusMessageEntities,
        CancellationToken cancellationToken)
    {
        await using var channel = await OpenChannelWithQueueAsync(cancellationToken);
        List<long> successFullSentOrderIds = [];

        var messageProps = new BasicProperties
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            Persistent = true,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        foreach (var messageEntity in paymentStatusMessageEntities)
        {
            try
            {
                messageProps.MessageId = messageEntity.Id.ToString();

                var rawJsonMessage = JsonSerializer.Serialize(new PaymentStatusMessage(
                    OrderId: messageEntity.OrderId,
                    Status: messageEntity.OrderStatus
                ));
                var messageBody = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(rawJsonMessage));

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: PaymentStatusQueueName,
                    mandatory: true,
                    basicProperties: messageProps,
                    body: messageBody,
                    cancellationToken: cancellationToken
                );

                successFullSentOrderIds.Add(messageEntity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogPaymentStatusMessagesProducerInvalidPublish(
                    curTime: DateTime.UtcNow,
                    exception: ex,
                    messageId: messageEntity.Id
                );
            }
        }

        return successFullSentOrderIds.ToArray();
    }

    private async Task InitializeRmqConnection(CancellationToken cancellationToken)
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = _mqConnectionOptions.HostName,
            UserName = _mqConnectionOptions.UserName,
            Password = _mqConnectionOptions.Password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        _rmqConnection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        _logger.LogProducerMqConnectionInitialized(
            curTime: DateTime.UtcNow
        );
    }

    private async Task<IChannel> OpenChannelWithQueueAsync(CancellationToken cancellationToken)
    {
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true,
            outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(256)
        );

        IChannel rmqChannel = await _rmqConnection!.CreateChannelAsync(channelOptions, cancellationToken);

        await rmqChannel.QueueDeclareAsync(
            queue: PaymentStatusQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        return rmqChannel;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await CleanupResourcesAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private async Task CleanupResourcesAsync(CancellationToken cancellationToken)
    {
        if (_rmqConnection != null)
        {
            await _rmqConnection.CloseAsync(cancellationToken);
            _rmqConnection.Dispose();
            _rmqConnection = null;
        }
    }
}