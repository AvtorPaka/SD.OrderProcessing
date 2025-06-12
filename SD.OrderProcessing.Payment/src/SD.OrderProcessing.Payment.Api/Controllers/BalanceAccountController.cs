using Microsoft.AspNetCore.Mvc;
using SD.OrderProcessing.Payment.Api.Contracts.Requests.BalanceAccount;
using SD.OrderProcessing.Payment.Api.Contracts.Responses.BalanceAccount;
using SD.OrderProcessing.Payment.Api.Filters;
using SD.OrderProcessing.Payment.Domain.Services.Interfaces;

namespace SD.OrderProcessing.Payment.Api.Controllers;

[ApiController]
[Route("balance-account")]
public class BalanceAccountController : ControllerBase
{
    private readonly IBalanceAccountsService _balanceAccountsService;

    public BalanceAccountController(IBalanceAccountsService balanceAccountsService)
    {
        _balanceAccountsService = balanceAccountsService;
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType<CreateAccountResponse>(200)]
    [ErrorResponse(409)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken cancellation)
    {
        var accountModel = await _balanceAccountsService.CreateNew(
            userId: request.UserId,
            cancellationToken: cancellation
        );

        return Ok(new CreateAccountResponse(
            AccountId: accountModel.Id,
            Balance: accountModel.Balance
        ));
    }

    [HttpGet]
    [Route("get")]
    [ProducesResponseType<GetAccountBalanceResponse>(200)]
    [ErrorResponse(404)]
    public async Task<IActionResult> Get([FromQuery] GetAccountBalanceRequest request, CancellationToken cancellation)
    {
        var accountModel = await _balanceAccountsService.GetAccount(
            userId: request.UserId,
            cancellation: cancellation
        );

        return Ok(new GetAccountBalanceResponse(
            AccountId: accountModel.Id,
            Balance: accountModel.Balance
        ));
    }

    [HttpPatch]
    [Route("deposit")]
    [ProducesResponseType<AccountDepositResponse>(200)]
    [ErrorResponse(400)]
    [ErrorResponse(404)]
    public async Task<IActionResult> Deposit([FromBody] AccountDepositRequest request, CancellationToken cancellation)
    {
        await _balanceAccountsService.DepositSum(
            userId: request.UserId,
            depositSum: request.Amount,
            cancellation: cancellation
        );

        return Ok(new AccountDepositResponse());
    }
}