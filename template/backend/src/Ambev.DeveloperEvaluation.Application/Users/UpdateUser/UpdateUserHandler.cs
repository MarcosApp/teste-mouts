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
        user.Name = new UserName(command.Name.Firstname, command.Name.Lastname);
        user.Address = new Address(
            command.Address.City,
            command.Address.Street,
            command.Address.Number,
            command.Address.Zipcode,
            new Geolocation(command.Address.Geolocation.Lat, command.Address.Geolocation.Long));
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(command.Password))
            user.Password = _passwordHasher.HashPassword(command.Password);

        var updated = await _userRepository.UpdateAsync(user, cancellationToken);
        return _mapper.Map<UpdateUserResult>(updated);
    }
}
