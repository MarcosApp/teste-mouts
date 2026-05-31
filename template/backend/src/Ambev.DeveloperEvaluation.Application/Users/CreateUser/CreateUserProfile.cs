using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

/// <summary>
/// Profile for mapping between User entity and CreateUserResponse
/// </summary>
public class CreateUserProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateUser operation
    /// </summary>
    public CreateUserProfile()
    {
        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(s =>
                new UserName(s.Name.Firstname, s.Name.Lastname)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(s =>
                new Address(s.Address.City, s.Address.Street, s.Address.Number, s.Address.Zipcode,
                    new Geolocation(s.Address.Geolocation.Lat, s.Address.Geolocation.Long))));

        CreateMap<User, CreateUserResult>()
            .ForMember(d => d.Name, o => o.MapFrom(s => new UserNameResult
            {
                Firstname = s.Name.Firstname,
                Lastname = s.Name.Lastname
            }))
            .ForMember(d => d.Address, o => o.MapFrom(s => new UserAddressResult
            {
                City = s.Address.City,
                Street = s.Address.Street,
                Number = s.Address.Number,
                Zipcode = s.Address.Zipcode,
                Geolocation = new UserGeolocationResult { Lat = s.Address.Geolocation.Lat, Long = s.Address.Geolocation.Long }
            }));
    }
}
