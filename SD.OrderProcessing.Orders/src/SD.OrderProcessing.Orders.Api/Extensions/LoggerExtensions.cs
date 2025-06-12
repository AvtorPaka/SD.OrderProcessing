namespace SD.OrderProcessing.Orders.Api.Extensions;

public static partial class LoggerExtensions
{
    #region Info

    [LoggerMessage(
        LogLevel.Information,
        EventId = 2000,
        Message = "[{CallId}] [{CurTime}] Start executing call. Endpoint: {EndpointRoute}"
    )]
    public static partial void LogRequestStart(this ILogger logger,
        DateTime curTime,
        string callId,
        string endpointRoute);


    [LoggerMessage(
        LogLevel.Information,
        EventId = 2001,
        Message = "[{CallId}] [{CurTime}] Ended executing call. Endpoint: {EndpointRoute}"
    )]
    public static partial void LogRequestEnd(this ILogger logger,
        DateTime curTime,
        string callId,
        string endpointRoute);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2002,
        Message = "[{CurTime}] Order Payment messages producer start executing."
    )]
    public static partial void LogPaymentMessagesProducerStart(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2003,
        Message = "[{CurTime}] Order Payment messages producer stopped executing."
    )]
    public static partial void LogPaymentMessagesProducerEnd(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2004,
        Message = "[{CurTime}] Order Payment messages producer RabbitMQ connection initialized."
    )]
    public static partial void LogProducerMqConnectionInitialized(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2005,
        Message = "[{CurTime}] Order Payment messages producer start processing {MessagesAmount} messages."
    )]
    public static partial void LogPaymentMessagesProducerStartProcessing(this ILogger logger,
        DateTime curTime,
        int messagesAmount);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2006,
        Message = "[{CurTime}] Order Payment messages producer ended processing {MessagesAmount} messages."
    )]
    public static partial void LogPaymentMessagesProducerEndProcessing(this ILogger logger,
        DateTime curTime,
        int messagesAmount);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2007,
        Message = "[{CurTime}] Payment status messages consumer RabbitMQ connection initialized."
    )]
    public static partial void LogConsumerMqConnectionInitialized(this ILogger logger,
        DateTime curTime);
    
    [LoggerMessage(
        LogLevel.Information,
        EventId = 2008,
        Message = "[{CurTime}] Payment status messages consumer stopped executing."
    )]
    public static partial void LogPaymentStatusConsumerEnd(this ILogger logger,
        DateTime curTime);

    #endregion

    #region Error
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4004,
        Message = "[{CurTime}] Unexpected exception occured during order payment messages processing."
    )]
    public static partial void LogPaymentMessagesProducesUnexpectedException(this ILogger logger,
        Exception exception,
        DateTime curTime);
    

    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4005,
        Message = "[{CurTime}] Unexpected exception occured during order payment message publish with id: {messageId}"
    )]
    public static partial void LogPaymentMessagesProducerInvalidPublish(this ILogger logger,
        Exception exception,
        DateTime curTime,
        long messageId);
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4006,
        Message = "[{CurTime}] Unexpected exception occured during order payment status message processing"
    )]
    public static partial void LogPaymentStatusConsumerUnexpectedException(this ILogger logger,
        Exception exception,
        DateTime curTime);
    
    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4000,
        Message = "[{CallId}] [{CurTime}] Unexpected exception occured during request processing."
    )]
    public static partial void LogInternalError(this ILogger logger,
        Exception exception,
        string callId,
        DateTime curTime);


    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 4001,
        Message =
            "[{CallId}] [{CurTime}] Order with id: {OrderId} not found."
    )]
    public static partial void LogOrderNotFoundError(this ILogger logger,
        string callId,
        DateTime curTime,
        long orderId
    );

    [LoggerMessage(
        LogLevel.Error,
        EventId = 4002,
        Message = "[{CallId}] [{CurTime}] Invalid order amount: {InvalidAmount} for account with user id: {UserId}"
    )]
    public static partial void LogInvalidOrderAmountError(this ILogger logger,
        string callId,
        DateTime curTime,
        decimal invalidAmount,
        long userId
    );
    
    [LoggerMessage(
        LogLevel.Error,
        EventId = 4003,
        Message = "[{CallId}] [{CurTime}] Unable to create order payment message for user with id: {UserId}. Order with id: {OrderId} doesnt exist."
    )]
    public static partial void LogOrderPaymentMessageInvalidOrder(this ILogger logger,
        string callId,
        DateTime curTime,
        long userId,
        long orderId
    );
    
    #endregion

    #region Debug

    [LoggerMessage(
        LogLevel.Debug,
        EventId = 1000,
        Message = "[{CallId}] [{CurTime}] Request headers:\n{Headers}"
    )]
    public static partial void LogRequestHeaders(this ILogger logger,
        DateTime curTime,
        string callId,
        string headers);

    #endregion
}