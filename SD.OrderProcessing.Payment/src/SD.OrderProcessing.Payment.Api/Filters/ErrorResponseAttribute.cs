using Microsoft.AspNetCore.Mvc;
using SD.OrderProcessing.Payment.Api.Contracts.Responses;

namespace SD.OrderProcessing.Payment.Api.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ErrorResponseAttribute: ProducesResponseTypeAttribute
{
    public ErrorResponseAttribute(int statusCode) : base(typeof(ErrorResponse), statusCode)
    {
    }
}