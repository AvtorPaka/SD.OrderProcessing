using Microsoft.AspNetCore.Mvc.Filters;
using SD.OrderProcessing.Payment.Api.Extensions;
using SD.OrderProcessing.Payment.Api.Filters.Utils;
using SD.OrderProcessing.Payment.Domain.Exceptions.Domain.BalanceAccount;

namespace SD.OrderProcessing.Payment.Api.Filters;

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
            case BalanceAccountAlreadyExistsException ex:

                _logger.LogBalanceAccountAlreadyExists(
                    callId: callId,
                    curTime: DateTime.UtcNow,
                    userId: ex.UserId
                );

                ErrorRequestHandler.HandleAccountAlreadyExistsForUser(
                    context: context,
                    exception: ex
                );

                break;

            case BalanceAccountDepositBadRequestException ex:

                _logger.LogInvalidDepositAmountBadRequest(
                    callId: callId,
                    curTime: DateTime.UtcNow,
                    invalidAmount: ex.InvalidSum,
                    userId: ex.UserId
                );

                ErrorRequestHandler.HandleAccountDepositBadRequestError(
                    context: context,
                    exception: ex
                );

                break;


            case BalanceAccountNotFoundForUserException ex:

                _logger.LogBalanceAccountNotFoundForUser(
                    callId: callId,
                    curTime: DateTime.UtcNow,
                    userId: ex.UserId
                );

                ErrorRequestHandler.HandleUserBalanceAccountNotFoundError(
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