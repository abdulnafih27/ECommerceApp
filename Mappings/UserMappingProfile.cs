using AutoMapper;

public class UserMappingProfile : Profile
{
    public UserMappingProfile(){
        CreateMap<CreateUserRequest, User>();
        CreateMap<User, AuthUserResponse>();
        CreateMap<User, UserDetailsResponse>();
    }
}