using AutoMapper;
using PaymentService.Application.Requests;
using PaymentService.Application.Responses;
using PaymentService.Domain.Models;

namespace PaymentService.Application.AutoMapper;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<PaymentTypeRequest, PaymentType>();
        CreateMap<PaymentType, PaymentTypeResponse>();

        CreateMap<PaymentMethodRequest, PaymentMethod>();
        CreateMap<PaymentMethod, PaymentMethodResponse>();

        CreateMap<PaymentRequest, Payment>();
        CreateMap<Payment, PaymentResponse>()
            .ForMember(dest => dest.PaymentTypeName, opt => opt.MapFrom(src => src.PaymentType.Name))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.Name));
    }
}