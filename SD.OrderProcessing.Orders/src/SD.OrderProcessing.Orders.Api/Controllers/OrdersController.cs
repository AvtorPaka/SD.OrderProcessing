using Microsoft.AspNetCore.Mvc;
using SD.OrderProcessing.Orders.Api.Contracts.Requests.Orders;
using SD.OrderProcessing.Orders.Api.Contracts.Responses.Orders;
using SD.OrderProcessing.Orders.Api.Filters;
using SD.OrderProcessing.Orders.Domain.Services.Interfaces;

namespace SD.OrderProcessing.Orders.Api.Controllers;

[ApiController]
[Route("user-orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType<CreateOrderResponse>(200)]
    [ErrorResponse(400)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var orderModel = await _ordersService.CreateOrder(
            userId: request.UserId,
            amount: request.Amount,
            description: string.IsNullOrEmpty(request.Description) ? string.Empty : request.Description,
            cancellationToken: cancellationToken
        );

        return Ok(new CreateOrderResponse(
            OrderId: orderModel.Id,
            Status: orderModel.Status
        ));
    }

    [HttpGet]
    [Route("get")]
    [ProducesResponseType<GetOrderResponse>(200)]
    [ErrorResponse(404)]
    public async Task<IActionResult> GetOrder([FromQuery] GetOrderRequest request, CancellationToken cancellationToken)
    {
        var orderModel = await _ordersService.GetOrder(
            orderId: request.OrderId,
            cancellationToken: cancellationToken
        );

        return Ok(new GetOrderResponse(
            Id: orderModel.Id,
            UserId: orderModel.UserId,
            Amount: orderModel.Amount,
            Description: orderModel.Description,
            Status: orderModel.Status
        ));
    }

    [HttpGet]
    [Route("get-all")]
    [ProducesResponseType<IEnumerable<GetAllOrdersResponse>>(200)]
    public async Task<IActionResult> GetAllForUser([FromQuery] GetAllOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var orderModels = await _ordersService.GetUserOrders(
            userId: request.UserId,
            cancellationToken: cancellationToken
        );

        return Ok(orderModels.Select(m => new GetAllOrdersResponse(
            Id: m.Id,
            Amount: m.Amount,
            Description: m.Description,
            Status: m.Status
        )));
    }
}