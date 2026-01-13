using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.IServices;
using OrderService.Application.Requests;

namespace OrderService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly Greeter.GreeterClient _greeterClient;

    public OrdersController(Greeter.GreeterClient greeterClient, IOrderService orderService)
    {
        _greeterClient = greeterClient;
        _orderService = orderService;
    }
    [HttpGet]
    public IActionResult Get()
    {
        var response = _greeterClient.SayHello(new HelloRequest { Name = "Akash" });
        
        return Ok(response.Message);
    }

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder(OrderRequest orderRequest)
    {
        await  _orderService.CreateOrderAsync(orderRequest);
        return Ok("Order created.");
    }
}