using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SD.OrderProcessing.Orders.Api.Extensions;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Entities;
using SD.OrderProcessing.Orders.Domain.Contracts.Dal.Interfaces;
using SD.OrderProcessing.Orders.Domain.Contracts.ISC.MessageQ.Messages;
using SD.OrderProcessing.Orders.Infrastructure.Configuration.Options;

namespace SD.OrderProcessing.Orders.Api.BackgroundServices;

public class OrderPaymentMessageProducer : BackgroundService
{
    private const string OrderPaymentQueueName = "ord_pay_mq";
    private const int MessagesPerProcessing = 50;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqConnectionOptions _mqConnectionOptions;
    private readonly ILogger<OrderPaymentMessageProducer> _logger;
    private IConnection? _rmqConnection;

    public OrderPaymentMessageProducer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqConnectionOptions> rmqConnectionOptions,
        ILogger<OrderPaymentMessageProducer> logger)
    {
        _serviceProvider = serviceProvider;
        _mqConnectionOptions = rmqConnectionOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await InitializeRmqConnection(cancellationToken);

        _logger.LogPaymentMessagesProducerStart(
            curTime: DateTime.UtcNow
        );

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    await ProcessPaymentMessages(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogPaymentMessagesProducesUnexpectedException(
                        curTime: DateTime.UtcNow,
                        exception: ex
                    );
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogPaymentMessagesProducerEnd(
                curTime: DateTime.UtcNow
            );
        }
        finally
        {
            await CleanupResourcesAsync(cancellationToken);
        }
    }

    private async Task ProcessPaymentMessages(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        IOrderPaymentMessagesRepository orderPaymentMessagesRepository =
            scope.ServiceProvider.GetRequiredService<IOrderPaymentMessagesRepository>();

        using var transaction = orderPaymentMessagesRepository.CreateTransactionScope();

        IReadOnlyList<OrderPaymentMessageEntity> paymentMessageEntities =
            await orderPaymentMessagesRepository.GetPriorPendingMessagesToPublishAndUpdate(
                limit: MessagesPerProcessing,
                cancellationToken: cancellationToken);


        if (paymentMessageEntities.Count != 0)
        {
            _logger.LogPaymentMessagesProducerStartProcessing(
                curTime: DateTime.UtcNow,
                messagesAmount: paymentMessageEntities.Count
            );

            long[] successFullSentOrderIds = await PublishMessages(paymentMessageEntities, cancellationToken);

            await orderPaymentMessagesRepository.MarkMessagesAsDone(
                messagesIds: successFullSentOrderIds,
                cancellationToken: cancellationToken
            );

            _logger.LogPaymentMessagesProducerEndProcessing(
                curTime: DateTime.UtcNow,
                messagesAmount: paymentMessageEntities.Count
            );
        }

        transaction.Complete();
    }

    private async Task<long[]> PublishMessages(IReadOnlyList<OrderPaymentMessageEntity> paymentMessageEntities,
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

        foreach (var messageEntity in paymentMessageEntities)
        {
            try
            {
                messageProps.MessageId = messageEntity.Id.ToString();

                var rawJsonMessage = JsonSerializer.Serialize(new OrderPaymentMessage(
                    UserOd: messageEntity.UserId,
                    OrderId: messageEntity.OrderId,
                    Amount: messageEntity.Amount
                ));
                var messageBody = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(rawJsonMessage));

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: OrderPaymentQueueName,
                    mandatory: true,
                    basicProperties: messageProps,
                    body: messageBody,
                    cancellationToken: cancellationToken
                );

                successFullSentOrderIds.Add(messageEntity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogPaymentMessagesProducerInvalidPublish(
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
            queue: OrderPaymentQueueName,
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