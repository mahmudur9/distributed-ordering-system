using Microsoft.AspNetCore.Mvc;
using OrderService.Application.IServices;
using OrderService.Application.Requests;

namespace OrderService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder(OrderRequest orderRequest)
    {
        await _orderService.CreateOrderAsync(orderRequest);
        return Ok("Order created.");
    }
}