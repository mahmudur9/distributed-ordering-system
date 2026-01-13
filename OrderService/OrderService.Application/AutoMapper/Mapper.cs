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
        
        CreateMap<PaymentRequest, CreatePaymentGrpcRequest>()
            .ForMember(dest => dest.PaymentMethodId, opt => opt.MapFrom(src => src.PaymentMethodId == null ? "" : src.PaymentMethodId.ToString()));
    }
}