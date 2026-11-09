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
    public ActionResult<List<Order>> GetOrders([FromQuery] OrderFilterRequest filter)
    {
        var orders = _orderService.GetOpenOrders(filter);
        return Ok(orders);
    }

    [HttpPost("{id}/complete")]
    public ActionResult<Order> Complete(int id, [FromBody] CompleteOrderRequest request)
    {
        var order = _orderService.CompleteOrder(id, request);
        return Ok(order);
    }
}
