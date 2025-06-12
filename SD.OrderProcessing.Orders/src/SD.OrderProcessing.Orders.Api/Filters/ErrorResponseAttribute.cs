using Microsoft.AspNetCore.Mvc;
using SD.OrderProcessing.Orders.Api.Contracts.Responses;

namespace SD.OrderProcessing.Orders.Api.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ErrorResponseAttribute: ProducesResponseTypeAttribute
{
    public ErrorResponseAttribute(int statusCode) : base(typeof(ErrorResponse), statusCode)
    {
    }
}