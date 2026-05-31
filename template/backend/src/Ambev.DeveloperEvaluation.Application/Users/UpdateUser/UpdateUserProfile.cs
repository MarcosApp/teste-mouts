using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserProfile : Profile
{
    public UpdateUserProfile()
    {
        CreateMap<User, UpdateUserResult>()
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
