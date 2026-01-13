using AutoMapper;
using ProductService.Application.Requests;
using ProductService.Application.Responses;
using ProductService.Domain.Models;

namespace ProductService.Application.AutoMapper;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<CategoryRequest, Category>();
        CreateMap<Category, CategoryResponse>();
        
        CreateMap<ProductRequest, Product>();
        CreateMap<Product, ProductResponse>();
    }
}