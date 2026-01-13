using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;

namespace PaymentService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPaymentsFilter filter)
    {
        return Ok(await _paymentService.GetAllPaymentsAsync(filter));
    }

    [HttpGet("GetById/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _paymentService.GetPaymentByIdAsync(id));
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await  _paymentService.DeletePaymentAsync(id);
        return Ok("Payment deleted");
    }
}