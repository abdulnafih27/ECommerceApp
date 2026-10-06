using AutoMapper;

public class UserMappingProfile : Profile
{
    public UserMappingProfile(){
        CreateMap<CreateUserRequest, User>();
        CreateMap<User, AuthUserResponse>();
        CreateMap<User, GetUserResponse>();
        CreateMap<User, UpdateUserResponse>();

        // Owner Request Mapping to the user
        CreateMap<RegisterOwnerRequest, User>();
    }
}