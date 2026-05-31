using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserProfile : Profile
{
    public UpdateUserProfile()
    {
        CreateMap<User, UpdateUserResult>()
            .ForMember(d => d.Firstname, o => o.MapFrom(s => s.Name.Firstname))
            .ForMember(d => d.Lastname, o => o.MapFrom(s => s.Name.Lastname));
    }
}
