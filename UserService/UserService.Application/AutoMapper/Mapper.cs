using AutoMapper;
using UserService.Application.Requests;
using UserService.Application.Responses;
using UserService.Domain.Models;

namespace UserService.Application.AutoMapper;

public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<UserRequest, User>();
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.RoleName, src => src.MapFrom(s => s.Role!.Name));
        
        CreateMap<Role,  RoleResponse>();
    }
}