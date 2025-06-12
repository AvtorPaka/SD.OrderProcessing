using Microsoft.AspNetCore.Mvc.Filters;
using SD.OrderProcessing.Orders.Api.Extensions;
using SD.OrderProcessing.Orders.Api.Filters.Utils;
using SD.OrderProcessing.Orders.Domain.Exceptions.Domain.OrderPaymentMessages;
using SD.OrderProcessing.Orders.Domain.Exceptions.Domain.Orders;

namespace SD.OrderProcessing.Orders.Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        string callId = context.HttpContext.TraceIdentifier;

        switch (context.Exception)
        {
            case PaymentMessageOrderIdNotFound ex:

                _logger.LogOrderPaymentMessageInvalidOrder(
                    callId: callId,
                    curTime: DateTime.UtcNow,
                    userId: ex.UserId,
                    orderId: ex.OrderId
                );

                ErrorRequestHandler.HandleInternalError(context);
                break;

            case OrderNotFoundException ex:

                _logger.LogOrderNotFoundError(
                    callId: callId,
                    curTime: DateTime.UtcNow,
                    orderId: ex.OrderId
                );

                ErrorRequestHandler.HandleOrderNotFoundError(
                    context: context,
                    exception: ex
                );

                break;

            case OrderInvalidAmountException ex:

                _logger.LogInvalidOrderAmountError(
                    callId: callId,
                    curTime: DateTime.UtcNow,
                    invalidAmount: ex.InvalidOrderAmount,
                    userId: ex.UserId
                );

                ErrorRequestHandler.HandleOrderAmountBadRequestError(
                    context: context,
                    exception: ex
                );

                break;

            default:
                _logger.LogInternalError(
                    callId: callId,
                    curTime: DateTime.UtcNow,
                    exception: context.Exception
                );

                ErrorRequestHandler.HandleInternalError(context);

                break;
        }
    }
}