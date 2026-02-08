using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.IServices;
using PaymentService.Application.Requests;

namespace PaymentService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentTypesController : ControllerBase
{
    private readonly IPaymentTypeService _paymentTypeService;

    public PaymentTypesController(IPaymentTypeService paymentTypeService)
    {
        _paymentTypeService = paymentTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPaymentTypesFilter  filter)
    {
        return Ok(await  _paymentTypeService.GetAllPaymentTypesAsync(filter));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await  _paymentTypeService.GetPaymentTypeByIdAsync(id));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(PaymentTypeRequest paymentTypeRequest)
    {
        await _paymentTypeService.CreatePaymentTypeAsync(paymentTypeRequest);
        return Ok("Payment type created");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, PaymentTypeRequest paymentTypeRequest)
    {
        await _paymentTypeService.UpdatePaymentTypeAsync(id, paymentTypeRequest);
        return Ok("Payment type updated");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _paymentTypeService.DeletePaymentTypeAsync(id);
        return Ok("Payment type deleted");
    }
}