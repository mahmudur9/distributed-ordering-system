using AutoMapper;
using OrderService.Application.Requests;
using OrderService.Application.Responses;
using OrderService.Domain.Models;
using PaymentService.API;

namespace OrderService.Application.AutoMapper;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<OrderRequest, Order>();
        CreateMap<Order, OrderResponse>();
        CreateMap<ProductOrderRequest, ProductOrder>();
        CreateMap<ProductOrder, ProductOrderResponse>();
    }
}