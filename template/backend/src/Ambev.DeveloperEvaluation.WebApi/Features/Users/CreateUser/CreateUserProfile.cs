using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;

/// <summary>
/// Profile for mapping between Application and API CreateUser responses
/// </summary>
public class CreateUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateUser feature
    /// </summary>
    public CreateUserProfile()
    {
        CreateMap<CreateUserRequest, CreateUserCommand>();
        CreateMap<UserNameCommand, UserNameCommand>();
        CreateMap<UserAddressCommand, UserAddressCommand>();
        CreateMap<UserGeolocationCommand, UserGeolocationCommand>();
        CreateMap<CreateUserResult, CreateUserResponse>();
    }
}
