using AutoMapper;
using ECommerceApp.DTOs.Requests.Auth;
using ECommerceApp.DTOs.Responses.Auth;
using ECommerceApp.DTOs.Responses.User;
using ECommerceApp.Models;

public class UserMappingProfile : Profile
{
    public UserMappingProfile(){
        CreateMap<CreateUserRequest, User>();
        CreateMap<User, AuthUserResponse>();
        CreateMap<User, UserDetailsResponse>();
    }
}