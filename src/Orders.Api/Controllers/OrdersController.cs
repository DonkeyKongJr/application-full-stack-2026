using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderListItemDto>>> GetOrders(
        [FromQuery] OrderFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOpenOrdersAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Complete(
        int id,
        [FromBody] CompleteOrderRequest request)
    {
        await _orderService.CompleteAsync(id, request);
        return NoContent();
    }
}
