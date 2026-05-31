using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserHandler(IUserRepository userRepository, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UpdateUserResult> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID '{command.Id}' not found.");

        user.Username = command.Username;
        user.Email = command.Email;
        user.Phone = command.Phone;
        user.Status = command.Status;
        user.Role = command.Role;
        user.Name = new UserName(command.Firstname, command.Lastname);
        user.Address = new Address(command.City, command.Street, command.AddressNumber, command.Zipcode,
            new Geolocation(command.GeoLat, command.GeoLong));
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(command.Password))
            user.Password = _passwordHasher.HashPassword(command.Password);

        var updated = await _userRepository.UpdateAsync(user, cancellationToken);
        return _mapper.Map<UpdateUserResult>(updated);
    }
}
