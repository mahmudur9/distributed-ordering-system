using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;

namespace PaymentService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPaymentMethodsFilter filter)
    {
        return Ok(await _paymentMethodService.GetAllPaymentMethodsAsync(filter));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _paymentMethodService.GetPaymentMethodByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(PaymentMethodRequest paymentMethodRequest)
    {
        await _paymentMethodService.CreatePaymentMethodAsync(paymentMethodRequest);
        return Ok("Payment method created");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, PaymentMethodRequest paymentMethodRequest)
    {
        await _paymentMethodService.UpdatePaymentMethodAsync(id, paymentMethodRequest);
        return Ok("Payment method updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _paymentMethodService.DeletePaymentMethodAsync(id);
        return Ok("Payment method deleted");
    }
}