using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.IServices;
using OrderService.Application.Requests;
using OrderService.Application.Responses;

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

    [Authorize(Roles = "User")]
    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder(OrderRequest orderRequest)
    {
        await _orderService.CreateOrderAsync(orderRequest);
        return Ok("Order created.");
    }

    [HttpGet("GetAll")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllOrdersFilter filters)
    {
        return Ok(await _orderService.GetAllOrdersAsync(filters));
    }
    
    [Authorize(Roles = "User")]
    [HttpGet("GetAllByUserId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllByUserId([FromQuery] GetAllOrdersFilter filters)
    {
        return Ok(await _orderService.GetAllOrdersByUserIdAsync(filters));
    }
}